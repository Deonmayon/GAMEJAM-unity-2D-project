using UnityEngine;
using UnityEngine.SceneManagement; // 1. (สำคัญมาก!) ต้องเพิ่มบรรทัดนี้เพื่อจัดการซีน

public class MainMenuManager : MonoBehaviour
{
    // 2. ฟังก์ชันสำหรับปุ่ม Start
    //    (ต้องเป็น public เพื่อให้ปุ่มมองเห็น)
    public void StartGame()
    {
        // !!! แก้ชื่อ "GameScene" ให้ตรงกับ "ชื่อไฟล์ซีนเกม" ของคุณเป๊ะๆ !!!
        // (เช็กชื่อจากใน Build Settings ได้)
        SceneManager.LoadScene("CutSceneStart");
    }

    // 3. ฟังก์ชันสำหรับปุ่ม Exit
    public void ExitGame()
    {
        // 4. (สำหรับเทส) พิมพ์ใน Console ว่าเรากดปุ่ม
        Debug.Log("กำลังออกจากเกม...");

        // 5. คำสั่งปิดเกม (จะทำงานเฉพาะในเกมที่ Build แล้วเท่านั้น)
        // (คำสั่งนี้จะไม่ทำงานใน Unity Editor ครับ)
        Application.Quit();
    }
}