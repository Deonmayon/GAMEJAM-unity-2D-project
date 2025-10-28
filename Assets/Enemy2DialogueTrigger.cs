using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy2DialogueTrigger : MonoBehaviour
{
    [Header("Who can trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Choices")]
    [SerializeField] private string[] choices = { "Help Him", "Walk Away", "Ask More" };

    [Header("Lock options (optional)")]
    [SerializeField] private MonoBehaviour[] movementScripts;  // สคริปต์เดินของ PLAYER (ถ้ามีให้ลากมา)
    [SerializeField] private Rigidbody2D playerRB;             // RB ของ PLAYER (ห้ามของ Enemy)

    [Header("Trigger")]
    [SerializeField] private bool oneShot = true;
    private bool triggered;

    // keep state to restore
    private readonly List<MonoBehaviour> _disabled = new();
    private bool _rbSaved;
    private RigidbodyConstraints2D _rbConstraintsSaved;
    private bool _rbSimulatedSaved;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (oneShot && triggered) return;

        LockPlayer(true);

        var dm = DialogueManager.Instance;
        if (!dm)
        {
            Debug.LogError("[Enemy2Trigger] DialogueManager not found.");
            LockPlayer(false);
            return;
        }

        dm.ShowChoiceOnly(choices, i =>
        {
            Debug.Log("[Enemy2Trigger] Picked = " + i);
            LockPlayer(false);  // 🔓 เดินได้ทันที
        }, alsoPauseGame: false);

        triggered = true;
    }

    private void LockPlayer(bool on)
    {
        // 1) toggle movement scripts (ถ้ามี)
        if (movementScripts != null)
        {
            if (on)
            {
                _disabled.Clear();
                foreach (var m in movementScripts)
                    if (m && m.enabled) { m.enabled = false; _disabled.Add(m); }
                if (_disabled.Count > 0) Debug.Log($"[Enemy2Trigger] Disabled {_disabled.Count} movement script(s).");
            }
            else
            {
                foreach (var m in _disabled) if (m) m.enabled = true;
                _disabled.Clear();
            }
        }

        // 2) lock/unlock RB
        if (!playerRB)
        {
            Debug.LogWarning("[Enemy2Trigger] PlayerRB is NULL. Drag PLAYER's Rigidbody2D here.");
            return;
        }

#if UNITY_6000_0_OR_NEWER
        playerRB.linearVelocity = Vector2.zero;
#else
        playerRB.velocity = Vector2.zero;
#endif
        playerRB.angularVelocity = 0f;

        if (on)
        {
            if (!_rbSaved)
            {
                _rbConstraintsSaved = playerRB.constraints;
                _rbSimulatedSaved = playerRB.simulated;
                _rbSaved = true;
                Debug.Log($"[Enemy2Trigger] Save RB: constraints={_rbConstraintsSaved}, simulated={_rbSimulatedSaved}");
            }

            // ตรึงตำแหน่งทั้งแกน + หมุน (ให้ simulated=true เพื่อคืนค่าทีหลังได้แน่นอน)
            playerRB.simulated = true;
            playerRB.constraints = RigidbodyConstraints2D.FreezePositionX
                                 | RigidbodyConstraints2D.FreezePositionY
                                 | RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"[Enemy2Trigger] LOCK: constraints={playerRB.constraints}, simulated={playerRB.simulated}");
        }
        else
        {
            // คืนค่าที่เซฟไว้ (กันค้าง)
            if (_rbSaved)
            {
                playerRB.constraints = _rbConstraintsSaved;
                playerRB.simulated = _rbSimulatedSaved;
            }
            else
            {
                // เผื่อไม่ได้เซฟไว้: ปลดตรึงตำแหน่ง (คง FreezeRotation ไว้)
                playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
                playerRB.simulated = true;
            }
            Debug.Log($"[Enemy2Trigger] UNLOCK: constraints={playerRB.constraints}, simulated={playerRB.simulated}");
        }
    }
}
