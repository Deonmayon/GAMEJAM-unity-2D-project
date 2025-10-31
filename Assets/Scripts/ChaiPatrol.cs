using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class ChaiPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("ความเร็วในการเดินของ NPC")]
    public float moveSpeed = 2f;

    [Tooltip("จุด A (ลาก GameObject ว่างๆ มาใส่)")]
    public Transform patrolPointA;

    [Tooltip("จุด B (ลาก GameObject ว่างๆ มาใส่)")]
    public Transform patrolPointB;

    [Tooltip("ระยะห่างที่จะสลับจุด (เช่น 0.1)")]
    public float stopDistance = 0.1f;

    // ตัวแปรภายใน
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Transform currentTarget; // เป้าหมายปัจจุบัน (A หรือ B)

    void Start()
    {
        // 1. ดึง Component มาเก็บไว้
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. ป้องกันตัวละครล้ม (เหมือน Player)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 3. ตรวจสอบว่าตั้งค่าครบ
        if (patrolPointA == null || patrolPointB == null)
        {
            Debug.LogError("ยังไม่ได้ตั้งค่า patrolPointA หรือ patrolPointB ใน " + gameObject.name, this);
            enabled = false; // ปิดสคริปต์นี้ไปเลย ถ้าตั้งค่าไม่ครบ
            return;
        }

        // 4. เริ่มต้นให้เดินไปที่จุด B
        currentTarget = patrolPointB;
    }

    // เราใช้ FixedUpdate เพราะเรายุ่งกับฟิสิกส์ (Rigidbody)
    void FixedUpdate()
    {
        if (currentTarget == null) return;

        // 1. คำนวณทิศทางแนวนอน
        float horizontalDistance = currentTarget.position.x - transform.position.x;
        float directionX = Mathf.Sign(horizontalDistance); // จะได้ -1 (ซ้าย) หรือ 1 (ขวา)

        float animatorSpeed = 0; // ความเร็วที่จะส่งให้ Animator (เริ่มต้นที่ 0)

        // 2. ตรวจสอบว่าถึงเป้าหมายหรือยัง
        if (Mathf.Abs(horizontalDistance) < stopDistance)
        {
            // --- ถ้าถึงแล้ว ---
            // 3. สลับเป้าหมาย
            if (currentTarget == patrolPointB)
            {
                currentTarget = patrolPointA;
            }
            else
            {
                currentTarget = patrolPointB;
            }

            // 4. หยุดนิ่ง (ความเร็วเป็น 0)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animatorSpeed = 0;
        }
        else
        {
            // --- ถ้ายังไม่ถึง ---
            // 5. สั่งการเคลื่อนที่
            rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
            animatorSpeed = moveSpeed; // บอก Animator ว่ากำลังเดิน
        }

        // 6. (สำคัญ) ส่งค่า Speed ไปให้ Animator
        // (Parameter "Speed" ต้องตรงกับใน Animator Controller ของคุณ)
        anim.SetFloat("Speed", animatorSpeed);

        // 7. (สำคัญ) กลับด้าน Sprite
        if (directionX > 0)
        {
            spriteRenderer.flipX = true; // หันขวา
        }
        else if (directionX < 0)
        {
            spriteRenderer.flipX = false; // หันซ้าย
        }
    }
}
