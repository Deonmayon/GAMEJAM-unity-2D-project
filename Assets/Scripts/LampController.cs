using System.Collections; // 1. ต้องใช้ Coroutines
using UnityEngine;
using UnityEngine.Rendering.Universal; // 2. ต้องใช้สำหรับ Light 2D

// 3. (สำคัญ) สร้าง "โหมด" ให้เราเลือกใน Inspector
public enum LightMode
{
    On,       // เปิดตลอด
    Off,      // ปิดตลอด
    Flicker   // กระพริบ
}

public class LampController : MonoBehaviour
{
    [Header("Mode Selection")]
    [Tooltip("เลือกโหมดของหลอดไฟนี้")]
    public LightMode currentMode = LightMode.On; // 4. ค่าเริ่มต้น

    [Header("Light Component")]
    [Tooltip("ลาก Light 2D (ที่เป็นลูก) มาใส่")]
    public Light2D lampLight; // 5. ตัวแสงไฟ

    [Header("Flicker Settings (ถ้าเลือก Flicker)")]
    [Tooltip("เวลาน้อยสุดที่จะสว่าง/ดับ")]
    public float minFlickerTime = 0.05f;
    [Tooltip("เวลามากสุดที่จะสว่าง/ดับ")]
    public float maxFlickerTime = 0.3f;

    private Coroutine flickerCoroutine;

    void Start()
    {
        // 6. เรียกฟังก์ชันเพื่อตั้งค่าโหมดตอนเริ่มเกม
        SetLightMode(currentMode);
    }

    // ฟังก์ชันนี้จะตั้งค่าแสงตามโหมดที่เลือก
    public void SetLightMode(LightMode mode)
    {
        currentMode = mode;

        // หยุด Coroutine เก่า (ถ้ามี)
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }

        if (lampLight == null) return;

        // 7. ใช้ switch เพื่อแยกการทำงาน
        switch (currentMode)
        {
            case LightMode.On:
                lampLight.enabled = true; // เปิดไฟ
                break;

            case LightMode.Off:
                lampLight.enabled = false; // ปิดไฟ
                break;

            case LightMode.Flicker:
                // 8. ถ้าเป็นโหมดกระพริบ ให้เริ่ม Coroutine
                flickerCoroutine = StartCoroutine(FlickerLoop());
                break;
        }
    }

    // 9. Coroutine สำหรับการกระพริบ
    IEnumerator FlickerLoop()
    {
        lampLight.enabled = true; // เริ่มที่เปิดก่อน

        while (true)
        {
            // สุ่มเวลา
            float waitTime = Random.Range(minFlickerTime, maxFlickerTime);
            yield return new WaitForSeconds(waitTime);

            // สลับไฟ (เปิด -> ปิด, ปิด -> เปิด)
            lampLight.enabled = !lampLight.enabled;
        }
    }
}