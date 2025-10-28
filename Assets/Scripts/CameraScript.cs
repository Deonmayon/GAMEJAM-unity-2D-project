using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // เป้าหมายที่กล้องจะตาม
    public Transform target;
    // ความสมูท
    public float smoothTime = 0.3f;
    // ตำแหน่งออฟเซ็ต
    public Vector3 offset = new Vector3(0, 0, -10);

    // --- ส่วนที่เพิ่มเข้ามา ---
    [Header("Camera Bounds")]
    [Tooltip("ลาก GameObject 'LevelBounds' ที่มี BoxCollider2D มาใส่ช่องนี้")]
    public BoxCollider2D mapBounds; // ขอบเขตของแมพ

    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    // --- จบส่วนที่เพิ่มเข้ามา ---

    void Start()
    {
        // --- ส่วนที่เพิ่มเข้ามา ---
        cam = GetComponent<Camera>(); // ดึง Component กล้อง
        camHalfHeight = cam.orthographicSize; // ความสูงครึ่งหนึ่งของกล้อง
        camHalfWidth = camHalfHeight * cam.aspect; // ความกว้างครึ่งหนึ่งของกล้อง (คำนวณจากสัดส่วนจอ)

        // ดึงขอบเขตจาก Collider ที่เราลากมาใส่
        if (mapBounds != null)
        {
            minBounds = mapBounds.bounds.min;
            maxBounds = mapBounds.bounds.max;
        }
        // --- จบส่วนที่เพิ่มเข้ามา ---
    }


    void LateUpdate()
    {
        if (target != null)
        {
            // 1. คำนวณตำแหน่งที่กล้องควรจะไป (เหมือนเดิม)
            Vector3 targetPosition = target.position + offset;

            // --- ส่วนที่เพิ่มเข้ามา ---
            if (mapBounds != null)
            {
                // 2. จำกัดขอบเขตของตำแหน่งนั้น
                // โดยคำนึงถึง "ขนาดของกล้อง" ด้วย (บวก/ลบ ครึ่งหนึ่งของความกว้าง/สูง)
                float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
                float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

                // 3. สร้างตำแหน่งใหม่ที่ถูกจำกัดขอบเขตแล้ว
                Vector3 clampedPosition = new Vector3(clampedX, clampedY, targetPosition.z);

                // 4. ค่อยๆ เคลื่อนกล้องไปหาตำแหน่งที่ "ถูกจำกัดขอบเขตแล้ว"
                transform.position = Vector3.SmoothDamp(transform.position, clampedPosition, ref velocity, smoothTime);
            }
            // --- จบส่วนที่เพิ่มเข้ามา ---
            else
            {
                // 5. ถ้าไม่ได้ใส่ mapBounds ไว้ (กัน Error) ก็ให้ทำงานแบบเดิม
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
        }
    }
    // (ฟังก์ชันใหม่) ใช้สำหรับ "Snap" กล้องไปที่เป้าหมายทันที (เช่น ตอนวาร์ป)
    public void SnapToTargetPosition(Vector3 targetPos)
    {
        // 1. คำนวณตำแหน่งเป้าหมาย (เหมือนใน LateUpdate)
        Vector3 targetPosition = targetPos + offset;

        // 2. จำกัดขอบเขต (เหมือนใน LateUpdate)
        //    (เราต้องคำนวณใหม่ เพราะกล้องอาจจะยังไม่เริ่ม Start() ดี)
        if (cam == null) cam = GetComponent<Camera>();
        if (mapBounds != null)
        {
            camHalfHeight = cam.orthographicSize;
            camHalfWidth = camHalfHeight * cam.aspect;
            minBounds = mapBounds.bounds.min;
            maxBounds = mapBounds.bounds.max;

            float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
            float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

            Vector3 clampedPosition = new Vector3(clampedX, clampedY, targetPosition.z);

            // 3. (สำคัญ) "ตั้งค่า" ตำแหน่งโดยตรง ไม่ใช้ SmoothDamp
            transform.position = clampedPosition;
        }
        else
        {
            // 4. (สำรอง) ตั้งค่าโดยตรง
            transform.position = targetPosition;
        }

        // 5. (สำคัญมาก) รีเซ็ตความเร็วของ SmoothDamp
        velocity = Vector3.zero;
    }
}