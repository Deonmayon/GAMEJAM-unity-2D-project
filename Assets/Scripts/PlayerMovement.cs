using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ตัวแปรสำหรับปรับความเร็ว
    public float moveSpeed = 5f;

    // 1. เพิ่มตัวแปรสำหรับไฟฉาย (ลากไปใส่ใน Inspector)
    public Transform flashlightTransform;

    // 1. เพิ่มตัวแปรสำหรับตำแหน่งไฟฉายเมื่อหันขวา
    public Vector2 flashlightOffsetRight = new Vector2(0.5f, 0);
    // 2. เพิ่มตัวแปรสำหรับตำแหน่งไฟฉายเมื่อหันซ้าย (จะกลับด้านจากขวา)
    public Vector2 flashlightOffsetLeft = new Vector2(-0.5f, 0);

    // ตัวแปรสำหรับเก็บค่า Input
    private float moveInput;

    // ส่วนประกอบที่เราต้องใช้
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 3. ตั้งตำแหน่งเริ่มต้นของไฟฉายให้ถูกต้อง (ถ้ามี)
        if (flashlightTransform != null)
        {
            // ตรวจสอบทิศทางเริ่มต้นของ spriteRenderer
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                flashlightTransform.localPosition = flashlightOffsetLeft;
                flashlightTransform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                flashlightTransform.localPosition = flashlightOffsetRight;
                flashlightTransform.rotation = Quaternion.Euler(0, 0, 270);
            }
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        moveInput = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput = 1f;
        }

        // 4. ปรับตำแหน่งและหมุนไฟฉายตามทิศทาง
        if (moveInput > 0) // ถ้าไปทางขวา
        {
            spriteRenderer.flipX = false; // หันขวา
            if (flashlightTransform != null)
            {
                flashlightTransform.localPosition = flashlightOffsetRight; // ตั้งตำแหน่งไปทางขวา
                flashlightTransform.rotation = Quaternion.Euler(0, 0, 270); // หมุนไปทางขวา
            }
        }
        else if (moveInput < 0) // ถ้าไปทางซ้าย
        {
            spriteRenderer.flipX = true; // หันซ้าย
            if (flashlightTransform != null)
            {
                flashlightTransform.localPosition = flashlightOffsetLeft; // ตั้งตำแหน่งไปทางซ้าย
                flashlightTransform.rotation = Quaternion.Euler(0, 0, 90); // หมุนไปทางซ้าย
            }
        }
    }
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}