using UnityEngine;
using UnityEngine.EventSystems; // 1. ต้องใช้ namespace นี้

// 2. เพิ่ม 2 อินเตอร์เฟสนี้ (สำหรับตรวจจับเมาส์)
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public string itemID;

    private InventoryUI inventoryManager;

    // 3. ฟังก์ชันสำหรับรับค่าตอนที่ถูกสร้าง
    public void Initialize(string id, InventoryUI manager)
    {
        itemID = id;
        inventoryManager = manager;
    }

    // 4. ฟังก์ชันนี้จะทำงาน "เมื่อเมาส์ชี้"
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryManager != null)
        {
            inventoryManager.ShowDescription(itemID);
        }
    }

    // 5. ฟังก์ชันนี้จะทำงาน "เมื่อเมาส์ออก"
    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryManager != null)
        {
            inventoryManager.HideDescription();
        }
    }
}