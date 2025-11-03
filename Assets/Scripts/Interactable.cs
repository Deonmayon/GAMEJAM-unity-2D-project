using UnityEngine;
using System.Collections.Generic; // (ต้องมีอันนี้)

// 1. (ของใหม่) เพิ่ม "Door" เข้าไปใน enum
public enum InteractionType
{
    Collectable, // ของเก็บได้
    Hideable,    // ที่ซ่อนตัว
    Door,       // ประตูวาร์ป
    KeypadDoor, // <-- เพิ่มอันนี้
    KeypadCollectible, // <-- เพิ่มอันนี้
    EndGameDoor, // <-- (1) เพิ่มอันนี้
    NPC
}

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionType type;

    [Header("Item Settings (if Collectable)")]
    public string itemID;

    [Header("Activation Settings (if Collectable)")]
    [Tooltip("ติ๊กถูก ถ้าอยากให้ไอเทมนี้ 'เปิด' (SetActive) GameObject ที่มีอยู่")]
    public bool activatesObjectsOnCollect = false;

    [Tooltip("ลิสต์ของ GameObject ในฉาก ที่จะถูก SetActive(true)")]
    public List<GameObject> objectsToActivate; // (เปลี่ยนจาก spawnList เป็นอันนี้)

    [Tooltip("ข้อความที่จะแสดงเมื่อเก็บไอเทมนี้ (ถ้าเป็น Collectable)")]
    [TextArea(3, 10)] // ทำให้ช่องพิมพ์ข้อความใหญ่ขึ้นใน Inspector
    public string itemDescription;

    [Tooltip("ไอเทมนี้ถูกล็อกโดยเงื่อนไขหรือไม่")]
    public bool isLockedByPrerequisite = false;
    [Tooltip("ItemID ของไอเทมที่ต้องมีก่อน (ถ้าล็อกอยู่)")]
    public string requiredItemID;

    [Tooltip("ติ๊กถูก ถ้าอยากให้โชว์ 'รูปภาพ' ของไอเทมนี้แทน 'ข้อความ' ตอนเก็บ")]
    public bool showImageOnlyInInfoUI = false;

    // vvv (ของใหม่) เพิ่ม Header และ 3 บรรทัดนี้ vvv
    [Header("Spawning Settings (if Collectable)")]
    [Tooltip("ติ๊กถูก ถ้าอยากให้ไอเทมนี้ Spawn อะไรบางอย่างตอนเก็บ")]
    public bool spawnsObjectOnCollect = false;
    [Tooltip("ลิสต์ของ Object ที่จะ Spawn (ใส่ได้ 1, 2, หรือ 10 อัน)")]
    // ^^^ จบส่วนของใหม่ ^^^

    // 2. (ของใหม่) เพิ่ม Header และตัวแปรสำหรับประตู
    [Header("Door Settings (if Door or KeypadDoor)")]
    public Transform warpTarget;

    [Tooltip("ลาก 'LevelBounds' ของ *ฉากปลายทาง* มาใส่")]
    public BoxCollider2D targetMapBounds;

    // vvv (ของใหม่) เพิ่ม 2 บรรทัดนี้ vvv
    [Tooltip("ประตูนี้ล็อกอยู่หรือไม่")]
    public bool isLocked = false;
    [Tooltip("ItemID ของกุญแจที่ต้องใช้ (ถ้า isLocked = true)")]
    public string requiredKeyID;
    public string correctPassword; // 3. รหัสผ่านที่ถูกต้อง
    // ^^^ (จบส่วนของใหม่) ^^^

    [Header("UI Prompt")]
    public GameObject interactPrompt;
    public GameObject lockedPrompt; // (อันใหม่)


    // สำหรับ Dialogue //
    [Header("NPC Settings (if NPC)")]
    [Tooltip("ลากไฟล์ ScriptableObject (DialogueData) ที่จะให้ NPC นี้พูดมาใส่")]
    public DialogueData dialogueToTrigger;
    // vvv (ของใหม่) เพิ่ม Header นี้ทั้งหมด vvv
    [Header("🚶 NPC Movement After Dialogue (Optional)")]
    [Tooltip("ติ๊กถูก ถ้าต้องการให้ NPC นี้เดินไปหลังคุยจบ")]
    [SerializeField] private bool enableNpcMovement = false;

    [Tooltip("Transform ของ NPC (ลากตัว NPC เองมาใส่)")]
    [SerializeField] private Transform npcTransform;

    [Tooltip("จุดหมายที่ NPC จะเดินไป (ลาก GameObject ว่างๆ มาใส่)")]
    [SerializeField] private Transform destinationTransform;

    [SerializeField] private float npcMoveSpeed = 3f;
    [SerializeField] private float arrivalDistance = 0.1f;
    [SerializeField] private bool disappearOnArrival = true;
    [SerializeField] private float disappearDelay = 0.5f;
    [Tooltip("ติ๊กถูก ถ้าต้องการให้ NPC นี้คุยได้แค่ครั้งเดียว")]
    public bool triggerOnce = true;

    // (เพิ่ม) Hideable Settings (แก้ Error CS1061)
    [Header("Hideable Settings (if Hideable)")]
    public Transform hideSpot; // <-- (แก้ Error CS1061)

    // vvv (ของใหม่) เพิ่ม 3 บรรทัดนี้ vvv
    [Header("NPC Auto-Interact Settings (Optional)")]
    [Tooltip("ติ๊กถูก ถ้าต้องการให้ผู้เล่นเดินไปหา NPC อัตโนมัติ (ไม่ต้องกด E)")]
    public bool triggerOnEnter = false;
    [Tooltip("ตำแหน่งที่ Player จะเดินไปยืนคุย (ลาก GameObject ว่างๆ มาใส่)")]
    public Transform playerWalkTarget;
    [Tooltip("ความเร็วที่ Player จะเดินไปหา")]
    public float autoWalkSpeed = 3f;
    [Tooltip("ระยะห่างที่ Player จะหยุด")]
    public float autoWalkStopDistance = 1f;
    // ^^^ จบส่วนของใหม่ ^^^


    void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    // (ของใหม่) เพิ่มฟังก์ชันนี้ เพื่อให้ PlayerInteract สั่งปลดล็อก
    public void Unlock()
    {
        isLocked = false;
        Debug.Log("ประตู " + this.name + " ถูกปลดล็อกแล้ว!");
        // (ในอนาคต อาจจะเปลี่ยน Sprite ปุ่ม E จาก "ล็อก" เป็น "เปิด")
    }
}