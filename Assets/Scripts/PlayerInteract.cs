using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private bool canInteract = false;
    private bool isHiding = false;
    private GameObject currentLocker;

    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public GameObject flashlightObject; 
    public QTEManager qteManager; // เพิ่ม reference ไป QTEManager 

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-find QTEManager ถ้าไม่ได้กำหนด
        if (qteManager == null)
        {
            qteManager = FindObjectOfType<QTEManager>();
            if (qteManager != null)
            {
                Debug.Log("🎯 Auto-found QTEManager: " + qteManager.gameObject.name);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Locker"))
        {
            canInteract = true;        
            currentLocker = other.gameObject; 
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Locker"))
        {
            canInteract = false; 
            currentLocker = null;
        }
    }

    void Update()
    {
        if (canInteract && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isHiding)
            {
                UnHide();
            }
            else
            {
                Hide();
            }
        }
    }

    void Hide()
    {
        Debug.Log("กำลังซ่อนตัว!");
        isHiding = true;

        // (โค้ดเดิม... ปิด playerMovement, rb.velocity, spriteRenderer)
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.enabled = false;

        // vvv เพิ่มส่วนนี้ vvv
        // ซ่อนไฟฉายและแสงรอบตัว
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }
        
        // เริ่ม QTE เมื่อซ่อนตัว
        if (qteManager != null)
        {
            Debug.Log("🎮 PlayerInteract: กำลังเรียก StartQTE()");
            qteManager.StartQTE();
        }
        else
        {
            Debug.LogError("❌ QTEManager ไม่พบ! ไม่สามารถเริ่ม QTE ได้");
        }
        // ^^^ จบส่วนที่เพิ่ม ^^^

        // (โค้ดเดิม... ย้ายตัวละครไปกลางตู้)
        transform.position = currentLocker.transform.position;
    }

    void UnHide()
    {
        Debug.Log("?????????????!");
        isHiding = false;

        // หยุด QTE เมื่อออกจากตู้
        if (qteManager != null)
        {
            Debug.Log("🛑 PlayerInteract: กำลังเรียก StopQTE()");
            qteManager.StopQTE();
        }
        else
        {
            Debug.LogError("❌ QTEManager ไม่พบ! ไม่สามารถหยุด QTE ได้");
        }

        // 1. ????????????????????????
        playerMovement.enabled = true;
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(true);
        }

        // 2. ????????????????????
        spriteRenderer.enabled = true;

        // (???????) ?????????????????????
        // flashlightTransform.gameObject.SetActive(true);
        // playerAmbient.gameObject.SetActive(true);
    }
}