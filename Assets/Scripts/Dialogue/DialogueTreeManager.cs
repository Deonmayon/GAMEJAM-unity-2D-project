using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// DialogueTreeManager - จัดการ Dialogue Tree ทั้งหมด
/// รองรับ: branching, conditions, choices, actions, variables
/// </summary>
public class DialogueTreeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueTreeUI dialogueUI;

    [Header("Current State")]
    private DialogueTree currentTree;
    private DialogueNode currentNode;
    private Dictionary<string, DialogueVariable> treeVariables = new Dictionary<string, DialogueVariable>();

    [Header("Action Executor")]
    private DialogueActionExecutor actionExecutor;

    [Header("Player Control")]
    [SerializeField] private MonoBehaviour[] playerMovementScripts;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Game State (ถ้ามี GameManager)")]
    [Tooltip("ถ้ามี InventoryManager/QuestManager จะใช้เช็คเงื่อนไข")]
    public GameObject gameManagerObject;

    // Events
    public UnityEvent<DialogueTree> onDialogueStart;
    public UnityEvent<DialogueTree> onDialogueEnd;
    public UnityEvent<DialogueNode> onNodeChange;

    private bool isDialogueActive = false;

    void Awake()
    {
        if (actionExecutor == null)
        {
            actionExecutor = gameObject.AddComponent<DialogueActionExecutor>();
        }
    }

    /// <summary>
    /// เริ่มต้น Dialogue Tree
    /// </summary>
    public void StartDialogue(DialogueTree tree)
    {
        if (tree == null)
        {
            Debug.LogError("Cannot start dialogue: tree is null");
            return;
        }

        // Validate tree
        List<string> errors;
        if (!tree.Validate(out errors))
        {
            Debug.LogError($"Cannot start dialogue tree '{tree.treeName}': validation failed");
            foreach (var error in errors)
            {
                Debug.LogError($"  - {error}");
            }
            return;
        }

        currentTree = tree;
        isDialogueActive = true;

        // Load variables
        LoadTreeVariables();

        // Lock player ถ้าต้องการ
        if (tree.lockPlayerMovement)
        {
            LockPlayer(true);
        }

        // Hide UI ถ้าต้องการ
        if (tree.hideOtherUI)
        {
            // TODO: ซ่อน UI อื่นๆ
        }

        // เริ่มที่ start node
        var startNode = tree.GetNode(tree.startNodeId);
        if (startNode != null)
        {
            onDialogueStart?.Invoke(tree);
            StartCoroutine(ProcessNode(startNode));
        }
        else
        {
            Debug.LogError($"Start node '{tree.startNodeId}' not found!");
            EndDialogue();
        }
    }

    /// <summary>
    /// ประมวลผล node ตาม type
    /// </summary>
    IEnumerator ProcessNode(DialogueNode node)
    {
        if (node == null) yield break;

        currentNode = node;
        onNodeChange?.Invoke(node);

        switch (node.nodeType)
        {
            case NodeType.Text:
                yield return ProcessTextNode(node);
                break;

            case NodeType.Choice:
                yield return ProcessChoiceNode(node);
                break;

            case NodeType.Action:
                yield return ProcessActionNode(node);
                break;

            case NodeType.Condition:
                yield return ProcessConditionNode(node);
                break;
        }
    }

    /// <summary>
    /// แสดง Text Node
    /// </summary>
    IEnumerator ProcessTextNode(DialogueNode node)
    {
        // Execute OnNodeEnter actions
        yield return ExecuteActionsWithTiming(node, ActionTiming.OnNodeEnter);

        // แสดงใน UI
        dialogueUI.ShowTextNode(node);

        // Execute OnTextComplete actions (หลังจาก typewriter เสร็จ)
        if (dialogueUI.IsUsingTypewriter())
        {
            yield return new WaitUntil(() => dialogueUI.IsTypewriterComplete());
            yield return ExecuteActionsWithTiming(node, ActionTiming.OnTextComplete);
        }

        // รอให้ผู้เล่นกด next
        yield return new WaitUntil(() => dialogueUI.IsWaitingForInput() == false);

        // Execute OnNodeExit actions
        yield return ExecuteActionsWithTiming(node, ActionTiming.OnNodeExit);

        // ไปยัง node ถัดไป หรือจบ
        if (node.isEndNode)
        {
            EndDialogue();
        }
        else if (!string.IsNullOrEmpty(node.nextNodeId))
        {
            var nextNode = currentTree.GetNode(node.nextNodeId);
            yield return ProcessNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// แสดง Choice Node
    /// </summary>
    IEnumerator ProcessChoiceNode(DialogueNode node)
    {
        // Execute OnNodeEnter actions
        yield return ExecuteActionsWithTiming(node, ActionTiming.OnNodeEnter);

        // กรองตัวเลือกที่ตรงเงื่อนไข
        List<DialogueChoice> availableChoices = new List<DialogueChoice>();
        foreach (var choice in node.choices)
        {
            // เช็คว่าตรงเงื่อนไขหรือไม่
            if (choice.requirementCondition == null || EvaluateCondition(choice.requirementCondition))
            {
                availableChoices.Add(choice);
            }
        }

        if (availableChoices.Count == 0)
        {
            Debug.LogWarning($"No available choices in node '{node.nodeId}'");
            EndDialogue();
            yield break;
        }

        // แสดง choices ใน UI
        dialogueUI.ShowChoiceNode(node, availableChoices);

        // รอให้ผู้เล่นเลือก
        yield return new WaitUntil(() => dialogueUI.HasSelectedChoice());

        // ดึง choice ที่เลือก
        var selectedChoice = dialogueUI.GetSelectedChoice();
        
        if (selectedChoice != null)
        {
            // Execute choice events
            yield return ExecuteChoiceEvents(selectedChoice);

            // Execute OnNodeExit actions
            yield return ExecuteActionsWithTiming(node, ActionTiming.OnNodeExit);

            // ไปยัง node ถัดไป
            var nextNode = currentTree.GetNode(selectedChoice.targetNodeId);
            yield return ProcessNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// ประมวลผล Action Node
    /// </summary>
    IEnumerator ProcessActionNode(DialogueNode node)
    {
        // Execute OnNodeEnter actions
        yield return ExecuteActionsWithTiming(node, ActionTiming.OnNodeEnter);

        // Execute OnNodeExit actions
        yield return ExecuteActionsWithTiming(node, ActionTiming.OnNodeExit);

        // ไปยัง node ถัดไป
        if (node.isEndNode)
        {
            EndDialogue();
        }
        else if (!string.IsNullOrEmpty(node.nextNodeId))
        {
            var nextNode = currentTree.GetNode(node.nextNodeId);
            yield return ProcessNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// ประมวลผล Condition Node
    /// </summary>
    IEnumerator ProcessConditionNode(DialogueNode node)
    {
        bool conditionResult = false;

        if (node.condition != null)
        {
            conditionResult = EvaluateCondition(node.condition);
        }

        // เลือก path ตามผลลัพธ์
        string nextNodeId = conditionResult ? node.trueNodeId : node.falseNodeId;

        if (!string.IsNullOrEmpty(nextNodeId))
        {
            var nextNode = currentTree.GetNode(nextNodeId);
            yield return ProcessNode(nextNode);
        }
        else
        {
            Debug.LogWarning($"Condition node '{node.nodeId}' has no valid path for result: {conditionResult}");
            EndDialogue();
        }
    }

    /// <summary>
    /// ประเมินเงื่อนไข
    /// </summary>
    bool EvaluateCondition(DialogueCondition condition)
    {
        if (condition == null) return true;

        switch (condition.type)
        {
            case ConditionType.VariableCheck:
                return EvaluateVariableCondition(condition);

            case ConditionType.Custom:
                return EvaluateCustomCondition(condition);

            case ConditionType.HasItem:
                // TODO: เชื่อมกับ Inventory System
                Debug.LogWarning("HasItem condition not implemented yet");
                return false;

            case ConditionType.QuestStatus:
                // TODO: เชื่อมกับ Quest System
                Debug.LogWarning("QuestStatus condition not implemented yet");
                return false;

            default:
                return true;
        }
    }

    bool EvaluateVariableCondition(DialogueCondition condition)
    {
        if (!treeVariables.ContainsKey(condition.variableName))
        {
            Debug.LogWarning($"Variable '{condition.variableName}' not found");
            return false;
        }

        var variable = treeVariables[condition.variableName];
        float value = variable.numericValue;
        float compareValue = condition.compareValue;

        switch (condition.compareOperator)
        {
            case CompareOperator.Equals:
                return Mathf.Approximately(value, compareValue);
            case CompareOperator.NotEquals:
                return !Mathf.Approximately(value, compareValue);
            case CompareOperator.GreaterThan:
                return value > compareValue;
            case CompareOperator.LessThan:
                return value < compareValue;
            case CompareOperator.GreaterOrEqual:
                return value >= compareValue;
            case CompareOperator.LessOrEqual:
                return value <= compareValue;
            default:
                return false;
        }
    }

    bool EvaluateCustomCondition(DialogueCondition condition)
    {
        if (gameManagerObject == null || string.IsNullOrEmpty(condition.customMethodName))
            return false;

        // ลองเรียก method จาก GameManager
        // หมายเหตุ: method นั้นต้อง return bool และเป็น public
        // ตัวอย่าง: public bool HasCompletedTutorial() { return true; }
        
        try
        {
            var component = gameManagerObject.GetComponent<MonoBehaviour>();
            if (component != null)
            {
                var method = component.GetType().GetMethod(condition.customMethodName);
                if (method != null && method.ReturnType == typeof(bool))
                {
                    var result = method.Invoke(component, null);
                    return (bool)result;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to evaluate custom condition '{condition.customMethodName}': {e.Message}");
        }

        return false;
    }

    /// <summary>
    /// จบ Dialogue
    /// </summary>
    void EndDialogue()
    {
        if (!isDialogueActive) return;

        // Execute OnDialogueEnd actions จาก node สุดท้าย
        if (currentNode != null)
        {
            StartCoroutine(ExecuteActionsWithTiming(currentNode, ActionTiming.OnDialogueEnd));
        }

        isDialogueActive = false;

        // ปลดล็อก player
        if (currentTree != null && currentTree.lockPlayerMovement)
        {
            LockPlayer(false);
        }

        // ซ่อน UI
        dialogueUI.HideUI();

        onDialogueEnd?.Invoke(currentTree);

        currentTree = null;
        currentNode = null;
    }

    /// <summary>
    /// Execute actions ตาม timing ที่กำหนด
    /// </summary>
    IEnumerator ExecuteActionsWithTiming(DialogueNode node, ActionTiming timing)
    {
        if (node.actions == null || node.actions.Count == 0)
            yield break;

        // กรอง actions ตาม timing
        var actionsToExecute = node.actions.FindAll(a => a.timing == timing);
        
        foreach (var action in actionsToExecute)
        {
            if (action.waitForCompletion)
            {
                yield return actionExecutor.ExecuteAction(action);
            }
            else
            {
                // Fire and forget
                StartCoroutine(actionExecutor.ExecuteAction(action));
            }
        }
    }

    /// <summary>
    /// Execute events เมื่อเลือก Choice
    /// </summary>
    IEnumerator ExecuteChoiceEvents(DialogueChoice choice)
    {
        if (choice == null) yield break;

        // 1. Execute actions
        if (choice.onSelectActions != null && choice.onSelectActions.Count > 0)
        {
            foreach (var action in choice.onSelectActions)
            {
                if (action.waitForCompletion)
                {
                    yield return actionExecutor.ExecuteAction(action);
                }
                else
                {
                    StartCoroutine(actionExecutor.ExecuteAction(action));
                }
            }
        }

        // 2. Invoke UnityEvent
        choice.onSelectEvent?.Invoke();

        // 3. Modify variables
        if (choice.variableModifiers != null && choice.variableModifiers.Count > 0)
        {
            foreach (var modifier in choice.variableModifiers)
            {
                ModifyVariable(modifier);
            }
        }
    }

    /// <summary>
    /// แก้ไขค่าตัวแปร
    /// </summary>
    void ModifyVariable(VariableModifier modifier)
    {
        if (!treeVariables.ContainsKey(modifier.variableName))
        {
            Debug.LogWarning($"Variable '{modifier.variableName}' not found");
            return;
        }

        var variable = treeVariables[modifier.variableName];
        float currentValue = variable.numericValue;

        switch (modifier.operation)
        {
            case ModifyOperation.Set:
                variable.numericValue = modifier.value;
                break;
            case ModifyOperation.Add:
                variable.numericValue = currentValue + modifier.value;
                break;
            case ModifyOperation.Subtract:
                variable.numericValue = currentValue - modifier.value;
                break;
            case ModifyOperation.Multiply:
                variable.numericValue = currentValue * modifier.value;
                break;
            case ModifyOperation.Divide:
                if (modifier.value != 0)
                    variable.numericValue = currentValue / modifier.value;
                break;
        }

        Debug.Log($"Variable '{modifier.variableName}' modified: {currentValue} → {variable.numericValue}");
    }

    /// <summary>
    /// จัดการตัวแปร
    /// </summary>
    void LoadTreeVariables()
    {
        treeVariables.Clear();
        if (currentTree != null && currentTree.variables != null)
        {
            foreach (var variable in currentTree.variables)
            {
                treeVariables[variable.variableName] = variable;
            }
        }
    }

    public void SetVariable(string varName, float value)
    {
        if (treeVariables.ContainsKey(varName))
        {
            treeVariables[varName].numericValue = value;
        }
    }

    public float GetVariable(string varName)
    {
        if (treeVariables.ContainsKey(varName))
        {
            return treeVariables[varName].numericValue;
        }
        return 0f;
    }

    /// <summary>
    /// ล็อก/ปลดล็อก Player
    /// </summary>
    void LockPlayer(bool locked)
    {
        // ปิด/เปิดสคริปต์
        if (playerMovementScripts != null)
        {
            foreach (var script in playerMovementScripts)
            {
                if (script != null)
                    script.enabled = !locked;
            }
        }

        // Freeze Rigidbody
        if (playerRb != null)
        {
            if (locked)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    public bool IsDialogueActive() => isDialogueActive;
    public DialogueNode GetCurrentNode() => currentNode;
    public DialogueTree GetCurrentTree() => currentTree;
}
