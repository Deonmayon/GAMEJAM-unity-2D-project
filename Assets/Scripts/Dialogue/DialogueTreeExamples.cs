using UnityEngine;

/// <summary>
/// Helper class สำหรับสร้าง Example Dialogue Trees
/// ใช้สำหรับทดสอบและเรียนรู้ระบบ
/// </summary>
public class DialogueTreeExamples
{
    /// <summary>
    /// สร้าง Simple Conversation (Text + Choice)
    /// </summary>
    public static DialogueTree CreateSimpleConversation()
    {
        var tree = ScriptableObject.CreateInstance<DialogueTree>();
        tree.treeName = "Simple Conversation";
        tree.description = "บทสนทนาธรรมดาที่มีตัวเลือก";
        tree.startNodeId = "start";

        // Node 1: Greeting
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "start",
            nodeType = NodeType.Text,
            characterName = "Village Elder",
            dialogueText = "Welcome, traveler! What brings you to our village?",
            nextNodeId = "choice1"
        });

        // Node 2: Main Choice
        var choiceNode = new DialogueNode
        {
            nodeId = "choice1",
            nodeType = NodeType.Choice,
            characterName = "Village Elder",
            dialogueText = "How can I help you today?",
            choices = new System.Collections.Generic.List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Tell me about this place", targetNodeId = "about_village" },
                new DialogueChoice { choiceText = "Any quests available?", targetNodeId = "quest_offer" },
                new DialogueChoice { choiceText = "I'm just passing through", targetNodeId = "goodbye" }
            }
        };
        tree.nodes.Add(choiceNode);

        // Node 3: About Village
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "about_village",
            nodeType = NodeType.Text,
            characterName = "Village Elder",
            dialogueText = "Our village has stood here for generations. We're a peaceful folk, but lately, strange things have been happening...",
            nextNodeId = "followup_choice"
        });

        // Node 4: Follow-up Choice
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "followup_choice",
            nodeType = NodeType.Choice,
            characterName = "Village Elder",
            dialogueText = "Would you like to know more?",
            choices = new System.Collections.Generic.List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "What strange things?", targetNodeId = "strange_things" },
                new DialogueChoice { choiceText = "I should go now", targetNodeId = "goodbye" }
            }
        });

        // Node 5: Strange Things
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "strange_things",
            nodeType = NodeType.Text,
            characterName = "Village Elder",
            dialogueText = "Monsters have been appearing near the old ruins. If you're brave enough, perhaps you could investigate?",
            nextNodeId = "goodbye"
        });

        // Node 6: Quest Offer
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "quest_offer",
            nodeType = NodeType.Text,
            characterName = "Village Elder",
            dialogueText = "Indeed! We need someone to clear out the monsters near the old ruins. Will you help us?",
            nextNodeId = "goodbye"
        });

        // Node 7: Goodbye
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "goodbye",
            nodeType = NodeType.Text,
            characterName = "Village Elder",
            dialogueText = "Safe travels, friend. May the winds guide you.",
            isEndNode = true
        });

        return tree;
    }

    /// <summary>
    /// สร้าง Shop Dialogue with Conditions
    /// </summary>
    public static DialogueTree CreateShopDialogue()
    {
        var tree = ScriptableObject.CreateInstance<DialogueTree>();
        tree.treeName = "Shop Dialogue";
        tree.description = "ร้านค้าที่เช็คว่ามีเงินพอหรือไม่";
        tree.startNodeId = "start";

        // เพิ่มตัวแปร gold
        tree.variables.Add(new DialogueVariable
        {
            variableName = "gold",
            type = VariableType.Integer,
            numericValue = 50 // เริ่มต้นมี 50 gold
        });

        // Node 1: Welcome
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "start",
            nodeType = NodeType.Text,
            characterName = "Merchant",
            dialogueText = "Welcome to my shop! Take a look at my wares.",
            nextNodeId = "shop_choice"
        });

        // Node 2: Shop Choice
        var choiceNode = new DialogueNode
        {
            nodeId = "shop_choice",
            nodeType = NodeType.Choice,
            characterName = "Merchant",
            dialogueText = "What would you like to buy?",
            choices = new System.Collections.Generic.List<DialogueChoice>
            {
                new DialogueChoice 
                { 
                    choiceText = "Health Potion (20 gold)", 
                    targetNodeId = "check_potion",
                    requirementCondition = new DialogueCondition
                    {
                        type = ConditionType.VariableCheck,
                        variableName = "gold",
                        compareOperator = CompareOperator.GreaterOrEqual,
                        compareValue = 20
                    }
                },
                new DialogueChoice 
                { 
                    choiceText = "Sword (100 gold)", 
                    targetNodeId = "check_sword",
                    requirementCondition = new DialogueCondition
                    {
                        type = ConditionType.VariableCheck,
                        variableName = "gold",
                        compareOperator = CompareOperator.GreaterOrEqual,
                        compareValue = 100
                    }
                },
                new DialogueChoice { choiceText = "Just browsing", targetNodeId = "browsing" }
            }
        };
        tree.nodes.Add(choiceNode);

        // Node 3: Check Potion
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "check_potion",
            nodeType = NodeType.Condition,
            condition = new DialogueCondition
            {
                type = ConditionType.VariableCheck,
                variableName = "gold",
                compareOperator = CompareOperator.GreaterOrEqual,
                compareValue = 20
            },
            trueNodeId = "buy_potion",
            falseNodeId = "no_money"
        });

        // Node 4: Buy Potion
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "buy_potion",
            nodeType = NodeType.Text,
            characterName = "Merchant",
            dialogueText = "Here's your health potion! Use it wisely.",
            nextNodeId = "end"
        });

        // Node 5: Check Sword
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "check_sword",
            nodeType = NodeType.Condition,
            condition = new DialogueCondition
            {
                type = ConditionType.VariableCheck,
                variableName = "gold",
                compareOperator = CompareOperator.GreaterOrEqual,
                compareValue = 100
            },
            trueNodeId = "buy_sword",
            falseNodeId = "no_money"
        });

        // Node 6: Buy Sword
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "buy_sword",
            nodeType = NodeType.Text,
            characterName = "Merchant",
            dialogueText = "An excellent choice! This sword has served many heroes well.",
            nextNodeId = "end"
        });

        // Node 7: No Money
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "no_money",
            nodeType = NodeType.Text,
            characterName = "Merchant",
            dialogueText = "Sorry, you don't have enough gold for that.",
            nextNodeId = "shop_choice"
        });

        // Node 8: Browsing
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "browsing",
            nodeType = NodeType.Text,
            characterName = "Merchant",
            dialogueText = "Take your time! Let me know if you change your mind.",
            nextNodeId = "end"
        });

        // Node 9: End
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "end",
            nodeType = NodeType.Text,
            characterName = "Merchant",
            dialogueText = "Come back anytime!",
            isEndNode = true
        });

        return tree;
    }

    /// <summary>
    /// สร้าง Cutscene Example
    /// </summary>
    public static DialogueTree CreateCutsceneExample()
    {
        var tree = ScriptableObject.CreateInstance<DialogueTree>();
        tree.treeName = "Cutscene Example";
        tree.description = "ตัวอย่างการใช้ Actions และ Cutscenes";
        tree.startNodeId = "start";

        // Node 1: Setup
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "start",
            nodeType = NodeType.Text,
            characterName = "Wizard",
            dialogueText = "Behold! Watch as I summon ancient powers!",
            nextNodeId = "cutscene"
        });

        // Node 2: Cutscene Actions
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "cutscene",
            nodeType = NodeType.Action,
            actions = new System.Collections.Generic.List<DialogueAction>
            {
                // Action 1: Play animation
                new DialogueAction
                {
                    actionType = ActionType.PlayAnimation,
                    delayBefore = 0.5f,
                    waitForCompletion = true
                },
                // Action 2: Play sound
                new DialogueAction
                {
                    actionType = ActionType.PlaySound,
                    delayBefore = 0f,
                    waitForCompletion = false
                },
                // Action 3: Spawn effect
                new DialogueAction
                {
                    actionType = ActionType.SpawnEffect,
                    delayBefore = 1f,
                    effectDuration = 2f,
                    waitForCompletion = true
                },
                // Action 4: Camera shake
                new DialogueAction
                {
                    actionType = ActionType.CameraAction,
                    cameraAction = CameraActionType.Shake,
                    cameraDuration = 0.5f,
                    waitForCompletion = true
                }
            },
            nextNodeId = "after_cutscene"
        });

        // Node 3: After Cutscene
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "after_cutscene",
            nodeType = NodeType.Text,
            characterName = "Wizard",
            dialogueText = "Impressive, wasn't it? That's just a taste of my power!",
            nextNodeId = "end"
        });

        // Node 4: End
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "end",
            nodeType = NodeType.Text,
            characterName = "Wizard",
            dialogueText = "Now, if you'll excuse me, I have spells to practice.",
            isEndNode = true
        });

        return tree;
    }

    /// <summary>
    /// สร้าง Branching Story Example
    /// </summary>
    public static DialogueTree CreateBranchingStory()
    {
        var tree = ScriptableObject.CreateInstance<DialogueTree>();
        tree.treeName = "Branching Story";
        tree.description = "เรื่องราวที่แยกเป็นหลายจบ";
        tree.startNodeId = "start";

        // เพิ่มตัวแปร karma
        tree.variables.Add(new DialogueVariable
        {
            variableName = "karma",
            type = VariableType.Integer,
            numericValue = 50
        });

        // Node 1: Intro
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "start",
            nodeType = NodeType.Text,
            characterName = "Mysterious Figure",
            dialogueText = "You stand at a crossroads. Your choices will determine your fate...",
            nextNodeId = "moral_choice"
        });

        // Node 2: Moral Choice
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "moral_choice",
            nodeType = NodeType.Choice,
            characterName = "Mysterious Figure",
            dialogueText = "A thief steals bread to feed his family. What do you do?",
            choices = new System.Collections.Generic.List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Help him escape (Good)", targetNodeId = "good_choice" },
                new DialogueChoice { choiceText = "Turn him in to guards (Evil)", targetNodeId = "evil_choice" },
                new DialogueChoice { choiceText = "Ignore the situation (Neutral)", targetNodeId = "neutral_choice" }
            }
        });

        // Node 3: Good Choice
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "good_choice",
            nodeType = NodeType.Text,
            characterName = "Mysterious Figure",
            dialogueText = "You show compassion. Your karma increases.",
            nextNodeId = "karma_check"
        });

        // Node 4: Evil Choice
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "evil_choice",
            nodeType = NodeType.Text,
            characterName = "Mysterious Figure",
            dialogueText = "You uphold the law without mercy. Your karma decreases.",
            nextNodeId = "karma_check"
        });

        // Node 5: Neutral Choice
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "neutral_choice",
            nodeType = NodeType.Text,
            characterName = "Mysterious Figure",
            dialogueText = "You remain neutral. Your karma stays the same.",
            nextNodeId = "karma_check"
        });

        // Node 6: Karma Check
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "karma_check",
            nodeType = NodeType.Condition,
            condition = new DialogueCondition
            {
                type = ConditionType.VariableCheck,
                variableName = "karma",
                compareOperator = CompareOperator.GreaterThan,
                compareValue = 50
            },
            trueNodeId = "good_ending",
            falseNodeId = "bad_ending"
        });

        // Node 7: Good Ending
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "good_ending",
            nodeType = NodeType.Text,
            characterName = "Mysterious Figure",
            dialogueText = "Your good deeds have not gone unnoticed. The path of light opens before you.",
            isEndNode = true
        });

        // Node 8: Bad Ending
        tree.nodes.Add(new DialogueNode
        {
            nodeId = "bad_ending",
            nodeType = NodeType.Text,
            characterName = "Mysterious Figure",
            dialogueText = "Your choices have consequences. The shadows embrace you.",
            isEndNode = true
        });

        return tree;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Dialogue/Create Examples/Simple Conversation")]
    static void CreateSimpleConversationAsset()
    {
        var tree = CreateSimpleConversation();
        UnityEditor.AssetDatabase.CreateAsset(tree, "Assets/Scripts/Dialogue/Example_SimpleConversation.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("✅ Created: Example_SimpleConversation.asset");
    }

    [UnityEditor.MenuItem("Dialogue/Create Examples/Shop Dialogue")]
    static void CreateShopDialogueAsset()
    {
        var tree = CreateShopDialogue();
        UnityEditor.AssetDatabase.CreateAsset(tree, "Assets/Scripts/Dialogue/Example_ShopDialogue.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("✅ Created: Example_ShopDialogue.asset");
    }

    [UnityEditor.MenuItem("Dialogue/Create Examples/Cutscene Example")]
    static void CreateCutsceneExampleAsset()
    {
        var tree = CreateCutsceneExample();
        UnityEditor.AssetDatabase.CreateAsset(tree, "Assets/Scripts/Dialogue/Example_Cutscene.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("✅ Created: Example_Cutscene.asset");
    }

    [UnityEditor.MenuItem("Dialogue/Create Examples/Branching Story")]
    static void CreateBranchingStoryAsset()
    {
        var tree = CreateBranchingStory();
        UnityEditor.AssetDatabase.CreateAsset(tree, "Assets/Scripts/Dialogue/Example_BranchingStory.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("✅ Created: Example_BranchingStory.asset");
    }

    [UnityEditor.MenuItem("Dialogue/Create Examples/All Examples")]
    static void CreateAllExamples()
    {
        CreateSimpleConversationAsset();
        CreateShopDialogueAsset();
        CreateCutsceneExampleAsset();
        CreateBranchingStoryAsset();
        Debug.Log("✅ Created all example dialogue trees!");
    }
#endif
}
