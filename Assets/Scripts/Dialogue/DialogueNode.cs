using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Node แต่ละอันใน Dialogue Tree สามารถเป็นได้หลายแบบ:
/// - Text: แสดงข้อความธรรมดา
/// - Choice: ให้ผู้เล่นเลือกคำตอบ
/// - Action: เรียก Cutscene/Events
/// - Condition: เช็คเงื่อนไข แล้วกระโดดไป node ต่างกัน
/// </summary>
[System.Serializable]
public class DialogueNode
{
    [Header("📌 Node Info")]
    public string nodeId; // Unique ID สำหรับอ้างอิง
    public NodeType nodeType = NodeType.Text;

    [Header("💬 Text Node (ถ้าเป็น Text/Choice)")]
    public string characterName;
    public Sprite portrait;
    [TextArea(2, 6)]
    public string dialogueText;

    [Header("🔀 Navigation")]
    [Tooltip("Node ถัดไปที่จะไป (สำหรับ Text node)")]
    public string nextNodeId;

    [Header("🎬 Actions (ใช้ได้กับทุก Node Type)")]
    [Tooltip("Actions ที่จะเล่นกับ Node นี้")]
    public List<DialogueAction> actions = new List<DialogueAction>();

    [Header("🔘 Choice Node (ถ้าเป็น Choice)")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Header("✅ Condition Node (ถ้าเป็น Condition)")]
    public DialogueCondition condition;
    [Tooltip("ไปที่ Node นี้ถ้าเงื่อนไขเป็นจริง")]
    public string trueNodeId;
    [Tooltip("ไปที่ Node นี้ถ้าเงื่อนไขเป็นเท็จ")]
    public string falseNodeId;

    [Header("🔚 End Node")]
    [Tooltip("เป็น Node สุดท้ายของ conversation นี้?")]
    public bool isEndNode = false;

    [Header("📝 Editor Note (ไม่มีผลในเกม)")]
    [TextArea(1, 3)]
    public string editorNote; // บันทึกเพื่อช่วยจำ
}

[System.Serializable]
public enum NodeType
{
    Text,       // แสดงข้อความธรรมดา
    Choice,     // ให้ผู้เล่นเลือก
    Action,     // เล่น cutscene/actions
    Condition   // เช็คเงื่อนไข แล้วแยกเส้นทาง
}

/// <summary>
/// ตัวเลือกใน Choice Node
/// </summary>
[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    [Tooltip("ไปที่ Node ไหนถ้าเลือกตัวเลือกนี้")]
    public string targetNodeId;
    
    [Header("🔒 Requirements (Optional)")]
    [Tooltip("ต้องมีเงื่อนไขอะไรถึงจะแสดงตัวเลือกนี้? (เว้นว่างถ้าไม่มี)")]
    public DialogueCondition requirementCondition;

    [Header("⚡ On Choice Selected Events")]
    [Tooltip("Actions ที่จะเล่นทันทีเมื่อเลือกตัวเลือกนี้")]
    public List<DialogueAction> onSelectActions = new List<DialogueAction>();
    
    [Tooltip("UnityEvent ที่จะเรียกเมื่อเลือกตัวเลือกนี้")]
    public UnityEvent onSelectEvent;
    
    [Header("📝 Variables (Optional)")]
    [Tooltip("ตั้งค่าตัวแปรเมื่อเลือกตัวเลือกนี้")]
    public List<VariableModifier> variableModifiers = new List<VariableModifier>();
}

/// <summary>
/// เงื่อนไขต่างๆ ที่ใช้ใน Dialogue
/// </summary>
[System.Serializable]
public class DialogueCondition
{
    public ConditionType type = ConditionType.HasItem;
    
    [Header("Item Check")]
    public string itemId;
    
    [Header("Variable Check")]
    public string variableName;
    public CompareOperator compareOperator = CompareOperator.Equals;
    public float compareValue;
    
    [Header("Quest Check")]
    public string questId;
    public QuestState questState = QuestState.Completed;
    
    [Header("Custom Check")]
    [Tooltip("ชื่อ Method ที่จะเรียก (ต้องมีใน DialogueManager หรือ GameManager)")]
    public string customMethodName;
}

[System.Serializable]
public enum ConditionType
{
    HasItem,            // มีไอเทมหรือไม่
    VariableCheck,      // เช็คค่าตัวแปร
    QuestStatus,        // สถานะของเควส
    Custom              // เรียก method ที่กำหนดเอง
}

[System.Serializable]
public enum CompareOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual
}

[System.Serializable]
public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed
}

/// <summary>
/// สำหรับแก้ไขค่าตัวแปรเมื่อเลือก Choice
/// </summary>
[System.Serializable]
public class VariableModifier
{
    public string variableName;
    public ModifyOperation operation = ModifyOperation.Set;
    public float value;
}

[System.Serializable]
public enum ModifyOperation
{
    Set,        // ตั้งค่าเป็น value
    Add,        // บวก value
    Subtract,   // ลบ value
    Multiply,   // คูณด้วย value
    Divide      // หารด้วย value
}
