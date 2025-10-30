using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro; // 1. (ของใหม่) ต้องเพิ่มบรรทัดนี้สำหรับ TextMeshPro

public class PlayerInteract : MonoBehaviour
{
    // --- (ของเดิม) Fade Effect ---
    [Header("Fade Effect")]
    public Animator fadeAnimator;
    public float fadeDuration = 0.5f;

    // --- (ของเดิม) Flashlight State ---
    [Header("Flashlight State")]
    public bool hasFlashlight = false;
    private bool isFlashlightOn = false;
    public GameObject flashlightObject;

    // --- (ของใหม่) Audio Settings ---
    [Header("Audio Settings")]
    public AudioSource interactAudioSource;  // AudioSource สำหรับเสียงโต้ตอบ
    public AudioClip hideSound;              // เสียงเข้าตู้
    public AudioClip unhideSound;            // เสียงออกจากตู้

    // --- (ของใหม่) UI Item Info ---
    [Header("Item Info UI")]
    [Tooltip("ลาก Panel พื้นหลังของ UI มาใส่")]
    public GameObject itemInfoPanel;
    [Tooltip("ลาก Text (TextMeshPro) ที่ใช้แสดงข้อความมาใส่")]
    public TextMeshProUGUI itemInfoText;
    private bool isDisplayingItemInfo = false; // สถานะกำลังแสดง UI

    // --- (ของเดิม) ระบบตู้ ---
    private Interactable currentInteractable;
    private bool isHiding = false;
    private GameObject currentLocker;
    private Vector3 originalPositionBeforeHiding;

    // --- (ของเดิม) กระเป๋า (Inventory) ---
    public List<string> inventory = new List<string>();

    // --- (ของเดิม) Components ---
    [Header("Component References")]
    public CameraFollow mainCameraFollow;
    public KeypadController keypadController;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    
    // Reference to the QTE manager (can be assigned in Inspector). If not set, we auto-find it in Start().
    public QTEManager qteManager;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Auto-assign QTEManager if it wasn't set in the Inspector
        if (qteManager == null)
        {
            qteManager = FindObjectOfType<QTEManager>();
            if (qteManager != null)
            {
                Debug.Log("QTEManager auto-assigned in PlayerInteract: " + qteManager.gameObject.name);
            }
        }
        
