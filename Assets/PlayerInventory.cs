using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<string> keys = new List<string>(); // เก็บชื่อคีย์ทั้งหมดที่เก็บได้

    public void PickupKey(string keyName)
    {
        if (!keys.Contains(keyName))
        {
            keys.Add(keyName);
            Debug.Log($"🗝️ เก็บกุญแจ: {keyName} (จำนวนทั้งหมด: {keys.Count})");
        }
        else
        {
            Debug.Log($"🔁 มี {keyName} แล้ว ไม่ต้องเก็บซ้ำ");
        }
    }

    public bool HasKey(string keyName)
    {
        return keys.Contains(keyName);
    }

    public List<string> GetAllKeys()
    {
        return keys;
    }
}
