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

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
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
        // ^^^ จบส่วนที่เพิ่ม ^^^

        // (โค้ดเดิม... ย้ายตัวละครไปกลางตู้)
        transform.position = currentLocker.transform.position;
    }

    void UnHide()
    {
        Debug.Log("?????????????!");
        isHiding = false;

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