using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Reflection;

[RequireComponent(typeof(Collider2D))]
public class Enemy3Encounter : MonoBehaviour
{
    [Header("Who triggers")]
    [SerializeField] private string playerTag = "Player";

    [Header("PLAYER control lock")]
    [SerializeField] private MonoBehaviour playerMovementScript;      // (ออปชัน) ถ้า OnDisable ทำให้หาย ให้เว้นว่าง/อย่าใช้
    [SerializeField] private bool hardDisablePlayerInput = true;      // ใช้ Deactivate/Activate ตัดอินพุต
    [SerializeField] private PlayerInput playerInput;                  // ของ Player เท่านั้น
    [SerializeField] private bool switchToUIMap = true;
    [SerializeField] private string uiMap = "UI";
    [SerializeField] private string gameplayMap = "Player";

    [Header("ENEMY lock during sequence")]
    [SerializeField] private Rigidbody2D enemyRb;                      // RB ของ Enemy3 (หาอัตโนมัติได้)
    [SerializeField] private MonoBehaviour[] enemyScriptsToDisable;    // AI/Patrol/Follow ที่ต้องปิดชั่วคราว
    [SerializeField] private bool freezeEnemyPosition = true;          // FreezeAll ระหว่างคัตซีน

    [Header("Dialogue (DialogueData asset)")]
    [SerializeField] private DialogueData mainChat;
    [SerializeField] private GameObject dialoguePanelRoot;             // ← ต้องชี้ไปที่ Panel ที่แสดงจริง

    [Header("After dialogue")]
    [SerializeField] private float delayBeforeWalkRealtime = 2f;       // ✅ หน่วงเวลาจริง (วินาที) หลังปิดไดอะล็อก
    [SerializeField] private float walkDistanceAfterDialogue = 5f;     // ✅ เดินต่ออีกกี่หน่วย (เมตร)
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private bool keepDialogueOpenDuringWalk = true;   // ✅ ค้างหน้าต่างไดอะล็อกไว้จนเดินเสร็จ

