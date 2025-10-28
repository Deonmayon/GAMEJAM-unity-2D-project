using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyItem : MonoBehaviour
{
    [SerializeField] private string keyName = "Key";       // ชื่อกุญแจ
    [SerializeField] private string playerTag = "Player";  // Tag ของ Player
    [SerializeField] private GameObject enemyObject;       // ศัตรูที่จะ "ปิดบทสนทนา"

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var inventory = other.GetComponent<PlayerInventory>();
        if (!inventory) return;

        // 1) บันทึกคีย์
        inventory.PickupKey(keyName);
        Debug.Log($"🗝️ เก็บกุญแจ: {keyName}");

        // 2) ปิดระบบสนทนา + ปิดคอลลิเดอร์ของศัตรูทั้งก้อน (parent + children)
        if (enemyObject != null)
        {
            // ปิดสคริปต์ DialogueTriggerOnCollision ทุกตัวในลำต้น
            var triggers = enemyObject.GetComponentsInChildren<DialogueTriggerOnCollision>(true);
            foreach (var t in triggers) t.enabled = false;

            // ปิดคอลลิเดอร์แบบ trigger ทั้งหมด กันยิง OnTriggerEnter/Stay ซ้ำ
            var cols = enemyObject.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols) c.enabled = false;

            Debug.Log($"🤐 ปิดบทสนทนาและตัวชนของ {enemyObject.name} ทั้งหมดแล้ว");
        }
        else
        {
            Debug.LogWarning("⚠️ enemyObject ยังไม่ถูกอ้างใน Inspector");
        }

        // 3) ลบตัวกุญแจเอง
        Destroy(gameObject);
    }
}
