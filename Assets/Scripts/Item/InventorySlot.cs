using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public string itemID;
    private InventoryUI inventoryManager;

    public void Initialize(string id, InventoryUI manager)
    {
        itemID = id;
        inventoryManager = manager;
    }

    // ฟังก์ชันนี้จะทำงาน "เมื่อเมาส์ชี้"
    public void OnPointerEnter(PointerEventData eventData)
    {
        // vvv เพิ่มบรรทัดนี้ vvv
        print("!!! MOUSE OVER SLOT DETECTED !!! on " + itemID);
        // ^^^ จบส่วนที่เพิ่ม ^^^

        if (inventoryManager != null)
        {
            inventoryManager.ShowDescription(itemID);
        }
    }

    // ฟังก์ชันนี้จะทำงาน "เมื่อเมาส์ออก"
    public void OnPointerExit(PointerEventData eventData)
    {
        // (เพิ่มอันนี้ด้วยก็ดีครับ)
        print("!!! MOUSE EXIT !!!");

        if (inventoryManager != null)
        {
            inventoryManager.HideDescription();
        }
    }
}