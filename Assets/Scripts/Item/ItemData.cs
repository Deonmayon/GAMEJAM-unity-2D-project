using UnityEngine;

[System.Serializable] // ทำให้มันโชว์ใน Inspector
public class ItemData
{
    public string itemID;
    public Sprite itemIcon;
    [TextArea(3, 5)] // ทำให้ช่องใน Inspector พิมพ์ง่ายขึ้น
    public string description;
}