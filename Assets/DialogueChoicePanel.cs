using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoicePanel : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup group;          // CanvasGroup บน ChoicePanel
    [SerializeField] private GameObject container;       // ChoiceContainer (ที่มีปุ่มเรียงใน VerticalLayoutGroup)
    [SerializeField] private Button[] buttons;           // ปุ่มตัวเลือก (ตามจำนวนที่มี)
    [SerializeField] private TMP_Text[] labels;          // ข้อความในปุ่ม (index ต้องตรงกับ buttons)

    // callback ที่จะถูกเรียกเมื่อเลือกเสร็จ
    private System.Action<int> onSelect;

    // ---------------------- Unity ----------------------
    void Awake()
    {
        // ensure CanvasGroup
        if (!group)
        {
            group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();
        }

        // auto-find ปุ่ม/ข้อความถ้าไม่กรอก (ปลอดภัย แต่แนะนำกรอกเองใน Inspector)
        if ((buttons == null || buttons.Length == 0) || (labels == null || labels.Length == 0))
        {
            var foundButtons = GetComponentsInChildren<Button>(true);
            var foundTexts = GetComponentsInChildren<TMP_Text>(true);
            if ((buttons == null || buttons.Length == 0) && foundButtons.Length > 0)
                buttons = foundButtons;
            if ((labels == null || labels.Length == 0) && foundTexts.Length > 0)
                labels = foundTexts;
        }

        // bind listeners ตามจำนวนที่ซิงก์กัน
        int count = Mathf.Min(buttons?.Length ?? 0, labels?.Length ?? 0);
        for (int i = 0; i < count; i++)
        {
            int idx = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => Select(idx));
        }

        HideImmediate();
    }

    // ---------------------- API ----------------------
    /// <summary>
    /// แสดงตัวเลือก (1..N) แล้วเรียก callback(i) เมื่อผู้เล่นกดเลือก
    /// </summary>
    public void Show(string[] choices, System.Action<int> callback)
    {
        if (choices == null || choices.Length == 0)
        {
            Debug.LogWarning("[ChoicePanel] Show called with EMPTY choices.");
            return;
        }
        if (buttons == null || labels == null || buttons.Length == 0 || labels.Length == 0)
        {
            Debug.LogError("[ChoicePanel] buttons/labels not assigned.");
            return;
        }

        // กัน array ไม่เท่ากัน
        int usable = Mathf.Min(Mathf.Min(buttons.Length, labels.Length), choices.Length);

        // เก็บ callback (ยอมรับ null ได้ แต่เราจะไม่ทำให้หายไปก่อนเรียก)
        onSelect = callback;

        // อัปเดตข้อความ + active ปุ่ม
        for (int i = 0; i < buttons.Length; i++)
        {
            bool active = i < usable;
            buttons[i].gameObject.SetActive(active);
            if (active) labels[i].text = choices[i];
        }

        // เปิด panel
        if (container && !container.activeSelf) container.SetActive(true);
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    /// <summary>ซ่อนแบบปกติ</summary>
    public void Hide()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (container && container.activeSelf) container.SetActive(false);
        if (gameObject.activeSelf) gameObject.SetActive(false);

        // ❌ อย่าเคลียร์ onSelect ที่นี่ (เดี๋ยว Select เรียกไม่ทัน)
        // onSelect = null;
    }

    /// <summary>ซ่อนทันที (ผลเท่ากับ Hide)</summary>
    public void HideImmediate() => Hide();

    // ---------------------- Internal ----------------------
    private void Select(int i)
    {
        // เก็บ callback ไว้ก่อน เพราะ Hide() จะไม่แตะ onSelect แต่กันเผื่อสคริปต์อื่นเรียกซ้ำ
        var cb = onSelect;

        // ซ่อนแผงก่อน เพื่อให้ UI หายทันที
        Hide();

        // เคลียร์ callback หลังใช้ (กัน callback ค้าง)
        onSelect = null;

        if (cb != null) cb.Invoke(i);
        else Debug.LogWarning("[ChoicePanel] onSelect is NULL (no callback).");
    }
}
