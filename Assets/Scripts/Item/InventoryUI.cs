using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 1. (สำคัญ) ต้อง using UI เพื่อใช้ Text
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryPanel;
    public PlayerInteract playerInteract;
    public ItemDatabase itemDatabase;

    [Header("Slot Prefab")]
    public GameObject slotPrefab;
    public Transform slotContainer;

    // vvv (ของใหม่) เพิ่ม 2 บรรทัดนี้ vvv
    [Header("Description UI")]
    public GameObject descriptionPanel; // 2. ลาก "DescriptionPanel" มาใส่
    public TextMeshProUGUI descriptionText; // 3. ลาก "DescriptionText" มาใส่

    private bool isOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false);
        descriptionPanel.SetActive(false); // 4. ซ่อน description ด้วย
        isOpen = false;
    }

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            UpdateUI();
        }
        else
        {
            // 5. (ของใหม่) ถ้าปิดกระเป๋า ให้ซ่อน Description ด้วย
            HideDescription();
        }
    }

    // --- (อัปเกรด) ฟังก์ชัน UpdateUI ---
    public void UpdateUI()
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        if (playerInteract == null || itemDatabase == null) return;

        foreach (string itemID in playerInteract.inventory)
        {
            // 6. (อัปเกรด) เรียกใช้ฟังก์ชันใหม่จาก Database
            ItemData itemData = itemDatabase.GetItemDataByID(itemID);

            if (itemData != null)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotContainer);

                // 7. (อัปเกรด) ส่งข้อมูลไปให้สคริปต์ InventorySlot
                newSlot.GetComponent<InventorySlot>().Initialize(itemData.itemID, this);

                // 8. (อัปเกรด) ตั้งค่า Sprite จาก itemData
                Image slotImage = newSlot.GetComponent<Image>();
                slotImage.sprite = itemData.itemIcon;
                slotImage.color = Color.white;
            }
        }
    }

    // --- (ของใหม่) ฟังก์ชันสำหรับแสดง/ซ่อน Description ---
    public void ShowDescription(string itemID)
    {
        // 9. หาข้อมูลจาก Database
        ItemData itemData = itemDatabase.GetItemDataByID(itemID);
        if (itemData != null)
        {
            // 10. ตั้งค่าข้อความและเปิด UI
            descriptionText.text = itemData.description;
            descriptionPanel.SetActive(true);
            print ("Show description for");
        }
    }

    public void HideDescription()
    {
        // 11. ซ่อน UI
        descriptionText.text = ""; // เคลียร์ข้อความ (เผื่อบั๊ก)
        descriptionPanel.SetActive(false);
    }
}