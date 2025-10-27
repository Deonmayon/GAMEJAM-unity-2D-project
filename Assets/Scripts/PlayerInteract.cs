using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    // (อัปเกรด) เราใช้ตัวแปรเดียวนี้สำหรับทุกอย่าง
    private Interactable currentInteractable;

    // (ของเดิม) สถานะการซ่อนตัว
    private bool isHiding = false;
    private GameObject currentLocker; // เรายังต้องใช้ตัวนี้เพื่อจำว่าซ่อนตู้ไหน

    // --- กระเป๋า (Inventory) ---
    public List<string> inventory = new List<string>();

    // --- Components ---
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    public GameObject flashlightObject;

    // (เพิ่มตัวแปรนี้)
    [Header("Component References")]
    public CameraFollow mainCameraFollow;


    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    // --- (อัปเกรด) ตรวจจับ "Interactable" แค่อย่างเดียว ---
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. มองหาสคริปต์ Interactable
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
        {
            currentInteractable = interactable; // จำไว้ว่าอันนี้อยู่ใกล้
            Debug.Log("อยู่ใกล้วัตถุ: " + interactable.type);

            // 2. แสดงปุ่ม E (ของมันเอง)
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

        // 1. เช็กว่าอันที่ออก คืออันเดียวกับที่เราจำไว้
        if (interactable != null && interactable == currentInteractable)
        {
            // 2. ซ่อนปุ่ม E
            if (interactable.interactPrompt != null)
            {
                interactable.interactPrompt.SetActive(false);
            }

            currentInteractable = null; // ล้างค่า
        }
    }

    // --- (อัปเกรด) ลำดับการกด E ---
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // ลำดับ 1: ถ้าซ่อนอยู่ ให้กด E เพื่อออก (สำคัญที่สุด)
            if (isHiding)
            {
                UnHide();
            }
            // ลำดับ 2: ถ้าอยู่ใกล้วัตถุ (ไม่ว่าจะตู้ หรือ กุญแจ) ให้โต้ตอบ
            else if (currentInteractable != null)
            {
                DoInteraction();
            }
        }
    }

    // --- (ของใหม่) ฟังก์ชันจัดการการโต้ตอบหลัก ---
    void DoInteraction()
    {
        // 1. ดู "ประเภท" ของวัตถุที่เราอยู่ใกล้
        switch (currentInteractable.type)
        {
            // 2. ถ้าเป็นของเก็บได้
            case InteractionType.Collectable:
                CollectItem(currentInteractable);
                break;

            // 3. ถ้าเป็นที่ซ่อน
            case InteractionType.Hideable:
                currentLocker = currentInteractable.gameObject; // จำตู้นี้ไว้
                Hide();
                break;

            case InteractionType.Door:
                // (อัปเกรด) เรียกฟังก์ชันใหม่สำหรับเช็กประตู
                CheckDoor(currentInteractable);
                break;

        }
    }

    // (ของใหม่) แยกฟังก์ชันเก็บของออกมา
    void CollectItem(Interactable item)
    {
        string collectedItemID = item.itemID;
        Debug.Log("เก็บ " + collectedItemID);

        inventory.Add(collectedItemID); // เพิ่มเข้ากระเป๋า

        Destroy(item.gameObject); // ทำลายวัตถุ
        currentInteractable = null; // ล้างค่า
    }

    // --- (ของใหม่) ฟังก์ชันสำหรับเช็กประตู ---
    void CheckDoor(Interactable door)
    {
        // 1. เช็กว่าประตู "ไม่ได้ล็อก" หรือไม่
        if (!door.isLocked)
        {
            Debug.Log("ประตูไม่ได้ล็อก วาร์ปเลย...");
            WarpPlayer(door); // วาร์ปตามปกติ
            return; // จบการทำงาน
        }

        // 2. ถ้ามาถึงตรงนี้ แปลว่าประตู "ล็อกอยู่"
        Debug.Log("ประตูนี้ล็อกอยู่... กำลังค้นหากุญแจ: " + door.requiredKeyID);

        // 3. เช็กใน "กระเป๋า" (inventory) ว่ามีกุญแจที่ต้องการหรือไม่
        if (inventory.Contains(door.requiredKeyID))
        {
            Debug.Log("พบกุญแจ! ทำการปลดล็อกและวาร์ป...");

            // 4. สั่งปลดล็อกประตู (เผื่อใช้ครั้งต่อไปจะได้ไม่เช็กอีก)
            door.Unlock();

            // (ทางเลือก) คุณอาจจะลบกุญแจออกจากกระเป๋าถ้าอยากให้ใช้ครั้งเดียว
            // inventory.Remove(door.requiredKeyID);

            // 5. วาร์ป
            WarpPlayer(door);
        }
        else
        {
            // 6. ถ้าไม่มีกุญแจ
            Debug.Log("ไม่มีกุญแจ! ประตูยังคงล็อกอยู่");
            // (ในอนาคต: เล่นเสียง "แกร็กๆ" (ล็อก) ตรงนี้)
        }
    }

    // ระบบวาร์ปไปที่ตำแหน่งหลังกดประตู//
    void WarpPlayer(Interactable door)
    {
        if (door.warpTarget != null)
        {
            Debug.Log("วาร์ปไปที่: " + door.warpTarget.name);

            // 1. (ของใหม่) อัปเดตขอบเขตของกล้อง *ก่อน*
            if (mainCameraFollow != null && door.targetMapBounds != null)
            {
                // สั่งให้กล้องเปลี่ยนขอบเขตเป็นอันใหม่ทันที
                mainCameraFollow.mapBounds = door.targetMapBounds;
            }
            else
            {
                Debug.LogWarning("ประตูนี้ไม่ได้ตั้งค่า Target Map Bounds, กล้องอาจจะติดขอบเขตเก่า!");
            }

            // 2. ย้ายตัวละคร (เหมือนเดิม)
            this.transform.position = door.warpTarget.position;

            // 3. สั่งให้กล้อง "Snap" (ซึ่งตอนนี้มันจะใช้ขอบเขตใหม่แล้ว)
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

    // --- ระบบตู้ (ของเดิม ไม่เปลี่ยนแปลง) ---
    void Hide()
    {
        Debug.Log("กำลังซ่อนตัว!");
        isHiding = true;
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.enabled = false;
        if (flashlightObject != null) flashlightObject.SetActive(false);

        // (สำคัญ) ซ่อนปุ่ม E ของตู้ด้วย ตอนที่เราซ่อนอยู่
        if (currentInteractable.interactPrompt != null)
        {
            currentInteractable.interactPrompt.SetActive(false);
        }

        transform.position = currentLocker.transform.position;
    }

    void UnHide()
    {
        Debug.Log("ออกจากที่ซ่อน!");
        isHiding = false;

        // หยุด QTE เมื่อออกจากตู้
        if (qteManager != null)
        {
            Debug.Log("🛑 PlayerInteract: กำลังเรียก StopQTE()");
            qteManager.StopQTE();
        }
        else
        {
            Debug.LogError("❌ QTEManager ไม่พบ! ไม่สามารถหยุด QTE ได้");
        }

        // 1. ????????????????????????
        playerMovement.enabled = true;
        spriteRenderer.enabled = true;
        if (flashlightObject != null) flashlightObject.SetActive(true);

        // (สำคัญ) ถ้าเรายังอยู่ใน Trigger ตู้, ให้แสดงปุ่ม E กลับมา
        if (currentInteractable != null && currentInteractable.type == InteractionType.Hideable)
        {
            if (currentInteractable.interactPrompt != null)
            {
                currentInteractable.interactPrompt.SetActive(true);
            }
        }
    }
}