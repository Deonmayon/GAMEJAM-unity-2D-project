using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DialogueTree - ระบบ Dialogue แบบใหม่ที่รองรับ:
/// - แยกสาขาได้ (Branching)
/// - มีตัวเลือกให้ผู้เล่นเลือก (Choices)
/// - เช็คเงื่อนไขได้ (Conditions)
/// - เล่น Cutscene/Actions ได้
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Dialogue/DialogueTree")]
public class DialogueTree : ScriptableObject
{
    [Header("📋 Tree Info")]
    [Tooltip("ชื่อของ Dialogue Tree นี้")]
    public string treeName;
    
    [TextArea(2, 4)]
    public string description; // อธิบายว่า tree นี้ใช้ทำอะไร

    [Header("🌳 Node Tree")]
    [Tooltip("Node แรกที่จะเริ่มต้น (ต้องมี)")]
    public string startNodeId = "start";
    
    [Tooltip("รายการ Node ทั้งหมดใน Tree นี้")]
    public List<DialogueNode> nodes = new List<DialogueNode>();

    [Header("🔧 Settings")]
    [Tooltip("ล็อกการเคลื่อนไหวของ Player ขณะเล่น dialogue นี้?")]
    public bool lockPlayerMovement = true;
    
    [Tooltip("ซ่อน UI อื่นๆ ขณะเล่น dialogue?")]
    public bool hideOtherUI = false;

    [Header("📝 Variables (สำหรับเก็บ state)")]
    [Tooltip("ตัวแปรที่ใช้ใน Tree นี้")]
    public List<DialogueVariable> variables = new List<DialogueVariable>();

    /// <summary>
    /// หา Node จาก ID
    /// </summary>
    public DialogueNode GetNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
            return null;

        return nodes.Find(n => n.nodeId == nodeId);
    }

    /// <summary>
    /// ตรวจสอบว่า Tree นี้มี errors หรือไม่
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        // เช็คว่ามี start node หรือไม่
        if (GetNode(startNodeId) == null)
        {
            errors.Add($"Start node '{startNodeId}' not found!");
        }

        // เช็คว่าทุก node มี ID ที่ไม่ซ้ำกัน
        HashSet<string> ids = new HashSet<string>();
        foreach (var node in nodes)
        {
            if (string.IsNullOrEmpty(node.nodeId))
            {
                errors.Add("Found node with empty ID!");
                continue;
            }

            if (ids.Contains(node.nodeId))
            {
                errors.Add($"Duplicate node ID: {node.nodeId}");
            }
            ids.Add(node.nodeId);
        }

        // เช็คว่า next node ที่อ้างถึงมีอยู่จริงหรือไม่
        foreach (var node in nodes)
        {
            if (!string.IsNullOrEmpty(node.nextNodeId) && GetNode(node.nextNodeId) == null)
            {
                errors.Add($"Node '{node.nodeId}' references non-existent node '{node.nextNodeId}'");
            }

            // เช็ค choices
            if (node.nodeType == NodeType.Choice)
            {
                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.targetNodeId) && GetNode(choice.targetNodeId) == null)
                    {
                        errors.Add($"Choice in '{node.nodeId}' references non-existent node '{choice.targetNodeId}'");
                    }
                }
            }

            // เช็ค condition nodes
            if (node.nodeType == NodeType.Condition)
            {
                if (!string.IsNullOrEmpty(node.trueNodeId) && GetNode(node.trueNodeId) == null)
                {
                    errors.Add($"Condition '{node.nodeId}' trueNode references non-existent node '{node.trueNodeId}'");
                }
                if (!string.IsNullOrEmpty(node.falseNodeId) && GetNode(node.falseNodeId) == null)
                {
                    errors.Add($"Condition '{node.nodeId}' falseNode references non-existent node '{node.falseNodeId}'");
                }
            }
        }

        return errors.Count == 0;
    }

#if UNITY_EDITOR
    [ContextMenu("Validate Tree")]
    void ValidateInEditor()
    {
        List<string> errors;
        if (Validate(out errors))
        {
            Debug.Log($"✅ DialogueTree '{treeName}' is valid!");
        }
        else
        {
            Debug.LogError($"❌ DialogueTree '{treeName}' has errors:");
            foreach (var error in errors)
            {
                Debug.LogError($"  - {error}");
            }
        }
    }

    [ContextMenu("Create Example Tree")]
    void CreateExampleTree()
    {
        treeName = "Example Conversation";
        startNodeId = "start";
        nodes.Clear();

        // Node 1: Welcome
        nodes.Add(new DialogueNode
        {
            nodeId = "start",
            nodeType = NodeType.Text,
            characterName = "NPC",
            dialogueText = "Hello there! How can I help you?",
            nextNodeId = "choice1"
        });

        // Node 2: Choice
        var choiceNode = new DialogueNode
        {
            nodeId = "choice1",
            nodeType = NodeType.Choice,
            characterName = "NPC",
            dialogueText = "What would you like to know?",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Tell me about yourself", targetNodeId = "about" },
                new DialogueChoice { choiceText = "I need help", targetNodeId = "help" },
                new DialogueChoice { choiceText = "Goodbye", targetNodeId = "end" }
            }
        };
        nodes.Add(choiceNode);

        // Node 3: About
        nodes.Add(new DialogueNode
        {
            nodeId = "about",
            nodeType = NodeType.Text,
            characterName = "NPC",
            dialogueText = "I'm just a simple NPC in this game world!",
            nextNodeId = "end"
        });

        // Node 4: Help
        nodes.Add(new DialogueNode
        {
            nodeId = "help",
            nodeType = NodeType.Text,
            characterName = "NPC",
            dialogueText = "Sure, I'm here to help! What do you need?",
            nextNodeId = "end"
        });

        // Node 5: End
        nodes.Add(new DialogueNode
        {
            nodeId = "end",
            nodeType = NodeType.Text,
            characterName = "NPC",
            dialogueText = "Take care! See you around!",
            isEndNode = true
        });

        Debug.Log("✨ Example tree created! Remember to save the asset.");
    }
#endif
}

/// <summary>
/// ตัวแปรที่ใช้ใน Dialogue (เช่น karma, friendship level, etc.)
/// </summary>
[System.Serializable]
public class DialogueVariable
{
    public string variableName;
    public VariableType type = VariableType.Integer;
    public float numericValue;
    public string stringValue;
    public bool boolValue;
}

[System.Serializable]
public enum VariableType
{
    Integer,
    Float,
    String,
    Bool
}
