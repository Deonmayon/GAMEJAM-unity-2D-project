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

    // NineDev
    [Header("Footstep Audio Settings")]
    public AudioSource footstepAudioSource;
    public AudioClip footstepSound;
    [Range(0.1f, 2f)]
    public float walkFootstepInterval = 0.5f;
    [Range(0.1f, 2f)]
    public float runFootstepInterval = 0.3f;
    [Range(0.5f, 1.5f)]
    public float walkPitch = 1f;
    [Range(0.5f, 1.5f)]
    public float runPitch = 1.2f;

    [Range(0f, 1f)]
    public float minVolume = 0.6f;  // ความดังต่ำสุด
    [Range(0f, 1f)]
    public float maxVolume = 1f;    // ความดังสูงสุด

    private float footstepTimer = 0f;
    private bool wasMovingLastFrame = false;

    private PlayerStamina playerStamina;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isRunning;
    // NineDev

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
        // NineDev
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.loop = false;
            Debug.LogWarning("สร้าง AudioSource สำหรับเสียงเท้าอัตโนมัติ");
        }
        // NineDev
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
        // NineDev
        HandleFootstepSound();
    }

    void FixedUpdate()
    {
        // เลือกความเร็วตามว่ากำลังวิ่งหรือเดิน
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }
    // NineDev
    void HandleFootstepSound()
    {
        bool isMovingNow = IsMoving;

        if (isMovingNow && !wasMovingLastFrame)
        {
            footstepTimer = 0f;
            // Devnine
            // เล่นเสียงครั้งแรกทันที
            float currentPitch = isRunning ? runPitch : walkPitch;
            PlayFootstepSound(currentPitch);
        }

        if (isMovingNow)
        {
            float currentInterval = isRunning ? runFootstepInterval : walkFootstepInterval;
            float currentPitch = isRunning ? runPitch : walkPitch;

            footstepTimer += Time.deltaTime;

            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound(currentPitch);
                footstepTimer = 0f;
            }
        }
        else
        {
            //devnine
            // ถ้าหยุดเดิน → หยุดเสียงทันที
            if (wasMovingLastFrame)
            {
                if (footstepAudioSource != null && footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Stop();
                }
            }
            footstepTimer = 0f;
        }

        wasMovingLastFrame = isMovingNow;
    }

    void PlayFootstepSound(float pitch)
    {
        if (footstepAudioSource != null && footstepSound != null)
        {

            footstepAudioSource.pitch = pitch;
            // สุ่มความดังในช่วง minVolume ถึง maxVolume
            footstepAudioSource.volume = Random.Range(minVolume, maxVolume);

            footstepAudioSource.PlayOneShot(footstepSound);
        }
    }

    public void StopFootstepSound()
    {
        if (footstepAudioSource != null && footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Stop();
        }
        footstepTimer = 0f;
    }
}