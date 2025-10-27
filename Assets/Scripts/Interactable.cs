using UnityEngine;

// 1. (ของใหม่) เพิ่ม "Door" เข้าไปใน enum
public enum InteractionType
{
    Collectable, // ของเก็บได้
    Hideable,    // ที่ซ่อนตัว
    Door         // ประตูวาร์ป
    // Switch (เผื่ออนาคต)
}

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionType type;

    [Header("Item Settings (if Collectable)")]
    public string itemID;

    // 2. (ของใหม่) เพิ่ม Header และตัวแปรสำหรับประตู
    [Header("Door Settings (if Door)")]
    [Tooltip("ลาก GameObject ที่เป็นเป้าหมายปลายทางมาใส่")]
    public Transform warpTarget;

    [Tooltip("ลาก 'LevelBounds' ของ *ฉากปลายทาง* มาใส่")]
    public BoxCollider2D targetMapBounds;

    // vvv (ของใหม่) เพิ่ม 2 บรรทัดนี้ vvv
    [Tooltip("ประตูนี้ล็อกอยู่หรือไม่")]
    public bool isLocked = false;
    [Tooltip("ItemID ของกุญแจที่ต้องใช้ (ถ้า isLocked = true)")]
    public string requiredKeyID;
    // ^^^ (จบส่วนของใหม่) ^^^

    [Header("UI Prompt")]
    public GameObject interactPrompt;

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