        // --- (ของใหม่) ตั้งค่าสถานะไฟฉายเริ่มต้น ---
        hasFlashlight = false;
        isFlashlightOn = false;
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }

        // (ของใหม่) ตรวจสอบให้แน่ใจว่า UI Item Info ถูกปิดอยู่ตอนเริ่ม
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(false);
        }
        if (itemInfoText != null)
        {
            itemInfoText.gameObject.SetActive(false);
        }

        // --- (ของใหม่) ตั้งค่า Audio Source ---
        if (interactAudioSource == null)
        {
            interactAudioSource = gameObject.AddComponent<AudioSource>();
            interactAudioSource.playOnAwake = false;
            interactAudioSource.loop = false;
            Debug.LogWarning("สร้าง AudioSource สำหรับเสียงโต้ตอบอัตโนมัติ");
        }

        if (keypadController != null)
        {
            keypadController.OnSuccess.AddListener(OnKeypadSuccess);
            keypadController.OnClose.AddListener(OnKeypadClose);
        }
    }

    // --- (ของเดิม) OnTriggerEnter2D / OnTriggerExit2D ---
    void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            Debug.Log("อยู่ใกล้วัตถุ: " + interactable.type);
            if (interactable.interactPrompt != null)
            {
                interactable.interactPrompt.SetActive(true);
            }
            if (interactable.lockedPrompt != null)
            {
                interactable.lockedPrompt.SetActive(false);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            if (interactable.interactPrompt != null)
            {
                interactable.interactPrompt.SetActive(false);
            }
            if (interactable.lockedPrompt != null)
            {
                interactable.lockedPrompt.SetActive(false);
            }
            currentInteractable = null;
        }
    }

    // --- (อัปเกรด) แก้ไข Update ให้รองรับ UI Item Info ---
    void Update()
    {
        // 1. (ของใหม่) ถ้ากำลังแสดง UI Item Info, จะรับอินพุตแค่คลิกซ้าย
        if (isDisplayingItemInfo)
        {
            // ตรวจสอบการคลิกซ้าย
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HideItemInfo(); // ซ่อน UI
            }
            return; // หยุดการทำงานของ Update ที่เหลือทั้งหมด
        }

        // 2. (ของเดิม) ตรวจสอบการกด 'E' (Interact)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isHiding)
            {
                // (อัปเกรด) เราต้องเปลี่ยน UnHide() เป็น Coroutine (ถ้ามันมี Animation)
                // แต่โค้ดของคุณยังเป็น void อยู่ เราจะเรียก void UnHide() ตามเดิม
                UnHide();
            }
            else if (currentInteractable != null)
            {
                DoInteraction();
            }
        }

        // 3. (ของเดิม) ตรวจสอบการกด 'G' (Toggle Flashlight)
        if (hasFlashlight && !isHiding && Keyboard.current.gKey.wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
    }

    // --- (ของเดิม) ฟังก์ชันสำหรับเปิด/ปิดไฟฉาย ---
    void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        Debug.Log("ไฟฉาย: " + isFlashlightOn);
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(isFlashlightOn);
        }
    }

    // --- (อัปเกรด) DoInteraction จะจัดการ Coroutine สำหรับ Collectable ---
    void DoInteraction()
    {
        switch (currentInteractable.type)
        {
            case InteractionType.Collectable:
                // (อัปเกรด) เรียก Coroutine สำหรับเก็บไอเทม
                StartCoroutine(CollectItemProcess(currentInteractable));
                break;
            case InteractionType.Hideable:
                currentLocker = currentInteractable.gameObject;
                // (โค้ดของคุณยังเป็น void Hide() เราจะเรียกตามนั้น)
                Hide();
                break;
            case InteractionType.Door:
                CheckDoor(currentInteractable); // (CheckDoor เรียก Coroutine ของมันเองอยู่แล้ว)
                break;

            case InteractionType.KeypadDoor:
                CheckKeypadDoor(currentInteractable);
                break;
        }
    }

    // --- (อัปเกรด) เปลี่ยน CollectItem เป็น Coroutine ---
    IEnumerator CollectItemProcess(Interactable item)
    {
        // 1. หยุดผู้เล่น
        playerMovement.enabled = false;

        if (item.isLockedByPrerequisite && !inventory.Contains(item.requiredItemID))
        {
            Debug.Log("เก็บไม่ได้! ต้องมี " + item.requiredItemID + " ก่อน");

            // แสดง UI "ล็อก" (เหมือนประตู)
            if (item.lockedPrompt != null)
            {
                item.lockedPrompt.SetActive(true);
            }

            // คืนการควบคุมให้ผู้เล่น และ *หยุด* Coroutine นี้ทันที
            playerMovement.enabled = true;
            yield break; // <-- ออกจาก Coroutine
        }

        // 2. เก็บไอเทม (โลจิกเดิมจาก CollectItem)
        string collectedItemID = item.itemID;
        Debug.Log("เก็บ " + collectedItemID);

        if (collectedItemID == "Flashlight")
        {
            hasFlashlight = true;
            isFlashlightOn = true;
            if (flashlightObject != null)
            {
                flashlightObject.SetActive(true);
            }
        }
        else
        {
            inventory.Add(collectedItemID);
        }

        // 3. ทำลายไอเทมที่พื้น
        Destroy(item.gameObject);
        currentInteractable = null;

        // 4. แสดง UI Item Info
        if (itemInfoPanel != null && itemInfoText != null)
        {
            itemInfoText.text = item.itemDescription; // ใส่ข้อความจาก Interactable
            itemInfoPanel.SetActive(true);
            itemInfoText.gameObject.SetActive(true);
            isDisplayingItemInfo = true; // ตั้งค่าสถานะว่ากำลังแสดง UI
        }
        else
        {
            // ถ้า UI ไม่พร้อม ก็คืนการควบคุมผู้เล่นเลย
            Debug.LogWarning("Item Info UI ไม่ได้ถูกตั้งค่าใน PlayerInteract!");
            playerMovement.enabled = true;
        }

        // 5. รอจนกว่าผู้เล่นจะปิด UI
        // (Update จะเช็ก isDisplayingItemInfo และรอคลิกซ้าย)
        while (isDisplayingItemInfo)
        {
            yield return null; // รอ 1 เฟรม แล้วเช็กใหม่
        }

        // 6. เมื่อ UI ปิดแล้ว (HideItemInfo ถูกเรียก) ก็คืนการควบคุมผู้เล่น
        playerMovement.enabled = true;
    }

    // (ของใหม่) ฟังก์ชันสำหรับซ่อน UI Item Info (ถูกเรียกโดย Update)
    void HideItemInfo()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(false);
        }
        if (itemInfoText != null)
        {
            itemInfoText.gameObject.SetActive(false);
        }
        isDisplayingItemInfo = false; // ตั้งค่าสถานะว่า UI ปิดแล้ว
    }


    // --- (ของเดิม) ฟังก์ชันสำหรับเช็กประตู ---
    void CheckDoor(Interactable door)
    {
        if (!door.isLocked)
        {
            Debug.Log("ประตูไม่ได้ล็อก วาร์ปเลย...");
            StartCoroutine(WarpTransition(door));
            return;
        }
        if (inventory.Contains(door.requiredKeyID))
        {
            Debug.Log("พบกุญแจ! ทำการปลดล็อกและวาร์ป...");
            door.Unlock();
            StartCoroutine(WarpTransition(door));
        }
        else
        {
            Debug.Log("ไม่มีกุญแจ! ประตูยังคงล็อกอยู่");
        }
        if (door.lockedPrompt != null)
        {
            door.lockedPrompt.SetActive(true);
        }
    }

    // --- (ของเดิม) Coroutine สำหรับ WarpTransition ---
    IEnumerator WarpTransition(Interactable door)
    {
        playerMovement.enabled = false;
        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("StartFadeOut");
        }
        yield return new WaitForSeconds(fadeDuration);

        WarpPlayer(door); // เรียกฟังก์ชันวาร์ปเดิม

        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("StartFadeIn");
        }
        yield return new WaitForSeconds(fadeDuration);

        playerMovement.enabled = true;
    }

    // --- (ของเดิม) ระบบวาร์ป ---
    void WarpPlayer(Interactable door)
    {
        if (door.warpTarget != null)
        {
            Debug.Log("วาร์ปไปที่: " + door.warpTarget.name);
            if (mainCameraFollow != null && door.targetMapBounds != null)
            {
                mainCameraFollow.mapBounds = door.targetMapBounds;
            }
            else
            {
                Debug.LogWarning("ประตูนี้ไม่ได้ตั้งค่า Target Map Bounds, กล้องอาจจะติดขอบเขตเก่า!");
            }
            this.transform.position = door.warpTarget.position;
            if (mainCameraFollow != null)
            {
                mainCameraFollow.SnapToTargetPosition(this.transform.position);
            }
        }
        else
        {
            Debug.LogError("ยังไม่ได้ตั้งค่า Warp Target สำหรับประตูนี้!");
        }
    }

    // --- (ของเดิม) ระบบตู้ ---
    void Hide()
    {
        Debug.Log("กำลังซ่อนตัว!");
        
        // === เล่นเสียงเข้าตู้ ===
        PlaySound(hideSound);
        
        isHiding = true;
        playerMovement.enabled = false;
        // Stop physics movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        spriteRenderer.enabled = false;

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }

        if (currentInteractable.interactPrompt != null)
        {
            currentInteractable.interactPrompt.SetActive(false);
        }

        originalPositionBeforeHiding = transform.position;
        // (โค้ดเดิมของคุณย้ายไปที่ "กลางตู้" ผมจะเก็บไว้อย่างนั้น)
        transform.position = currentLocker.transform.position;
        // เมื่อซ่อน ให้เริ่ม QTE UI ถ้ามี QTEManager
        if (qteManager != null)
        {
            Debug.Log("🎯 PlayerInteract: เรียก StartQTE() เมื่อซ่อน");
            qteManager.StartQTE();
        }
        else
        {
            Debug.LogWarning("QTEManager ไม่ได้ตั้งค่าใน PlayerInteract — QTE จะไม่เริ่ม");
        }
    }

    void UnHide()
    {
        Debug.Log("ออกจากที่ซ่อน!");
        
        // === เล่นเสียงออกจากตู้ ===
        PlaySound(unhideSound);
        
        isHiding = false;

        // (คอมเมนต์ QTE ของคุณ)
        //if (qteManager != null)
        //{
        //    Debug.Log("🛑 PlayerInteract: กำลังเรียก StopQTE()");
        //    qteManager.StopQTE();
        //}
        //else
        //{
        //    Debug.LogError("❌ QTEManager ไม่พบ! ไม่สามารถหยุด QTE ได้");
        //}

        playerMovement.enabled = true;
        spriteRenderer.enabled = true;

        if (flashlightObject != null && isFlashlightOn)
        {
            flashlightObject.SetActive(true);
        }

        transform.position = originalPositionBeforeHiding; // ย้ายกลับที่เดิม

        if (currentInteractable != null && currentInteractable.type == InteractionType.Hideable)
        {
            if (currentInteractable.interactPrompt != null)
            {
                currentInteractable.interactPrompt.SetActive(true);
            }
        }
    }

    void CheckKeypadDoor(Interactable door)
    {
        // 1. ถ้าประตูปลดล็อกแล้ว ก็วาร์ปเลย
        if (!door.isLocked)
        {
            StartCoroutine(WarpTransition(door));
        }
        // 2. ถ้ายังล็อกอยู่ ให้เปิด Keypad
        else
        {
            Debug.Log("ประตูนี้ต้องใช้รหัส");
            playerMovement.enabled = false; // หยุดผู้เล่น
            keypadController.ShowKeypad(door.correctPassword);
        }
    }

    // 3. ฟังก์ชันนี้จะถูกเรียกโดย Event "OnSuccess"
    void OnKeypadSuccess()
    {
        Debug.Log("PlayerInteract: ได้รับ Event สำเร็จ!");
        if (currentInteractable != null && currentInteractable.type == InteractionType.KeypadDoor)
        {
            currentInteractable.Unlock(); // ปลดล็อกประตู
        }
        playerMovement.enabled = true; // คืนการควบคุม
    }

    // 4. ฟังก์ชันนี้จะถูกเรียกโดย Event "OnClose"
    void OnKeypadClose()
    {
        Debug.Log("PlayerInteract: ได้รับ Event ปิด!");
        playerMovement.enabled = true; // คืนการควบคุม
    }

    // --- (ของใหม่) ฟังก์ชันเล่นเสียง ---
    void PlaySound(AudioClip clip)
    {
        if (interactAudioSource != null && clip != null)
        {
            interactAudioSource.PlayOneShot(clip);
        }
    }
}