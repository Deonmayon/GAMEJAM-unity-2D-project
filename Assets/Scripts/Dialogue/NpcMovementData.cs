using UnityEngine;

[System.Serializable]
public class NpcMovementData
{
    [Header("🧍 NPC to Move")]
    [Tooltip("ตัว NPC ที่จะเดินหลัง dialogue จบ")]
    public Transform npcTransform;

    [Header("📍 Destination")]
    [Tooltip("จุดหมายที่ NPC จะเดินไป")]
    public Transform destinationTransform;

    [Header("⚙️ Movement Settings")]
    [Tooltip("ความเร็วในการเดิน")]
    public float moveSpeed = 3f;

    [Tooltip("ระยะที่ถือว่าถึงจุดหมายแล้ว")]
    public float arrivalDistance = 0.1f;

    [Header("✨ Disappear Effect")]
    [Tooltip("ถ้าเปิด NPC จะหายไปเมื่อถึงจุดหมาย")]
    public bool disappearOnArrival = true;

    [Tooltip("เวลาที่รอก่อนหายไป (วินาที)")]
    public float disappearDelay = 0.5f;
}
