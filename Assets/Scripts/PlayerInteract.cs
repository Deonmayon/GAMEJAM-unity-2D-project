using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro; // 1. (ของใหม่) ต้องเพิ่มบรรทัดนี้สำหรับ TextMeshPro
using UnityEngine.UI; // 1. (ของใหม่) ต้องเพิ่มบรรทัดนี้สำหรับ Image

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

    public Image itemInfoImage; // 2. (ลาก ItemInfoImage (UI) มาใส่)

    [Header("End Game UI")]
    public GameObject endGameUIPanel;

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

    public ItemDatabase itemDatabase; // 3. (ลาก ItemDatabase (ไฟล์) มาใส่)

    public KeypadController keypadController;
    private System.Action onKeypadSuccessCallback;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Coroutine autoWalkCoroutine; // 1. ตัวแปรกัน Coroutine ชนกัน
    private Animator anim; // 2. ตัวแปร Animator (สำหรับท่าเดิน)

    // Reference to the QTE manager (can be assigned in Inspector). If not set, we auto-find it in Start().
    public QTEManager qteManager;
    public DialogueUI dialogueUI;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>(); // 3. ดึง Animator ของ Player

        // Auto-assign QTEManager if it wasn't set in the Inspector
        if (qteManager == null)
        {
            qteManager = FindObjectOfType<QTEManager>();
            if (qteManager != null)
            {
                Debug.Log("QTEManager auto-assigned in PlayerInteract: " + qteManager.gameObject.name);
            }
        }

        if (dialogueUI == null)
        {
            Debug.LogWarning("ยังไม่ได้ลาก DialogueUI มาใส่ PlayerInteract!");
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
            // เปลี่ยนจาก OnKeypadSuccess เป็น HandleKeypadSuccess
            keypadController.OnSuccess.AddListener(HandleKeypadSuccess);
            keypadController.OnClose.AddListener(OnKeypadClose);
        }
        if (endGameUIPanel != null)
        {
            endGameUIPanel.SetActive(false);
        }
    }

    // --- (ของเดิม) OnTriggerEnter2D / OnTriggerExit2D ---
    void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        // (เช็กว่าเจอ, เปิดอยู่, และไม่ใช่ตัวเดียวกับที่คุยค้างไว้)
        if (interactable != null && interactable.enabled && interactable != currentInteractable)
        {
            // (ของใหม่) เช็กว่าเป็น NPC แบบ Auto-Trigger หรือไม่
            if (interactable.type == InteractionType.NPC && interactable.triggerOnEnter && autoWalkCoroutine == null)
            {
                // ถ้าใช่ -> เริ่ม Coroutine เดินอัตโนมัติ
                autoWalkCoroutine = StartCoroutine(AutoWalkAndTalk(interactable));
            }
            // (ของเดิม) ถ้าไม่ใช่ (หรือกำลัง Auto-walk อยู่) -> โชว์ปุ่ม E
            else
            {
                currentInteractable = interactable;
                if (interactable.interactPrompt != null) { interactable.interactPrompt.SetActive(true); }
                if (interactable.lockedPrompt != null) { interactable.lockedPrompt.SetActive(false); }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        // ถ้าเราเดินออกจาก Interactable ที่เรา "จำ" ไว้ (ปุ่ม E)
        if (interactable != null && interactable == currentInteractable)
        {
            if (interactable.interactPrompt != null) { interactable.interactPrompt.SetActive(false); }
            if (interactable.lockedPrompt != null) { interactable.lockedPrompt.SetActive(false); }

            currentInteractable = null; // ลืมมันซะ
        }
    }

    // --- (อัปเกรด) แก้ไข Update ให้รองรับ UI Item Info ---
    void Update()
    {
        // 1. (ของใหม่) ถ้ากำลังแสดง UI Item Info, จะรับอินพุตแค่คลิกซ้าย
        if (isDisplayingItemInfo)
        {
            // vvv (นี่คือบรรทัดที่แก้ไขแล้ว) vvv
            if (Keyboard.current.eKey.wasPressedThisFrame)
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

            // vvv (ของใหม่) vvv
            case InteractionType.KeypadCollectible:
                CheckKeypadCollectible(currentInteractable); // (ฟังก์ชันใหม่)
                break;

            case InteractionType.EndGameDoor:
                CheckEndGameDoor(currentInteractable);
                break;

            case InteractionType.NPC:
                CheckNpcDialogue(currentInteractable);
                break;
        }
    }

    // --- (อัปเกรด) ฟังก์ชันสำหรับเรียก Dialogue (ให้ส่ง NpcMovementData) ---
    void CheckNpcDialogue(Interactable npc)
    {
        // 1. (เดิม) เช็กว่าตั้งค่าครบหรือไม่
        if (dialogueUI == null) { /*...*/ return; }
        if (npc.dialogueToTrigger == null) { /*...*/ return; }

        // 2. (ของใหม่) ตรวจสอบว่า NPC ตัวนี้ต้องเดินหรือไม่
        //    (เราต้อง "อ่าน" ค่า private จาก Interactable.cs)
        //    (นี่เป็นเทคนิคขั้นสูงเล็กน้อยครับ)

        bool enableMovement = (bool)npc.GetType().GetField("enableNpcMovement",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(npc);

        if (enableMovement)
        {
            // 3. (ของใหม่) ถ้าใช่, "สร้าง" กล่องข้อมูล
            NpcMovementData movementData = new NpcMovementData();

            // ดึงค่า Private Fields จาก Interactable.cs
            movementData.npcTransform = (Transform)npc.GetType().GetField("npcTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(npc);
            movementData.destinationTransform = (Transform)npc.GetType().GetField("destinationTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(npc);
            movementData.moveSpeed = (float)npc.GetType().GetField("npcMoveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(npc);
            movementData.arrivalDistance = (float)npc.GetType().GetField("arrivalDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(npc);
            movementData.disappearOnArrival = (bool)npc.GetType().GetField("disappearOnArrival", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(npc);
            movementData.disappearDelay = (float)npc.GetType().GetField("disappearDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(npc);

            // 4. (ของใหม่) เรียก DialogueUI (Overload 2)
            dialogueUI.StartDialogue(npc.dialogueToTrigger, movementData);
        }
        else
        {
            // 5. (ของเดิม) เรียก DialogueUI (Overload 1)
            dialogueUI.StartDialogue(npc.dialogueToTrigger);
        }
        if (npc.triggerOnce)
        {
            // 1. ซ่อนปุ่ม "E"
            if (npc.interactPrompt != null)
            {
                npc.interactPrompt.SetActive(false);
            }
            // 2. ปิดสคริปต์ Interactable นี้ไปเลย (จะกด E ไม่ได้อีก)
            npc.enabled = false;
            currentInteractable = null;
        }
    }

    // --- (อัปเกรด) เปลี่ยน CollectItem เป็น Coroutine ---
    IEnumerator CollectItemProcess(Interactable item)
    {
        // 1. หยุดผู้เล่น
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero; // <--- หยุดการไถลทันที

        ItemData collectedData = itemDatabase.GetItemDataByID(item.itemID);

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
        if (item.activatesObjectsOnCollect && item.objectsToActivate != null)
        {
            Debug.Log("กำลัง Activate " + item.objectsToActivate.Count + " objects...");
            foreach (GameObject objToActivate in item.objectsToActivate)
            {
                if (objToActivate != null)
                {
                    objToActivate.SetActive(true);
                    Debug.Log("Activated: " + objToActivate.name);
                }
            }
        }

        // 3. ทำลายไอเทมที่พื้น
        Destroy(item.gameObject);
        currentInteractable = null;

        // 4. แสดง UI Item Info
        if (itemInfoPanel != null)
        {
            isDisplayingItemInfo = true;
            itemInfoPanel.SetActive(true); // เปิดพื้นหลัง

            // 6a. (ใหม่) ถ้าตั้งค่าให้ "โชว์รูปภาพ"
            if (item.showImageOnlyInInfoUI && collectedData != null && itemInfoImage != null)
            {
                itemInfoImage.sprite = collectedData.itemIcon; // ใช้ไอคอนจาก Database
                itemInfoImage.gameObject.SetActive(true); // โชว์รูป
                itemInfoText.gameObject.SetActive(false); // ซ่อน Text
            }
            // 6b. (เดิม) ถ้าโชว์ Text (ค่าเริ่มต้น)
            else if (itemInfoText != null)
            {
                itemInfoText.text = item.itemDescription; // ใช้ Description จาก Interactable
                itemInfoText.gameObject.SetActive(true); // โชว์ Text
                if (itemInfoImage != null) { itemInfoImage.gameObject.SetActive(false); } // ซ่อนรูป
            }
        }
        else
        {
            // ถ้าไม่มี UI Panel ก็คืนการควบคุมเลย
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
        // --- 1. หยุดผู้เล่นและเริ่ม Fade Out ---
        playerMovement.enabled = false;

        rb.linearVelocity = Vector2.zero; // <--- หยุดการไถลทันที

        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("StartFadeOut");
        }

        // --- 2. รอจนกว่าจอจะมืด ---
        yield return new WaitForSeconds(fadeDuration);

        // --- 3. ทำการวาร์ป (ในขณะที่จอมืด) ---
        WarpPlayer(door); // (ผู้เล่นวาร์ปไปตำแหน่งใหม่แล้ว)

        // --- 4. เริ่ม Fade In ---
        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("StartFadeIn");
        }

        // vvv (ของใหม่) บังคับให้ Trigger ทำงานอีกครั้ง vvv
        // นี่คือการ "Re-trigger" โซนที่เราวาร์ปไปถึง
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false; // ปิด
            yield return null; // (สำคัญมาก) รอ 1 เฟรม
            playerCollider.enabled = true; // เปิด
        }
        // ^^^ จบส่วนของใหม่ ^^^

        // --- 5. รอจนกว่าจอจะสว่าง ---
        yield return new WaitForSeconds(fadeDuration); // (ย้ายบรรทัดนี้มาไว้หลังโค้ดใหม่)

        // --- 6. คืนการควบคุมให้ผู้เล่น ---
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
        if (!door.isLocked)
        {
            StartCoroutine(WarpTransition(door));
        }
        else
        {
            Debug.Log("ประตูนี้ต้องใช้รหัส");
            playerMovement.enabled = false;

            // (อัปเกรด) "บันทึก" ว่าถ้าสำเร็จ ให้ *ปลดล็อกและวาร์ป*
            onKeypadSuccessCallback = () => {
                Debug.Log("Callback: ปลดล็อกประตูและวาร์ป");
                door.Unlock();
                StartCoroutine(WarpTransition(door)); // <--- เพิ่มบรรทัดนี้
            };

            keypadController.ShowKeypad(door.correctPassword);
        }
    }

    // --- (ของใหม่) ฟังก์ชันสำหรับไอเทมติดรหัส ---
    void CheckKeypadCollectible(Interactable item)
    {
        // 1. ถ้าปลดล็อกแล้ว ก็เก็บเลย
        if (!item.isLocked)
        {
            StartCoroutine(CollectItemProcess(item));
        }
        // 2. ถ้ายังล็อกอยู่ ให้เปิด Keypad
        else
        {
            Debug.Log("ไอเทมนี้ต้องใช้รหัส");
            playerMovement.enabled = false;

            // (อัปเกรด) "บันทึก" ว่าถ้าสำเร็จ ให้ *ปลดล็อกและเก็บ* ทันที
            onKeypadSuccessCallback = () => {
                Debug.Log("Callback: ปลดล็อกและเก็บไอเทม");
                item.Unlock();
                StartCoroutine(CollectItemProcess(item));
            };

            keypadController.ShowKeypad(item.correctPassword);
        }
    }

    // --- (ของใหม่) ฟังก์ชัน Handler กลาง ---
    // ฟังก์ชันนี้จะถูกเรียกโดย Event "OnSuccess"
    void HandleKeypadSuccess()
    {
        Debug.Log("PlayerInteract: ได้รับ Event สำเร็จ!");

        if (onKeypadSuccessCallback != null)
        {
            onKeypadSuccessCallback.Invoke(); // เรียก Coroutine (Warp/Collect)
        }

        onKeypadSuccessCallback = null;

        // playerMovement.enabled = true; // <--- ลบบรรทัดนี้ทิ้ง
        // (เราจะให้ Coroutine เป็นคนคืนการควบคุมเอง)
    }


    // 4. ฟังก์ชันนี้จะถูกเรียกโดย Event "OnClose"
    void OnKeypadClose()
    {
        Debug.Log("PlayerInteract: ได้รับ Event ปิด!");
        onKeypadSuccessCallback = null; // เคลียร์ Callback (เผื่อกดปิดเอง)
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
    void CheckEndGameDoor(Interactable door)
    {
        // 1. เช็กว่าประตู "ล็อกอยู่" หรือไม่
        if (door.isLocked)
        {
            // 2. ถ้าล็อกอยู่, เช็กว่ามีกุญแจที่ถูกต้องไหม
            if (inventory.Contains(door.requiredKeyID))
            {
                Debug.Log("ใช้กุญแจไขประตูทางออก! จบเกม!");
                door.Unlock(); // ปลดล็อก

                // เรียกฟังก์ชันจบเกม (ด้านล่าง)
                TriggerGameEnd(door);
            }
            else
            {
                // 3. ถ้าไม่มีกุญแจ
                Debug.Log("ประตูทางออกล็อกอยู่! ต้องใช้ " + door.requiredKeyID);
                if (door.lockedPrompt != null)
                {
                    door.lockedPrompt.SetActive(true); // โชว์ "ล็อกอยู่"
                }
            }
        }
        else
        {
            // 4. ถ้าประตู "ไม่ได้ล็อก" (เช่น ปลดล็อกไปแล้ว)
            Debug.Log("ประตูเปิดอยู่ จบเกม!");
            TriggerGameEnd(door);
        }
    }

    IEnumerator AutoWalkAndTalk(Interactable npc)
    {
        // 1. ซ่อนปุ่ม "E" (ถ้ามี) และหยุดผู้เล่น
        if (npc.interactPrompt != null) { npc.interactPrompt.SetActive(false); }
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // 2. ดึงค่าจาก Interactable
        Transform targetPosition = npc.playerWalkTarget;
        float walkSpeed = npc.autoWalkSpeed;
        float stopDistance = npc.autoWalkStopDistance;

        // (เช็กเผื่อลืมตั้งค่า)
        if (targetPosition == null)
        {
            Debug.LogWarning("AutoWalk NPC " + npc.name + " needs 'Player Walk Target' assigned!", npc);
            CheckNpcDialogue(npc); // ถ้าตั้งค่าไม่ครบ ก็เริ่มคุยเลย
            yield break;
        }

        Debug.Log("Auto-walking to " + npc.name);

        // 3. ลูปเดิน
        while (Vector2.Distance(transform.position, targetPosition.position) > stopDistance)
        {
            // สั่ง Animator ให้เดิน
            if (anim != null) anim.SetFloat("Speed", walkSpeed);

            // หันหน้า Player ให้ถูกทาง
            if (targetPosition.position.x < transform.position.x)
                spriteRenderer.flipX = true; // ไปทางซ้าย
            else if (targetPosition.position.x > transform.position.x)
                spriteRenderer.flipX = false; // ไปทางขวา

            // เคลื่อนที่
            transform.position = Vector2.MoveTowards(transform.position, targetPosition.position, walkSpeed * Time.deltaTime);
            yield return null; // รอเฟรมหน้า
        }

        // 4. ถึงที่หมายแล้ว -> หยุด
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f);

        // 5. เริ่ม Dialogue (ซึ่ง DialogueUI จะล็อกผู้เล่นต่อเอง)
        CheckNpcDialogue(npc);

        autoWalkCoroutine = null; // เคลียร์ Coroutine
    }

    // --- (ของใหม่) ฟังก์ชันสำหรับแสดง UI จบเกม ---
    void TriggerGameEnd(Interactable door)
    {
        // 1. หยุดผู้เล่น
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // 2. (ทางเลือก) ซ่อน UI ปุ่ม "E"
        if (door.interactPrompt != null)
        {
            door.interactPrompt.SetActive(false);
        }

        // 3. แสดง UI จบเกม
        if (endGameUIPanel != null)
        {
            endGameUIPanel.SetActive(true);
        }

        // (ทางเลือก) หยุดเวลาในเกม
        // Time.timeScale = 0f;
    }
}