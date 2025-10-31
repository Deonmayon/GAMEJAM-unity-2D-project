using UnityEngine;
using UnityEngine.UI; // 1. ต้องใช้สำหรับ Image
using UnityEngine.InputSystem; // 2. ต้องใช้สำหรับ Mouse
using UnityEngine.SceneManagement; // 3. ต้องใช้สำหรับ LoadScene

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ลาก Image (UI) ที่ใช้โชว์รูปมาใส่")]
    public Image cutsceneDisplay; // 4. ช่องสำหรับลาก Image

    [Header("Cutscene Settings")]
    [Tooltip("ลาก Sprite ทั้ง 4 รูป (หรือมากกว่า) มาใส่ที่นี่")]
    public Sprite[] cutsceneSlides; // 5. ช่องสำหรับรูปทั้ง 4

    [Tooltip("ชื่อของ 'ฉากเกม' ที่จะให้ไปต่อ")]
    public string nextSceneName = "GameScene"; // 6. (แก้ชื่อนี้ให้ตรงกับฉากเกมของคุณ)

    // --- ตัวแปรภายใน ---
    private int currentImageIndex = 0; // 7. ตัวนับว่าถึงรูปไหน

    void Start()
    {
        if (cutsceneDisplay != null && cutsceneSlides.Length > 0)
        {
            // 8. เริ่มที่รูปแรก
            cutsceneDisplay.sprite = cutsceneSlides[0];
        }
    }

    void Update()
    {
        // 9. ตรวจสอบการคลิกซ้าย
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowNextImage();
        }
    }

    void ShowNextImage()
    {
        // 10. เลื่อนไปรูปถัดไป
        currentImageIndex++;

        // 11. เช็กว่ายังมีรูปเหลือไหม
        if (currentImageIndex < cutsceneSlides.Length)
        {
            // ถ้ายังมี -> เปลี่ยนรูป
            cutsceneDisplay.sprite = cutsceneSlides[currentImageIndex];
        }
        else
        {
            // 12. ถ้าหมดแล้ว -> โหลดฉากเกม
            Debug.Log("Cutscene จบแล้ว, กำลังโหลด " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
    }
}