using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Lock Player While Talking")]
    [SerializeField] private PlayerMovement playerMovement;   // ✅ เพิ่มช่องอ้างอิง PlayerMovement
    [SerializeField] private Rigidbody2D playerRb;             // ✅ เพิ่มช่องอ้างอิง Rigidbody2D
    [SerializeField] private bool hardFreezePosition = true;   // ✅ หยุดแรงฟิสิกส์ด้วย (optional)

    private int index = 0;
    private bool isLocked = false;

    void Start()
    {
        panel.SetActive(false);
        nextButton.onClick.AddListener(NextLine);
    }

    public void StartDialogue(DialogueData newDialogue)
    {
        dialogue = newDialogue;
        index = 0;
        panel.SetActive(true);

        // 🔒 ล็อกการเคลื่อนไหว
        LockPlayer(true);

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

        // 🔓 ปลดล็อกเมื่อคุยจบ
        LockPlayer(false);
    }

    // 💡 ฟังก์ชันล็อกและปลดล็อกการเคลื่อนไหวของ Player
    void LockPlayer(bool lockIt)
    {
        if (playerMovement != null)
            playerMovement.enabled = !lockIt;

        if (playerRb != null && hardFreezePosition)
        {
            if (lockIt)
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
            else
                playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
