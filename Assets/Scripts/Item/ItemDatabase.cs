using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    // --- (อัปเกรด) เปลี่ยนจาก GetSpriteByID เป็นฟังก์ชันนี้ ---
    public ItemData GetItemDataByID(string id)
    {
        foreach (ItemData item in allItems)
        {
            if (item.itemID == id)
            {
                return item; // ส่งข้อมูลทั้งก้อน
            }
        }
        Debug.LogWarning("หา ItemID ไม่เจอใน Database: " + id);
        return null; // หาไม่เจอ
    }
}