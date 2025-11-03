using UnityEngine;

/// <summary>
/// Trigger สำหรับเริ่ม DialogueTree
/// ใช้แทน DialogueTrigger เดิม
/// </summary>
public class DialogueTreeTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueTreeManager dialogueManager;
    [SerializeField] private DialogueTree dialogueTree;

    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnEnter = true;
    [SerializeField] private bool requireInput = false; // ต้องกด interact key?
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool requireExitBeforeRetrigger = true;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject interactPrompt; // แสดง "Press E" เป็นต้น

    private bool hasTriggered = false;
    private bool playerInZone = false;

    void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueTreeManager>();
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // ถ้าต้องกดปุ่มเพื่อเริ่ม dialogue
        if (requireInput && playerInZone && !hasTriggered)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TriggerDialogue();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInZone = true;

        // แสดง prompt ถ้ามี
        if (requireInput && interactPrompt != null)
        {
            interactPrompt.SetActive(true);
        }

        // ถ้าไม่ต้องกดปุ่ม ก็เริ่มเลย
        if (triggerOnEnter && !requireInput)
        {
            TriggerDialogue();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInZone = false;

        // ซ่อน prompt
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        // รีเซ็ต trigger ถ้าต้องการให้ออกก่อน
        if (requireExitBeforeRetrigger)
        {
            hasTriggered = false;
        }
    }

    void TriggerDialogue()
    {
        if (hasTriggered) return;

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueTreeManager not found!");
            return;
        }

        if (dialogueTree == null)
        {
            Debug.LogError("DialogueTree not assigned!");
            return;
        }

        // เริ่ม dialogue
        dialogueManager.StartDialogue(dialogueTree);

        hasTriggered = true;

        // ซ่อน prompt
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        // ปิดตัวเองถ้าต้องการใช้แค่ครั้งเดียว
        if (triggerOnce)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        // วาด trigger zone ใน Scene view
        Gizmos.color = Color.cyan;
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        // วาดเส้นไปยัง DialogueManager
        if (dialogueManager != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, dialogueManager.transform.position);
        }
    }
}
