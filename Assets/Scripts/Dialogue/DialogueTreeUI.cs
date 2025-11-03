using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI สำหรับแสดง DialogueTree
/// รองรับ: Text nodes, Choice nodes, Actions
/// </summary>
public class DialogueTreeUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Text Display")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Next Button (สำหรับ Text Node)")]
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Choice Buttons (สำหรับ Choice Node)")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonContainer;

    [Header("Typewriter Effect")]
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float typewriterSpeed = 0.05f;

    // Internal state
    private bool waitingForInput = false;
    private bool hasSelectedChoice = false;
    private DialogueChoice selectedChoice = null;
    private Coroutine typewriterCoroutine;
    private List<GameObject> activeChoiceButtons = new List<GameObject>();
    private bool typewriterComplete = false;

    void Start()
    {
        HideUI();
        
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    /// <summary>
    /// แสดง Text Node
    /// </summary>
    public void ShowTextNode(DialogueNode node)
    {
        if (node == null) return;

        // แสดง panel
        dialoguePanel.SetActive(true);
        choicePanel.SetActive(false);
        nextButton.gameObject.SetActive(true);

        // แสดงข้อมูล
        if (portraitImage != null)
            portraitImage.sprite = node.portrait;

        if (nameText != null)
            nameText.text = node.characterName;

        // แสดงข้อความ (typewriter หรือแบบปกติ)
        if (useTypewriter)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterComplete = false;
            typewriterCoroutine = StartCoroutine(TypewriterEffect(node.dialogueText));
        }
        else
        {
            dialogueText.text = node.dialogueText;
            typewriterComplete = true;
        }

        waitingForInput = true;
    }

    /// <summary>
    /// แสดง Choice Node
    /// </summary>
    public void ShowChoiceNode(DialogueNode node, List<DialogueChoice> availableChoices)
    {
        if (node == null || availableChoices == null) return;

        // แสดง panel
        dialoguePanel.SetActive(true);
        nextButton.gameObject.SetActive(false);
        choicePanel.SetActive(true);

        // แสดงข้อมูล
        if (portraitImage != null)
            portraitImage.sprite = node.portrait;

        if (nameText != null)
            nameText.text = node.characterName;

        if (dialogueText != null)
            dialogueText.text = node.dialogueText;

        // ลบ choice buttons เก่า
        ClearChoiceButtons();

        // สร้าง choice buttons ใหม่
        foreach (var choice in availableChoices)
        {
            CreateChoiceButton(choice);
        }

        hasSelectedChoice = false;
        selectedChoice = null;
    }

    /// <summary>
    /// สร้างปุ่มตัวเลือก
    /// </summary>
    void CreateChoiceButton(DialogueChoice choice)
    {
        if (choiceButtonPrefab == null || choiceButtonContainer == null)
        {
            Debug.LogError("Choice button prefab or container not assigned!");
            return;
        }

        GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        activeChoiceButtons.Add(buttonObj);

        // ตั้งค่าข้อความ
        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = choice.choiceText;
        }

        // ตั้งค่า onClick
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    /// <summary>
    /// ลบปุ่มตัวเลือกทั้งหมด
    /// </summary>
    void ClearChoiceButtons()
    {
        foreach (var button in activeChoiceButtons)
        {
            Destroy(button);
        }
        activeChoiceButtons.Clear();
    }

    /// <summary>
    /// เมื่อกดปุ่ม Next
    /// </summary>
    void OnNextButtonClicked()
    {
        // ถ้ากำลัง typewriter อยู่ ให้แสดงข้อความทั้งหมดทันที
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            dialogueText.text = dialogueText.text; // แสดงข้อความเต็ม
            typewriterCoroutine = null;
            return;
        }

        waitingForInput = false;
    }

    /// <summary>
    /// เมื่อเลือก Choice
    /// </summary>
    void OnChoiceSelected(DialogueChoice choice)
    {
        selectedChoice = choice;
        hasSelectedChoice = true;
    }

    /// <summary>
    /// Typewriter effect
    /// </summary>
    IEnumerator TypewriterEffect(string fullText)
    {
        dialogueText.text = "";
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        typewriterComplete = true;
        typewriterCoroutine = null;
    }

    /// <summary>
    /// ซ่อน UI
    /// </summary>
    public void HideUI()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        ClearChoiceButtons();
        waitingForInput = false;
        hasSelectedChoice = false;
        typewriterComplete = false;
    }

    // Getters สำหรับ Manager
    public bool IsWaitingForInput() => waitingForInput;
    public bool HasSelectedChoice() => hasSelectedChoice;
    public DialogueChoice GetSelectedChoice() => selectedChoice;
    public bool IsUsingTypewriter() => useTypewriter;
    public bool IsTypewriterComplete() => typewriterComplete;
}
