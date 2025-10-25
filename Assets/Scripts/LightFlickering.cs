using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D flashlight;
    private float normalIntensity; // ความสว่างปกติ
    private Coroutine flickerCoroutine; // ตัวแปรสำหรับเก็บ Coroutine

    [Header("Timings")]
    [Tooltip("เวลาน้อยสุด ที่ไฟจะสว่างปกติ")]
    public float minWaitTime = 5f;
    [Tooltip("เวลามากสุด ที่ไฟจะสว่างปกติ")]
    public float maxWaitTime = 15f;

    [Header("Flicker Settings")]
    [Tooltip("ความเร็วในการกระพริบ")]
    public float flickerSpeed = 0.05f;
    [Tooltip("จำนวนครั้งที่กระพริบ (น้อยสุด-มากสุด)")]
    public int minFlickerCount = 1;
    public int maxFlickerCount = 4;
    [Tooltip("ความสว่างตอนที่กระพริบ")]
    public float flickerIntensity = 0.1f;

    // Awake() จะทำงาน "ครั้งเดียว" ตอนที่ Object ถูกสร้าง
    // เหมาะสำหรับการ "จำ" ค่าเริ่มต้น
    void Awake()
    {
        // 1. ดึง Component Light2D มาเก็บไว้
        flashlight = GetComponent<Light2D>();
        if (flashlight == null)
        {
            Debug.LogError("ไม่เจอ Light2D component!");
            return;
        }

        // 2. จำค่าความสว่างปกติ "ดั้งเดิม" ไว้ (จะจำค่านี้แค่ครั้งเดียว)
        normalIntensity = flashlight.intensity;
    }

    // OnEnable() จะทำงาน "ทุกครั้ง" ที่ GameObject นี้ถูกเปิด (SetActive(true))
    void OnEnable()
    {
        // 3. "บังคับ" รีเซ็ตความสว่างกลับเป็นปกติทันทีที่ถูกเปิด
        flashlight.intensity = normalIntensity;

        // 4. เริ่มลูปการกระพริบใหม่
        flickerCoroutine = StartCoroutine(FlickerLoop());
    }

    // OnDisable() จะทำงาน "ทุกครั้ง" ที่ GameObject นี้ถูกปิด (SetActive(false))
    void OnDisable()
    {
        // 5. หยุด Coroutine ที่กำลังทำงานอยู่ (กัน Error)
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
    }

    IEnumerator FlickerLoop()
    {
        // 6. ทำงานวนไปเรื่อยๆ (ตราบใดที่ยัง OnEnable)
        while (true)
        {
            // --- เฟสที่ 1: ไฟสว่างปกติ ---
            // (เราเพิ่งตั้งค่าไปใน OnEnable() แล้ว)
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // --- เฟสที่ 2: เริ่มกระพริบ ---
            int flickerCount = Random.Range(minFlickerCount, maxFlickerCount);

            for (int i = 0; i < flickerCount; i++)
            {
                flashlight.intensity = flickerIntensity;
                yield return new WaitForSeconds(flickerSpeed);

                flashlight.intensity = normalIntensity;
                yield return new WaitForSeconds(flickerSpeed);
            }
        }
    }
}