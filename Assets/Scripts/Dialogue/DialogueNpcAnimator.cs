using UnityEngine;

/// <summary>
/// ควบคุม Animation ของ NPC อัตโนมัติระหว่างคุย
/// ไม่ต้องใส่ action ทุก node
/// </summary>
[RequireComponent(typeof(Animator))]
public class DialogueNpcAnimator : MonoBehaviour
{
    [Header("Animator Parameters")]
    [Tooltip("ชื่อ parameter ใน Animator (Bool)")]
    [SerializeField] private string talkingParameter = "isTalking";
    [SerializeField] private string idleParameter = "isIdle";
    [SerializeField] private string walkingParameter = "isWalking";

    [Header("Auto Animation")]
    [Tooltip("เล่น animation พูดอัตโนมัติเมื่อ dialogue เริ่ม?")]
    [SerializeField] private bool autoPlayTalkAnimation = true;
    
    [Tooltip("เล่น animation idle อัตโนมัติเมื่อ dialogue จบ?")]
    [SerializeField] private bool autoPlayIdleOnEnd = true;

    [Header("Look At Player")]
    [Tooltip("หันหน้าไปทาง Player อัตโนมัติเมื่อคุย?")]
    [SerializeField] private bool autoLookAtPlayer = true;
    [SerializeField] private Transform playerTransform;

    private Animator animator;
    private Vector3 originalScale;

    void Awake()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;

        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    /// <summary>
    /// เรียกเมื่อ Dialogue เริ่ม (จาก DialogueTreeManager)
    /// </summary>
    public void OnDialogueStart()
    {
        if (autoPlayTalkAnimation)
        {
            SetTalking(true);
        }

        if (autoLookAtPlayer && playerTransform != null)
        {
            LookAtTarget(playerTransform.position);
        }
    }

    /// <summary>
    /// เรียกเมื่อ Dialogue จบ
    /// </summary>
    public void OnDialogueEnd()
    {
        if (autoPlayTalkAnimation)
        {
            SetTalking(false);
        }

        if (autoPlayIdleOnEnd)
        {
            SetIdle(true);
        }
    }

    /// <summary>
    /// ตั้งค่า animation พูด
    /// </summary>
    public void SetTalking(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(talkingParameter))
        {
            animator.SetBool(talkingParameter, value);
        }
    }

    /// <summary>
    /// ตั้งค่า animation idle
    /// </summary>
    public void SetIdle(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(idleParameter))
        {
            animator.SetBool(idleParameter, value);
        }
    }

    /// <summary>
    /// ตั้งค่า animation เดิน
    /// </summary>
    public void SetWalking(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(walkingParameter))
        {
            animator.SetBool(walkingParameter, value);
        }
    }

    /// <summary>
    /// หันหน้าไปทางเป้าหมาย
    /// </summary>
    public void LookAtTarget(Vector3 targetPosition)
    {
        float direction = Mathf.Sign(targetPosition.x - transform.position.x);
        if (direction != 0)
        {
            transform.localScale = new Vector3(
                direction * Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
    }

    /// <summary>
    /// เล่น trigger animation
    /// </summary>
    public void PlayTrigger(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    /// <summary>
    /// รีเซ็ตกลับสู่ท่าปกติ
    /// </summary>
    public void ResetToDefault()
    {
        SetTalking(false);
        SetWalking(false);
        SetIdle(true);
        transform.localScale = originalScale;
    }
}
