using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private DialogueData dialogueData;

    [Header("Who can trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Behavior")]
    [Tooltip("ต้องออกจากโซนก่อน จึงจะทริกได้ใหม่")]
    [SerializeField] private bool requireExitBeforeRetrigger = true;

    [Tooltip("ติ๊กถูก ถ้าต้องการให้ Trigger นี้ทำงานแค่ 'ครั้งเดียว' แล้วหายไปเลย")]
    [SerializeField] private bool triggerOnceAndDisable = true;

    [Header("🚶 Player Auto-Walk to NPC (Optional)")]
    [Tooltip("ถ้าต้องการให้ Player เดินไปหา NPC อัตโนมัติเมื่อเข้า Trigger")]
    [SerializeField] private bool autoWalkToNpc = false;
    
    [Tooltip("ตำแหน่งของ NPC ที่ Player จะเดินไป")]
    [SerializeField] private Transform npcPosition;
    
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float stopDistance = 1f;

    [Header("🚶 NPC Movement After Dialogue (Optional)")]
    [Tooltip("ถ้าต้องการให้ NPC เดินไปหายไปหลัง dialogue จบ")]
    [SerializeField] private bool enableNpcMovement = false;
    [SerializeField] private Transform npcTransform;
    [SerializeField] private Transform destinationTransform;
    [SerializeField] private float npcMoveSpeed = 3f;
    [SerializeField] private float arrivalDistance = 0.1f;
    [SerializeField] private bool disappearOnArrival = true;
    [SerializeField] private float disappearDelay = 0.5f;

    // ----- internal -----
    private bool playerInside = false;
    private bool triggeredWhileInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;

        // ถ้ายังยืนค้างอยู่ในโซนเดิม และตั้งไว้ให้ต้องออกก่อน → ไม่ให้ทริกซ้ำ
        if (requireExitBeforeRetrigger && triggeredWhileInside) return;

        // ถ้าเปิดใช้งานการเดินไปหา NPC อัตโนมัติ
        if (autoWalkToNpc && npcPosition != null)
        {
            StartCoroutine(WalkToNpcThenDialogue(other.transform));
        }
        else
        {
            FireDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;

        // ออกจากโซนแล้ว ค่อยอนุญาตให้ทริกได้อีกครั้ง
        triggeredWhileInside = false;
    }

    private void FireDialogue()
    {
        if (dialogueUI == null || dialogueData == null)
        {
            Debug.LogWarning("[DialogueTrigger] Missing DialogueUI or DialogueData.", this);
            return;
        }

        // ถ้าเปิดใช้งาน NPC Movement ให้สร้าง NpcMovementData และส่งไปด้วย
        if (enableNpcMovement && npcTransform != null && destinationTransform != null)
        {
            NpcMovementData movementData = new NpcMovementData();
            movementData.npcTransform = npcTransform;
            movementData.destinationTransform = destinationTransform;
            movementData.moveSpeed = npcMoveSpeed;
            movementData.arrivalDistance = arrivalDistance;
            movementData.disappearOnArrival = disappearOnArrival;
            movementData.disappearDelay = disappearDelay;
            
            // ส่งข้อมูลไปให้ DialogueUI
            dialogueUI.StartDialogue(dialogueData, movementData);
        }
        else
        {
            dialogueUI.StartDialogue(dialogueData);
        }

        triggeredWhileInside = true; // จดว่า รอบนี้ได้ทริกไปแล้วขณะอยู่วง

        if (triggerOnceAndDisable)
        {
            // ...ก็สั่งให้ GameObject นี้ "ปิดการทำงาน" (หายไป)
            gameObject.SetActive(false);
        }
    }

    private IEnumerator WalkToNpcThenDialogue(Transform player)
    {
        // ล็อก Player ไม่ให้ควบคุมได้
        var movementScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in movementScripts)
        {
            if (script.GetType().Name.Contains("Movement") || script.GetType().Name.Contains("Controller"))
            {
                script.enabled = false;
            }
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        // เดินไปหา NPC
        while (Vector2.Distance(player.position, npcPosition.position) > stopDistance)
        {
            Vector2 direction = (npcPosition.position - player.position).normalized;
            player.position = Vector2.MoveTowards(player.position, npcPosition.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        // หยุดแล้วเริ่ม Dialogue
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        FireDialogue();

        // เปิดการควบคุม Player กลับคืน (DialogueUI จะจัดการเอง)
        foreach (var script in movementScripts)
        {
            if (script.GetType().Name.Contains("Movement") || script.GetType().Name.Contains("Controller"))
            {
                script.enabled = true;
            }
        }
    }
}
