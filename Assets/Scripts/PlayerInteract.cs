using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    // --- (ของใหม่) เพิ่มสถานะไฟฉาย ---
    [Header("Flashlight State")]
    public bool hasFlashlight = false;      // ผู้เล่นมีไฟฉายหรือยัง
    private bool isFlashlightOn = false;    // ไฟฉายกำลังเปิดอยู่หรือไม่
    public GameObject flashlightObject;     // ไฟฉาย (Spot Light 2D) ที่ติดตัว

    //Devnine
    [Header("Audio Settings")]
    public AudioSource interactAudioSource;  // AudioSource สำหรับเสียงโต้ตอบ
    public AudioClip hideSound;              // เสียงเข้าตู้
    public AudioClip unhideSound;            // เสียงออกจากตู้

    // --- (ของเดิม) ระบบตู้ ---
    private Interactable currentInteractable;
    private bool isHiding = false;
    private GameObject currentLocker;
    private Vector3 originalPositionBeforeHiding; // สำหรับจำตำแหน่งก่อนซ่อน

    // --- (ของเดิม) กระเป๋า (Inventory) ---
    public List<string> inventory = new List<string>();

    // --- (ของเดิม) Components ---
    [Header("Component References")]
    public CameraFollow mainCameraFollow;
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
            flashlightObject.SetActive(false); // ปิดไฟฉายตอนเริ่มเกม
        }

        // Devnine
        if (interactAudioSource == null)
        {
            interactAudioSource = gameObject.AddComponent<AudioSource>();
            interactAudioSource.playOnAwake = false;
            interactAudioSource.loop = false;
            Debug.LogWarning("สร้าง AudioSource สำหรับเสียงโต้ตอบอัตโนมัติ");
        }
    }

    // --- (อัปเกรด) ตรวจจับ "Interactable" แค่อย่างเดียว ---
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
        }
    }

    // --- (อัปเกรด) ออกจาก "Interactable" ---
    void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            if (interactable.interactPrompt != null)
            {
                interactable.interactPrompt.SetActive(false);
            }
            currentInteractable = null;
        }
    }

    // --- (อัปเกรด) ลำดับการกด E และ G ---
    void Update()
    {
        // 1. (ของเดิม) ตรวจสอบการกด 'E' (Interact)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isHiding)
            {
                UnHide();
            }
            else if (currentInteractable != null)
            {
                DoInteraction();
            }
        }

        // 2. (ของใหม่) ตรวจสอบการกด 'G' (Toggle Flashlight)
        //    ต้อง "มีไฟฉาย" และ "ไม่ได้ซ่อนตัวอยู่"
        if (hasFlashlight && !isHiding && Keyboard.current.gKey.wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
    }

    // --- (ของใหม่) ฟังก์ชันสำหรับเปิด/ปิดไฟฉาย ---
    void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn; // สลับค่า true/false
        Debug.Log("ไฟฉาย: " + isFlashlightOn);
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(isFlashlightOn); // เปิด/ปิด GameObject
        }
    }

    // --- (ของเดิม) ฟังก์ชันจัดการการโต้ตอบหลัก ---
    void DoInteraction()
    {
        switch (currentInteractable.type)
        {
            case InteractionType.Collectable:
                CollectItem(currentInteractable);
                break;
            case InteractionType.Hideable:
                currentLocker = currentInteractable.gameObject;
                Hide();
                break;
            case InteractionType.Door:
                CheckDoor(currentInteractable);
                break;
        }
    }

    // --- (อัปเกรด) แยกฟังก์ชันเก็บของออกมา ---
    void CollectItem(Interactable item)
    {
        string collectedItemID = item.itemID;
        Debug.Log("เก็บ " + collectedItemID);

        // (ของใหม่) เช็กว่าใช่ "Flashlight" หรือไม่
        if (collectedItemID == "Flashlight")
        {
            hasFlashlight = true;
            isFlashlightOn = true; // เปิดครั้งแรกอัตโนมัติ
            if (flashlightObject != null)
            {
                flashlightObject.SetActive(true);
            }
        }
        else
        {
            // (ของเดิม) ถ้าไม่ใช่ไฟฉาย ก็เก็บเข้ากระเป๋า
            inventory.Add(collectedItemID);
        }

        Destroy(item.gameObject);
        currentInteractable = null;
    }

    // --- (ของเดิม) ฟังก์ชันสำหรับเช็กประตู ---
    void CheckDoor(Interactable door)
    {
        if (!door.isLocked)
        {
            Debug.Log("ประตูไม่ได้ล็อก วาร์ปเลย...");
            WarpPlayer(door);
            return;
        }
        if (inventory.Contains(door.requiredKeyID))
        {
            Debug.Log("พบกุญแจ! ทำการปลดล็อกและวาร์ป...");
            door.Unlock();
            WarpPlayer(door);
        }
        else
        {
            Debug.Log("ไม่มีกุญแจ! ประตูยังคงล็อกอยู่");
        }
    }

    // --- (ของเดิม) ระบบวาร์ปไปที่ตำแหน่งหลังกดประตู ---
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

        //Dev
        // === เพิ่มบรรทัดนี้ - เล่นเสียงเข้าตู้ ===
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

        // vvv (ของใหม่) เพิ่มบรรทัดนี้ vvv
        originalPositionBeforeHiding = transform.position; // <--- จำตำแหน่งปัจจุบันไว้
        // ^^^ จบส่วนของใหม่ ^^^

        // (โค้ดเดิม) ย้ายตัวไปกลางตู้
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

        //Devnine
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

        // vvv (ของใหม่) เพิ่มบรรทัดนี้ vvv
        // ย้ายผู้เล่นกลับไปที่เดิมที่เคยยืนอยู่
        transform.position = originalPositionBeforeHiding;
        // ^^^ จบส่วนของใหม่ ^^^

        if (currentInteractable != null && currentInteractable.type == InteractionType.Hideable)
        {
            if (currentInteractable.interactPrompt != null)
            {
                currentInteractable.interactPrompt.SetActive(true);
            }
        }
    }
    void PlaySound(AudioClip clip)
    {
        if (interactAudioSource != null && clip != null)
        {
            interactAudioSource.PlayOneShot(clip);
        }
    }
}