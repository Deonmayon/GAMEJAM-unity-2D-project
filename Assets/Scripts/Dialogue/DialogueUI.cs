using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text buttonLabel;

    [Header("Data")]
    public DialogueData dialogue;
    
    private NpcMovementData currentNpcMovement; // เก็บข้อมูล NPC movement ที่ส่งมาจาก Trigger

    [Header("Lock Player While Talking")]
    [Tooltip("สคริปต์ควบคุมการเคลื่อนไหวของ Player เช่น PlayerMovement")]
    [SerializeField] private MonoBehaviour[] movementScriptsToDisable;

    [Tooltip("Rigidbody2D ของ Player (ถ้ามี)")]
    [SerializeField] private Rigidbody2D playerRb;

    [Tooltip("ถ้าเปิด จะ Freeze ตำแหน่ง Player ทั้งหมดระหว่างคุย")]
    [SerializeField] private bool hardFreezePosition = true;

    private int index = 0;

    void Start()
    {
        panel.SetActive(false);
        nextButton.onClick.AddListener(NextLine);
    }

    // Overload 1: แบบเดิม (backward compatible)
    public void StartDialogue(DialogueData newDialogue)
    {
        StartDialogue(newDialogue, null);
    }

    // Overload 2: แบบใหม่ (รับ NPC movement data)
    public void StartDialogue(DialogueData newDialogue, NpcMovementData npcMovement)
    {
        dialogue = newDialogue;
        currentNpcMovement = npcMovement; // เก็บข้อมูล NPC movement
        index = 0;
        panel.SetActive(true);

        LockPlayer(true); // 🔒 ล็อกขา

        ShowLine();
    }

    void ShowLine()
    {
        if (dialogue == null || dialogue.lines.Count == 0) return;

        var current = dialogue.lines[index];
        portraitImage.sprite = current.portrait;
        nameText.text = current.characterName;
        lineText.text = current.lineText;
        buttonLabel.text = current.buttonText;
        nextButton.gameObject.SetActive(current.showButton);
    }

    void NextLine()
    {
        index++;
        if (index >= dialogue.lines.Count)
        {
            EndDialogue();
            return;
        }
        ShowLine();
    }

    void EndDialogue()
    {
        panel.SetActive(false);
        LockPlayer(false); // 🔓 ปลดล็อกขา

        // ใช้ข้อมูล NPC movement ที่ส่งมาจาก Trigger (ถ้ามี)
        if (currentNpcMovement != null && currentNpcMovement.npcTransform != null)
        {
            var npcController = currentNpcMovement.npcTransform.GetComponent<NpcController>();
            if (npcController == null)
            {
                npcController = currentNpcMovement.npcTransform.gameObject.AddComponent<NpcController>();
            }
            npcController.StartMovement(currentNpcMovement);
            currentNpcMovement = null; // ล้างข้อมูลหลังใช้งาน
        }
        // ถ้าไม่มี ให้ลองใช้จาก DialogueData แทน (วิธีเก่า)
        else if (dialogue != null && dialogue.npcMovement != null && dialogue.npcMovement.npcTransform != null)
        {
            var npcController = dialogue.npcMovement.npcTransform.GetComponent<NpcController>();
            if (npcController == null)
            {
                npcController = dialogue.npcMovement.npcTransform.gameObject.AddComponent<NpcController>();
            }
            npcController.StartMovement(dialogue.npcMovement);
        }
    }

    // 🔧 ฟังก์ชันล็อก/ปลดล็อก Player
    private void LockPlayer(bool state)
    {
        // ปิด/เปิดสคริปต์ควบคุมการเดิน
        if (movementScriptsToDisable != null)
        {
            foreach (var script in movementScriptsToDisable)
            {
                if (script != null)
                    script.enabled = !state;
            }
        }

        // ถ้ามี Rigidbody ให้ Freeze ไว้
        if (playerRb != null)
        {
            if (hardFreezePosition)
            {
                playerRb.constraints = state
                    ? RigidbodyConstraints2D.FreezeAll
                    : RigidbodyConstraints2D.FreezeRotation;
            }

            // หยุดความเร็วทันที
            if (state) playerRb.linearVelocity = Vector2.zero;
        }

        // ถ้ามีฟังก์ชัน SetCanMove ใน PlayerMovement → เรียกด้วย
        if (playerRb != null)
        {
            playerRb.gameObject.SendMessage("SetCanMove", !state, SendMessageOptions.DontRequireReceiver);
        }
    }
}
