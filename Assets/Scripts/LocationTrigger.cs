using UnityEngine;
using TMPro; // 1. ต้องใช้ TextMeshPro

public class LocationTrigger : MonoBehaviour
{
    [Header("Location Settings")]
    [Tooltip("ชื่อสถานที่ที่จะแสดง (เช่น ห้องโถงชั้น 1)")]
    public string locationName; // 2. ชื่อสถานที่ (กรอกใน Inspector)

    [Header("UI References")]
    [Tooltip("ลาก TextMeshPro UI ที่ใช้แสดงชื่อมาใส่")]
    public TextMeshProUGUI locationTextElement; // 3. UI Text ที่เราสร้างไว้

    // 4. ฟังก์ชันนี้จะทำงานเมื่อ "เข้าไป" ในโซน
    void OnTriggerEnter2D(Collider2D other)
    {
        // เช็กว่าใช่ "Player" หรือไม่
        if (other.CompareTag("Player"))
        {
            Debug.Log("เข้าสู่: " + locationName);
            if (locationTextElement != null)
            {
                locationTextElement.text = locationName; // แสดงชื่อ
            }
        }
    }

    // 5. ฟังก์ชันนี้จะทำงานเมื่อ "ออกมา" จากโซน
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ออกจาก: " + locationName);
            if (locationTextElement != null)
            {
                locationTextElement.text = ""; // ลบชื่อทิ้ง
            }
        }
    }
}