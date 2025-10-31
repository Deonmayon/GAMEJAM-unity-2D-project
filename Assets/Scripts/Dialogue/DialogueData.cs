using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("🚶 NPC Movement After Dialogue (Optional)")]
    [Tooltip("ถ้าต้องการให้ NPC เดินไปยังจุดหมายแล้วหายไปหลัง dialogue จบ")]
    public NpcMovementData npcMovement;
}
