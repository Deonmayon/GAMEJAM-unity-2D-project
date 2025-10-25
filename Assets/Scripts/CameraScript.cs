using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 1. เป้าหมายที่กล้องจะตาม (ลากตัวละครใส่ช่องนี้)
    public Transform target;

    // 2. ความสมูท (ค่ายิ่งน้อย ยิ่งตามเร็ว)
    public float smoothTime = 0.3f;

    // 3. ตำแหน่งออฟเซ็ตของกล้อง (สำคัญคือแกน Z ต้องเป็น -10)
    public Vector3 offset = new Vector3(0, 5, -10);

    // ตัวแปรภายใน ไม่ต้องยุ่ง
    private Vector3 velocity = Vector3.zero;

    // ใช้ LateUpdate() จะดีที่สุดสำหรับกล้อง
    // เพราะมันจะทำงานหลังจากที่ตัวละครขยับ (Update, FixedUpdate) เสร็จแล้ว
    void LateUpdate()
    {
        if (target != null)
        {
            // 1. คำนวณตำแหน่งที่กล้องควรจะไป
            Vector3 targetPosition = target.position + offset;

            // 2. ค่อยๆ เคลื่อนกล้องไปหาตำแหน่งนั้นอย่างนุ่มนวล
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}