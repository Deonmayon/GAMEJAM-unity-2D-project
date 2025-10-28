using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-500)]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;      // DialoguePanel
    [SerializeField] private Image portraitImage;   // Portrait (optional)
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button clickCatcher;   // ปุ่มเต็มจอ คลิกเพื่อ Next
    [SerializeField] private DialogueChoicePanel choicePanel; // ← ลาก ChoicePanel มาวางที่นี่

    [Header("Behavior")]
    [SerializeField] private bool pauseGameDuringDialogue = false;

    [Header("Input (optional)")]
    [Tooltip("อ้างถึง PlayerInput ของตัวละคร ถ้ามี")]
    [SerializeField] private PlayerInput playerInput;
    [Tooltip("สลับไป Action Map UI ระหว่างคุย (ถ้า false จะ disable PlayerInput แทน)")]
    [SerializeField] private bool useUIActionMap = true;
    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";

    [Header("Choices (config)")]
    [Tooltip("ให้ขึ้นตัวเลือกที่บรรทัดที่เท่าไหร่ (เริ่มนับ 0) ใส่ -1 ถ้าไม่ใช้")]
    [SerializeField] private int choicesAtIndex = -1;
    [Tooltip("ข้อความตัวเลือก (1–3 ข้อก็ได้)")]
    [SerializeField] private string[] choices = new string[0];
    [Tooltip("index ปลายทางของแต่ละข้อ (-1 = ไปต่อปกติ) ความยาวควรเท่ากับ choices")]
    [SerializeField] private int[] gotoIndexPerChoice = new int[0];

    private DialogueData current;
    private int index;
    private bool isOpen;
    private bool waitingForChoice;   // กันการ Next ระหว่างรอผู้เล่นเลือก
    public bool IsOpen => isOpen;

    // Events
    public static event System.Action OnDialogueClosed;
    public static event System.Action<bool> OnDialogueStateChanged; // true = เปิดคุย, false = ปิดคุย

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // หา ChoicePanel อัตโนมัติ (Unity 6)
        if (!choicePanel)
            choicePanel = FindFirstObjectByType<DialogueChoicePanel>(FindObjectsInactive.Include);

        CloseImmediate();

        if (clickCatcher)
            clickCatcher.onClick.AddListener(Next);
    }

    void OnDestroy()
    {
        if (clickCatcher)
            clickCatcher.onClick.RemoveListener(Next);
    }

    void Update()
    {
        if (!isOpen) return;
        if (waitingForChoice) return; // รอเลือกอยู่ ห้าม Next

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        bool keyPressed =
            (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)) ||
            (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (keyPressed) Next();
#else
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0))
            Next();
