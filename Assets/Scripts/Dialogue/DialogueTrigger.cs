using UnityEngine;

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

    // ----- internal -----
    private bool playerInside = false;
    private bool triggeredWhileInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;

        // ถ้ายังยืนค้างอยู่ในโซนเดิม และตั้งไว้ให้ต้องออกก่อน → ไม่ให้ทริกซ้ำ
        if (requireExitBeforeRetrigger && triggeredWhileInside) return;

        FireDialogue();
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

        dialogueUI.StartDialogue(dialogueData);
        triggeredWhileInside = true; // จดว่า รอบนี้ได้ทริกไปแล้วขณะอยู่วง
    }
}
