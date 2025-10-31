using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteract2 : MonoBehaviour
{
    // (อัปเกรด) เราใช้ตัวแปรเดียวนี้สำหรับทุกอย่าง
    private WarpLocationManager.WarpLocationInfo targetWarp;
    private Interactable currentInteractable;
    private EnemyAI enemyAI;
    public string currentFloor = "Floor1"; // The player's starting floor

    private bool isHiding = false;
    private GameObject currentLocker;
    // --- กระเป๋า (Inventory) ---
    public List<string> inventory = new List<string>();

    // --- Components ---
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    public GameObject flashlightObject;

    // (เพิ่มตัวแปรนี้)
    [Header("Component References")]
    public CameraFollow mainCameraFollow;
    public QTEManager qteManager;


    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        // Auto-assign QTEManager if it wasn't set in the Inspector
        if (qteManager == null)
        {
            qteManager = FindObjectOfType<QTEManager>();
            if (qteManager != null)
            {
                Debug.Log("QTEManager auto-assigned in PlayerInteract: " + qteManager.gameObject.name);
            }
        }
        // ... (your existing Start code) ...
        enemyAI = FindObjectOfType<EnemyAI>();
        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.UpdatePlayerFloor(currentFloor);
        }

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
            case InteractionType.Stair:
                CheckStair(currentInteractable);
                break;

        }
    }

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

    void CheckStair(Interactable stair)
    {
        WarpPlayer(stair);
        return;
    }

    // ระบบวาร์ปไปที่ตำแหน่งหลังกดประตู//
    void WarpPlayer(Interactable objects)
    {
        if (objects.warpTarget != null)
        {
            Debug.Log("วาร์ปไปที่: " + objects.warpTarget.name);

            // 1. (ของใหม่) อัปเดตขอบเขตของกล้อง *ก่อน*
            if (mainCameraFollow != null && objects.targetMapBounds != null)
            {
                // สั่งให้กล้องเปลี่ยนขอบเขตเป็นอันใหม่ทันที
                mainCameraFollow.mapBounds = objects.targetMapBounds;
            }
            else
            {
                Debug.Log(objects.targetMapBounds);
                Debug.LogWarning("ประตูนี้ไม่ได้ตั้งค่า Target Map Bounds, กล้องอาจจะติดขอบเขตเก่า!");
            }

            // 2. ย้ายตัวละคร (เหมือนเดิม)
            this.transform.position = objects.warpTarget.position;

            // --- NEW CODE ---
            // Get the destination floor name from the door/stair's WarpLocationSetup script
            WarpLocationSetup warpInfo = objects.GetComponent<WarpLocationSetup>();
            if (warpInfo != null)
            {
                currentFloor = warpInfo.destinationFloor;
                if (WarpLocationManager.Instance != null)
                {
                    WarpLocationManager.Instance.UpdatePlayerFloor(currentFloor);
                }
            }
            else
            {
                Debug.LogError("ยังไม่ได้ตั้งค่า Warp Target สำหรับประตูนี้!");
            }
        }
    }

    // --- ระบบตู้ (ของเดิม ไม่เปลี่ยนแปลง) ---
    void Hide()
    {
        Debug.Log("กำลังซ่อนตัว!");
        isHiding = true;
        playerMovement.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // ✅ NEW: ตรึงตำแหน่งและการหมุนทั้งหมดเพื่อไม่ให้ขยับเลย
        }

        spriteRenderer.enabled = false;
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }

        // ซ่อนปุ่ม E ของตู้
        if (currentInteractable.interactPrompt != null)
        {
            currentInteractable.interactPrompt.SetActive(false);
        }

        // ย้ายผู้เล่นไปที่ตำแหน่งตู้
        transform.position = currentLocker.transform.position;

        if (enemyAI != null)
        {
            enemyAI.OnPlayerHiding();
        }

        // เมื่อซ่อน ให้เริ่ม QTE UI
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

    // ใน PlayerInteract.cs, หาฟังก์ชัน UnHide() แล้วแทนที่ด้วยโค้ดนี้

    void UnHide()
    {
        Debug.Log("ออกจากที่ซ่อน!");
        isHiding = false;

        // หยุด QTE
        if (qteManager != null)
        {
            Debug.Log("🛑 PlayerInteract: กำลังเรียก StopQTE()");
            qteManager.StopQTE();
        }

        playerMovement.enabled = true;
        spriteRenderer.enabled = true;
        if (flashlightObject != null) flashlightObject.SetActive(true);

        // 3. แจ้ง Enemy AI ให้จัดการสถานะตัวเอง (Respawn Enemy)
        enemyAI = FindObjectOfType<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.OnPlayerUnHiding();
        }

        // (สำคัญ) ถ้าเรายังอยู่ใน Trigger ตู้, ให้แสดงปุ่ม E กลับมา (ถ้าตู้มีปุ่ม E)
        if (currentInteractable != null && currentInteractable.type == InteractionType.Hideable)
        {
            if (currentInteractable.interactPrompt != null)
            {
                currentInteractable.interactPrompt.SetActive(true);
            }
        }
    }
}