#endif
    }

    // ===== API สำหรับตั้งค่า choices รายเคส (เช่น Enemy2 เรียกก่อน StartDialogue) =====
    public void ConfigureChoices(int atIndex, string[] options, int[] gotoIndexes = null)
    {
        choicesAtIndex = atIndex;
        choices = options ?? System.Array.Empty<string>();

        if (gotoIndexes != null && options != null && gotoIndexes.Length == options.Length)
            gotoIndexPerChoice = gotoIndexes;
        else
            gotoIndexPerChoice = new int[choices.Length];

        for (int i = 0; i < gotoIndexPerChoice.Length; i++)
            if (gotoIndexes == null || i >= gotoIndexes.Length) gotoIndexPerChoice[i] = -1; // -1 = ไปต่อปกติ
    }
    // ================================================================================

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.LogWarning("⚠️ StartDialogue called with EMPTY data.");
            return;
        }
        if (isOpen)
        {
            Debug.Log("ℹ️ Dialogue is already open, ignore StartDialogue.");
            return;
        }

        current = data;
        index = -1;
        isOpen = true;
        waitingForChoice = false;

        if (!panel) { Debug.LogError("❌ panel is NULL on DialogueManager."); return; }
        panel.SetActive(true);

        if (clickCatcher) clickCatcher.interactable = true;

        if (pauseGameDuringDialogue) Time.timeScale = 0f;

        if (playerInput)
        {
            if (useUIActionMap) playerInput.SwitchCurrentActionMap(uiMap);
            else playerInput.enabled = false;
        }

        OnDialogueStateChanged?.Invoke(true);

        Debug.Log($"✅ StartDialogue: {data.name}, lines={data.lines.Length}");
        Next();
    }

    public void Next()
    {
        if (!isOpen) return;
        if (waitingForChoice) return; // กัน Next ระหว่างมีช้อยส์

        index++;
        if (current == null || index >= current.lines.Length)
        {
            Close();
            return;
        }

        var line = current.lines[index];

        if (portraitImage)
        {
            bool hasPortrait = line.speaker != null && line.speaker.portrait != null;
            portraitImage.enabled = hasPortrait;
            if (hasPortrait) portraitImage.sprite = line.speaker.portrait;
        }

        if (nameText) nameText.text = line.speaker != null ? line.speaker.displayName : "";
        if (lineText) lineText.text = line.text;

        // จุดโชว์ตัวเลือก
        if (choicesAtIndex >= 0 &&
            index == choicesAtIndex &&
            choicePanel != null &&
            choices != null && choices.Length > 0)
        {
            waitingForChoice = true;
            if (clickCatcher) clickCatcher.interactable = false; // กันกดข้าม

            // กันพลาด: ถ้าไม่มี choicePanel/choices ให้ไปต่อ
            if (choicePanel == null || choices == null || choices.Length == 0)
            {
                Debug.LogWarning("[DialogueManager] No choicePanel / empty choices; continue.");
                waitingForChoice = false;
                if (clickCatcher) clickCatcher.interactable = true;
                return;
            }

            // ส่ง callback ภายในแน่นอน (ไม่เป็น null)
            choicePanel.Show(choices, OnChoiceSelected);
            return; // รอผู้เล่นเลือก
        }

        if (clickCatcher) clickCatcher.interactable = true;
    }

    private void OnChoiceSelected(int selected)
    {
        waitingForChoice = false;
        if (clickCatcher) clickCatcher.interactable = true;

        // ถ้ามีปลายทางเฉพาะตัวเลือกนี้ให้กระโดด index
        if (gotoIndexPerChoice != null &&
            selected >= 0 && selected < gotoIndexPerChoice.Length &&
            gotoIndexPerChoice[selected] >= 0 &&
            current != null &&
            gotoIndexPerChoice[selected] < current.lines.Length)
        {
            int target = gotoIndexPerChoice[selected];
            index = target - 1; // เพราะ Next() จะ ++ อีกครั้ง
            Next();
            return;
        }

        // ไม่ได้กำหนดปลายทาง → ไปต่อถัดไป
        Next();
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        waitingForChoice = false;

        if (panel) panel.SetActive(false);
        if (clickCatcher) clickCatcher.interactable = false;
        current = null;

        if (pauseGameDuringDialogue) Time.timeScale = 1f;

        if (playerInput)
        {
            if (useUIActionMap) playerInput.SwitchCurrentActionMap(gameplayMap);
            else playerInput.enabled = true;
        }

        OnDialogueClosed?.Invoke();
        OnDialogueStateChanged?.Invoke(false);
    }

    /// โชว์เฉพาะ ChoicePanel (ไม่มี DialoguePanel)
    public void ShowChoiceOnly(string[] options, System.Action<int> onPick = null, bool alsoPauseGame = false)
    {
        // ป้องกัน null/empty
        if (choicePanel == null)
        {
            Debug.LogError("[DialogueManager] choicePanel is NULL.");
            return;
        }
        if (options == null || options.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] ShowChoiceOnly called with EMPTY options.");
            return;
        }

        // ไม่โชว์ DialoguePanel
        if (panel) panel.SetActive(false);

        // จัดสถานะเกม/อินพุตตามที่เคยทำตอนคุย
        if (alsoPauseGame && pauseGameDuringDialogue) Time.timeScale = 0f;
        if (playerInput)
        {
            if (useUIActionMap) playerInput.SwitchCurrentActionMap(uiMap);
            else playerInput.enabled = false;
        }

        // กันกดข้าม และตั้งสถานะภายใน
        isOpen = true;                // ให้ Update กัน key next ขณะ choice-only
        waitingForChoice = true;
        if (clickCatcher) clickCatcher.interactable = false;

        // ทำให้ onPick ไม่เป็น null (fallback จะคืนอินพุต/เวลาคืน)
        System.Action<int> safePick = onPick ?? (_ =>
        {
            Debug.LogWarning("[DialogueManager] ShowChoiceOnly without callback → fallback unlock.");
        });

        choicePanel.Show(options, i =>
        {
            // ปิดสถานะรอ
            waitingForChoice = false;
            if (clickCatcher) clickCatcher.interactable = true;

            // คืนสถานะเกม/อินพุต
            if (alsoPauseGame && pauseGameDuringDialogue) Time.timeScale = 1f;
            if (playerInput)
            {
                if (useUIActionMap) playerInput.SwitchCurrentActionMap(gameplayMap);
                else playerInput.enabled = true;
            }

            // เรียก callback ผู้ใช้ (ไม่ null แล้ว)
            safePick(i);

            // ปิด state ภายในของ choice-only
            isOpen = false;
        });
    }

    private void CloseImmediate()
    {
        isOpen = false;
        waitingForChoice = false;
        if (panel) panel.SetActive(false);
        if (clickCatcher) clickCatcher.interactable = false;
        current = null;
    }
}