    [Header("Misc")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private bool destroyTriggerAfter = false;         // ❗ไม่ลบ Enemy3

    private bool triggered;
    private RigidbodyConstraints2D enemyConstraintsBackup;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Awake()
    {
        if (!enemyRb) TryGetComponent(out enemyRb);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((triggered && oneShot) || !other.CompareTag(playerTag)) return;
        triggered = true;
        var player = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        StartCoroutine(RunSequence(player));
    }

    IEnumerator RunSequence(GameObject player)
    {
        // ---------- LOCK ENEMY ----------
        if (freezeEnemyPosition && enemyRb)
        {
            enemyConstraintsBackup = enemyRb.constraints;
            enemyRb.isKinematic = true;
#if UNITY_6000_0_OR_NEWER
            enemyRb.linearVelocity = Vector2.zero;
#else
            enemyRb.velocity = Vector2.zero;
#endif
            enemyRb.gravityScale = 0f;
            enemyRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        if (enemyScriptsToDisable != null)
            foreach (var s in enemyScriptsToDisable) if (s) s.enabled = false;

        // ---------- LOCK PLAYER ----------
        var rb = player.GetComponent<Rigidbody2D>();
        if (!rb) yield break;

        if (hardDisablePlayerInput && playerInput) playerInput.DeactivateInput();
        if (switchToUIMap && playerInput && !string.IsNullOrEmpty(uiMap)) SafeSwitch(playerInput, uiMap);
        SetVX(rb, 0f);

        // ---------- (1) เปิด Dialogue ----------
        if (mainChat && DialogueManager.Instance)
        {
            Debug.Log("[Enemy3Encounter] StartDialogue");
            DialogueManager.Instance.StartDialogue(mainChat);

            // รอจนผู้เล่นปิดบทสนทนา (หรือ panel ถูกปิดจริง)
            yield return WaitDialogueClosed();
            Debug.Log("[Enemy3Encounter] Dialogue closed");

            // ✅ หน่วงเวลา 'จริง' ก่อนเริ่มเดิน (ไม่ขึ้นกับ timeScale)
            if (delayBeforeWalkRealtime > 0f)
            {
                Debug.Log($"[Enemy3Encounter] Delay before walk (realtime): {delayBeforeWalkRealtime}s");
                yield return new WaitForSecondsRealtime(delayBeforeWalkRealtime);
            }
        }

        // ---------- (2) ค้างหน้าต่างไดอะล็อกไว้ระหว่างเดิน (ถ้าต้องการ) ----------
        if (keepDialogueOpenDuringWalk && dialoguePanelRoot)
        {
            if (!dialoguePanelRoot.activeSelf) dialoguePanelRoot.SetActive(true);
            Debug.Log("[Enemy3Encounter] Keep dialogue open while walking");
        }

        // ---------- (3) เดินต่ออีก N เมตร ----------
        float dir = GetAutoDirFromRelativePosition(player.transform.position.x, transform.position.x);
        float targetX = player.transform.position.x + (walkDistanceAfterDialogue * dir);
        Debug.Log($"[Enemy3Encounter] Walk to X={targetX} (dir={dir}, dist={walkDistanceAfterDialogue})");
        yield return MoveToX(rb, targetX, dir, walkSpeed);

        // ---------- (4) ปิดหน้าต่างไดอะล็อกหลังเดินเสร็จ ----------
        if (keepDialogueOpenDuringWalk && dialoguePanelRoot)
        {
            if (dialoguePanelRoot.activeSelf) dialoguePanelRoot.SetActive(false);
            Debug.Log("[Enemy3Encounter] Close dialogue after walking");
        }

        // ---------- (5) คืนการควบคุม ----------
        SetVX(rb, 0f);
        if (switchToUIMap && playerInput && !string.IsNullOrEmpty(gameplayMap)) SafeSwitch(playerInput, gameplayMap);
        if (hardDisablePlayerInput && playerInput) playerInput.ActivateInput();

        if (enemyScriptsToDisable != null)
            foreach (var s in enemyScriptsToDisable) if (s) s.enabled = true;

        if (freezeEnemyPosition && enemyRb)
            enemyRb.constraints = enemyConstraintsBackup;

        // ไม่ลบ Enemy3:
        // if (destroyTriggerAfter) Destroy(gameObject);
    }

    // ---------- Helpers ----------
    IEnumerator MoveToX(Rigidbody2D rb, float targetX, float dir, float speed)
    {
        var sr = rb.GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.flipX = dir < 0f;

        const float eps = 0.02f;
        while (Mathf.Abs(rb.position.x - targetX) > eps)
        {
            SetVX(rb, speed * dir);
            yield return null;
        }
        SetVX(rb, 0f);
    }

    IEnumerator WaitDialogueClosed()
    {
        // A) ถ้ามี IsRunning ใน DialogueManager ให้รอจน false
        var dm = DialogueManager.Instance;
        if (dm != null)
        {
            var prop = dm.GetType().GetProperty("IsRunning", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                yield return new WaitUntil(() => dm == null || !(bool)prop.GetValue(dm));
                yield break;
            }
        }
        // B) รอจน panel ปิด
        if (dialoguePanelRoot)
        {
            // กันเฟรมแรกที่สถานะยังไม่อัปเดต
            yield return null;
            yield return new WaitUntil(() => !dialoguePanelRoot.activeInHierarchy);
            yield break;
        }
        // C) กันค้างกรณีไม่มีระบบไดอะล็อกจริง ๆ (สั้นมาก)
        yield return new WaitForSecondsRealtime(0.1f);
    }

    void SafeSwitch(PlayerInput input, string map)
    {
        try { if (input && input.currentActionMap?.name != map) input.SwitchCurrentActionMap(map); } catch { }
    }

    static float GetAutoDirFromRelativePosition(float playerX, float enemyX)
        => playerX < enemyX ? +1f : -1f;

    static void SetVX(Rigidbody2D rb, float vx)
    {
#if UNITY_6000_0_OR_NEWER
        var v = rb.linearVelocity; v.x = vx; rb.linearVelocity = v;
#else
        var v = rb.velocity; v.x = vx; rb.velocity = v;
#endif
    }
}
