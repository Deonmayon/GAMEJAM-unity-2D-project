using UnityEngine;
using UnityEngine.InputSystem; // ถ้าไม่ได้ใช้ New Input System ก็ไม่เป็นไร

[RequireComponent(typeof(Collider2D))]
public class DialogueTriggerOnCollision : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = false;              // true = คุยครั้งเดียวพอ
    [SerializeField] private float cooldown = 0.4f;             // กันเด้งรัว (วัดด้วย unscaled time)
    [SerializeField] private bool requireExitBeforeNext = true; // ต้องออกนอกโซนก่อนคุยรอบใหม่

    [Header("Lock Player During Dialogue (no movement edits)")]
    [SerializeField] private bool lockPlayerDuringDialogue = true;
    [SerializeField] private bool alsoFreezeRigidBody = true;   // Freeze Rigidbody2D ระหว่างคุย

    private bool triggered;
    private bool inside;
    private bool requireExitGate;       // เกตบังคับให้ออกจากโซนก่อน
    private float nextReadyUnscaled;    // ใช้ UnscaledTime กัน timeScale=0

    // ===== refs ที่เราจะล็อก/ปลดล็อก (จับจาก player ที่เข้ามาชน) =====
    private GameObject currentPlayerGO;
    private PlayerInput cachedPlayerInput;
    private Rigidbody2D cachedRb;
    private RigidbodyConstraints2D cachedConstraints;
    private bool hadRb;
    private bool lockedByMe;            // กันกรณีมีหลาย trigger ซ้อน

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnEnable() { DialogueManager.OnDialogueClosed += HandleDialogueClosed; }
    void OnDisable() { DialogueManager.OnDialogueClosed -= HandleDialogueClosed; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = true;

        // เก็บ ref player ไว้เพื่อจะล็อก/ปลดล็อก
        currentPlayerGO = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;

        TryStartDialogue();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = false;

        if (!oneShot) triggered = false;
        requireExitGate = false; // ผ่านเกตเมื่อออกนอกโซน
    }

    // ไม่เรียก TryStart ใน Stay ถ้าเปิด requireExitBeforeNext
    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = true;

        if (!requireExitBeforeNext && !oneShot && !triggered)
            TryStartDialogue();
    }

    private void TryStartDialogue()
    {
        if (!inside) return;
        if (oneShot && triggered) return;
        if (Time.unscaledTime < nextReadyUnscaled) return;

        // ถ้าบังคับให้ออกโซนก่อน แล้วยังไม่ได้ออก ห้ามเริ่มใหม่
        if (requireExitGate) return;

        // หา DialogueManager ถ้ายังไม่มีอ้างอิง
        if (DialogueManager.Instance == null)
        {
            var found = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
            if (found == null)
            {
                Debug.LogWarning("⚠️ DialogueManager.Instance is NULL. Put a DialogueManager in the scene.");
                return;
            }
            // Instance จะถูกเซ็ตใน Awake ของ DialogueManager เอง
        }

        if (dialogue == null) { Debug.LogWarning("⚠️ DialogueData is NULL."); return; }
        if (DialogueManager.Instance.IsOpen) return;

        // ===== เริ่มไดอะล็อก =====
        DialogueManager.Instance.StartDialogue(dialogue);
        triggered = true;

        // ล็อกการเดิน (โดยไม่แก้สคริปต์ movement)
        if (lockPlayerDuringDialogue && currentPlayerGO != null)
            LockPlayer(currentPlayerGO);

        // คูลดาวน์กันเด้งรัว
        nextReadyUnscaled = Time.unscaledTime + cooldown;
    }

    private void HandleDialogueClosed()
    {
        if (oneShot) return;

        // คุยเสร็จ เตรียมคุยรอบใหม่
        triggered = false;

        // ต้อง “ออกนอกโซน” ก่อน จึงคุยใหม่ได้ (ป้องกันเบิ้ล)
        if (requireExitBeforeNext)
            requireExitGate = true;

        // กันกดปิดแล้ว OnTriggerStay ยิงทันที
        nextReadyUnscaled = Mathf.Max(nextReadyUnscaled, Time.unscaledTime + 0.2f);

        // ปลดล็อก player (เฉพาะถ้าเราเป็นคนล็อก)
        if (lockPlayerDuringDialogue)
            UnlockPlayer();
    }

    // ====== Lock/Unlock helpers ======

    private void LockPlayer(GameObject playerGO)
    {
        if (lockedByMe) return; // กันล็อกซ้ำจาก trigger อื่น
        lockedByMe = true;

        // ปิด PlayerInput (ถ้าใช้งาน New Input System)
        cachedPlayerInput = playerGO.GetComponent<PlayerInput>();
        if (cachedPlayerInput)
            cachedPlayerInput.enabled = false;

        // Freeze Rigidbody2D (หยุดทันที)
        if (alsoFreezeRigidBody)
        {
            cachedRb = playerGO.GetComponent<Rigidbody2D>();
            hadRb = cachedRb != null;
            if (hadRb)
            {
                cachedConstraints = cachedRb.constraints;
                cachedRb.velocity = Vector2.zero;
                cachedRb.angularVelocity = 0f;
                cachedRb.constraints = RigidbodyConstraints2D.FreezePositionX |
                                       RigidbodyConstraints2D.FreezePositionY |
                                       RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    private void UnlockPlayer()
    {
        if (!lockedByMe) return; // ถ้าไม่ได้ล็อกไว้เอง อย่าปลด (กันชนหลายทริกเกอร์)

        // เปิด PlayerInput กลับ
        if (cachedPlayerInput)
            cachedPlayerInput.enabled = true;

        // คืนค่า Rigidbody2D constraints
        if (hadRb && cachedRb)
            cachedRb.constraints = cachedConstraints;

        // ล้าง refs
        lockedByMe = false;
        cachedPlayerInput = null;
        cachedRb = null;
        hadRb = false;
        currentPlayerGO = null;
    }
}
