using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ตัวแปรสำหรับปรับความเร็ว
    public float walkSpeed = 5f;
    public float runSpeed = 10f;

    // 1. เพิ่มตัวแปรสำหรับไฟฉาย (ลากไปใส่ใน Inspector)
    public Transform flashlightTransform;

    // ตัวแปรสำหรับตำแหน่งไฟฉายเมื่อหันขวาหรือซ้าย
    public Vector2 flashlightOffsetRight = new Vector2(0.5f, 0);
    public Vector2 flashlightOffsetLeft = new Vector2(-0.5f, 0);

    private PlayerStamina playerStamina;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isRunning;

    // Property สำหรับให้ script อื่นเช็คว่ากำลังวิ่งอยู่หรือไม่
    public bool IsRunning => isRunning;
    public bool IsMoving => Mathf.Abs(moveInput) > 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerStamina = GetComponent<PlayerStamina>();

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

        // เช็คว่ากด Shift หรือไม่ (สำหรับวิ่ง)
        isRunning = Keyboard.current.leftShiftKey.isPressed &&
                    (playerStamina == null || playerStamina.CanRun);

        if (moveInput > 0)
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
        // เลือกความเร็วตามว่ากำลังวิ่งหรือเดิน
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }
}