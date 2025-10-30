using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // 1. (สำคัญ) ต้องใช้ Events

public class KeypadController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject keypadPanel; // 2. ลาก KeypadPanel มาใส่
    public TextMeshProUGUI codeDisplay; // 3. ลาก CodeDisplay (Text) มาใส่

    [Header("Keypad Settings")]
    public int maxCodeLength = 4; // รหัสยาวกี่ตัว

    // --- Events ---
    [HideInInspector]
    public UnityEvent OnSuccess; // 4. Event ยิงเมื่อ "สำเร็จ"
    [HideInInspector]
    public UnityEvent OnClose;   // 5. Event ยิงเมื่อ "ปิด"

    // --- Private State ---
    private string currentInput = "";
    private string correctCode;

    // ฟังก์ชันนี้จะถูกเรียกโดยปุ่มตัวเลข (0-9)
    public void DigitClick(string digit)
    {
        if (currentInput.Length < maxCodeLength)
        {
            currentInput += digit;
            UpdateDisplay();
        }
    }

    // ฟังก์ชันนี้ถูกเรียกโดยปุ่ม "Enter"
    public void EnterClick()
    {
        if (currentInput == correctCode)
        {
            Debug.Log("รหัสถูกต้อง!");
            OnSuccess.Invoke(); // ยิง Event สำเร็จ
            CloseKeypad();
        }
        else
        {
            Debug.Log("รหัสผิด! " + currentInput);
            // (ทางเลือก: เล่นเสียง "ผิด" หรือทำให้จอแดง)
            ClearClick();
        }
    }

    // ฟังก์ชันนี้ถูกเรียกโดยปุ่ม "Clear"
    public void ClearClick()
    {
        currentInput = "";
        UpdateDisplay();
    }

    // ฟังก์ชันนี้ถูกเรียกโดยปุ่ม "Exit"
    public void ExitClick()
    {
        OnClose.Invoke(); // ยิง Event ปิด
        CloseKeypad();
    }

    // --- ฟังก์ชันหลักที่ PlayerInteract จะเรียก ---
    public void ShowKeypad(string password)
    {
        correctCode = password; // จำรหัสที่ถูกต้องไว้
        this.gameObject.SetActive(true);
        ClearClick(); // เคลียร์หน้าจอ
    }

    private void CloseKeypad()
    {
        this.gameObject.SetActive(false);
        ClearClick();
    }

    private void UpdateDisplay()
    {
        codeDisplay.text = currentInput;
    }
}