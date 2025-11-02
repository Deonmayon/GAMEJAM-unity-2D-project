# 🎭 Dialogue Tree System - คู่มือการใช้งาน

ระบบ Dialogue แบบใหม่ที่ครอบคลุมและยืดหยุ่น รองรับ:
- ✅ **Dialogue Tree** - แยกสาขาได้ (branching)
- ✅ **Choices** - ให้ผู้เล่นเลือกคำตอบ
- ✅ **Conditions** - เช็คเงื่อนไขแล้วไปเส้นทางต่างกัน
- ✅ **Actions/Cutscenes** - เล่น animation, sound, camera, effects
- ✅ **Variables** - เก็บ state และข้อมูลต่างๆ

---

## 📦 ไฟล์ที่สร้าง

### Core System
- `DialogueNode.cs` - โครงสร้าง Node แต่ละประเภท
- `DialogueTree.cs` - ScriptableObject สำหรับสร้าง Dialogue Tree
- `DialogueAction.cs` - ระบบ Action/Cutscene
- `DialogueTreeManager.cs` - จัดการ Dialogue ทั้งหมด
- `DialogueTreeUI.cs` - UI สำหรับแสดงผล
- `DialogueTreeTrigger.cs` - Trigger เพื่อเริ่ม Dialogue

### ระบบเดิม (ยังใช้งานได้)
- `DialogueData.cs`, `DialogueLine.cs` - ระบบเก่า (backward compatible)

---

## 🚀 Quick Start

### 1. สร้าง Dialogue Tree

1. คลิกขวาใน Project → `Create > Dialogue > DialogueTree`
2. ตั้งชื่อ เช่น `MyFirstDialogue`
3. เปิดไฟล์ใน Inspector
4. กด **Context Menu (⋮)** → `Create Example Tree` เพื่อดูตัวอย่าง

### 2. ตั้งค่า Scene

1. สร้าง GameObject ใหม่ชื่อ `DialogueTreeManager`
2. Add Component → `DialogueTreeManager`
3. ลาก UI ที่จะใช้แสดง Dialogue เข้า `Dialogue UI` slot
4. ตั้งค่า Player Movement Scripts และ Rigidbody2D

### 3. สร้าง Trigger

1. สร้าง GameObject พร้อม `Collider2D` (ตั้งเป็น Trigger)
2. Add Component → `DialogueTreeTrigger`
3. ลาก `DialogueTreeManager` และ `DialogueTree` เข้า Inspector
4. ตั้งค่าพฤติกรรม (Trigger Once, Require Input, etc.)

---

## 📝 Node Types

### 1. Text Node (ข้อความธรรมดา)
แสดงข้อความ แล้วให้ผู้เล่นกด Next

```
Node Settings:
- Node Type: Text
- Character Name: "NPC"
- Dialogue Text: "Hello there!"
- Next Node ID: "node2"
```

### 2. Choice Node (ให้เลือกคำตอบ)
แสดงตัวเลือกหลายอัน ผู้เล่นเลือกได้

```
Node Settings:
- Node Type: Choice
- Character Name: "NPC"
- Dialogue Text: "What would you like?"
- Choices:
  - Choice 1: "Tell me more" → "info_node"
  - Choice 2: "Goodbye" → "end_node"
```

#### ตัวเลือกที่มีเงื่อนไข
สามารถซ่อนตัวเลือกถ้าไม่ตรงเงื่อนไข:

```
Choice Settings:
- Choice Text: "Buy sword (100 gold)"
- Target Node ID: "buy_sword"
- Requirement Condition:
  - Type: Variable Check
  - Variable Name: "gold"
  - Compare Operator: Greater Or Equal
  - Compare Value: 100
```

### 3. Condition Node (เช็คเงื่อนไข)
ตรวจสอบเงื่อนไข แล้วไปทางใดทางหนึ่ง

```
Node Settings:
- Node Type: Condition
- Condition:
  - Type: Variable Check
  - Variable Name: "karma"
  - Compare Operator: Greater Than
  - Compare Value: 50
- True Node ID: "good_ending"
- False Node ID: "bad_ending"
```

#### ประเภทเงื่อนไข:
- **Variable Check** - เช็คค่าตัวแปร
- **Has Item** - มีไอเทมหรือไม่ (ต้องเชื่อม Inventory System)
- **Quest Status** - สถานะเควส (ต้องเชื่อม Quest System)
- **Custom** - เรียก method ที่กำหนดเอง

### 4. Action Node (เล่น Cutscene)
ประมวลผล Actions ต่างๆ

```
Node Settings:
- Node Type: Action
- Actions:
  - Action 1: Move NPC
  - Action 2: Play Sound
  - Action 3: Camera Focus
- Wait For Actions Complete: true
- Next Node ID: "after_cutscene"
```

---

## 🎬 Action Types

### Animation
เล่น animation ของตัวละคร

```
Action Settings:
- Action Type: Play Animation
- Target Animator: [NPC Animator]
- Animation Trigger: "wave"
- Wait For Completion: true
```

### Sound
เล่นเสียง

```
Action Settings:
- Action Type: Play Sound
- Sound Clip: [AudioClip]
- Volume: 1.0
```

### Move Character
เคลื่อนย้ายตัวละคร

```
Action Settings:
- Action Type: Move Character
- Move Target: [NPC Transform]
- Move Destination: [Target Point]
- Move Speed: 3.0
- Look At Direction: true
```

### Camera Action
ควบคุมกล้อง

```
Action Settings:
- Action Type: Camera Action
- Camera Action Type: Focus On Target
- Camera Target: [Transform]
- Camera Duration: 2.0
```

### Spawn Effect
สร้าง particle effect

```
Action Settings:
- Action Type: Spawn Effect
- Effect Prefab: [GameObject]
- Effect Spawn Point: [Transform]
- Destroy On Complete: true
- Effect Duration: 2.0
```

### Set Object Active
เปิด/ปิด GameObject

```
Action Settings:
- Action Type: Set Object Active
- Target Object: [GameObject]
- Set Active: false
```

### Custom Event
เรียก UnityEvent หรือ Method

```
Action Settings:
- Action Type: Custom Event
- Custom Event: [UnityEvent]
- Custom Event Target: [GameObject]
- Custom Method Name: "MyMethod"
```

---

## 🔧 Variables

ใช้เก็บ state ต่างๆ เช่น karma, friendship level, money

### สร้าง Variable
1. เปิด DialogueTree
2. ใน `Variables` list กด `+`
3. ตั้งชื่อ เช่น `karma`
4. เลือก Type (Integer, Float, String, Bool)
5. ตั้งค่าเริ่มต้น

### ใช้งานใน Code
```csharp
// Get
float karma = dialogueManager.GetVariable("karma");

// Set
dialogueManager.SetVariable("karma", karma + 10);
```

---

## 📋 ตัวอย่างการใช้งาน

### Example 1: Dialogue ง่ายๆ

```
[Start Node] "start"
  → Text: "Hello! How are you?"
  → Next: "choice1"

[Choice Node] "choice1"
  → Text: "How do you feel?"
  → Choices:
    - "I'm good!" → "good_response"
    - "Not great" → "bad_response"
    
[Text Node] "good_response"
  → Text: "That's great to hear!"
  → isEndNode: true
  
[Text Node] "bad_response"
  → Text: "I hope things get better..."
  → isEndNode: true
```

### Example 2: Dialogue พร้อม Cutscene

```
[Start Node] "start"
  → Text: "Watch this!"
  → Next: "action_demo"

[Action Node] "action_demo"
  → Actions:
    1. Play Animation (NPC waves)
    2. Play Sound (whoosh)
    3. Spawn Effect (sparkles)
    4. Camera Focus (on NPC)
  → Next: "end"

[Text Node] "end"
  → Text: "Pretty cool, right?"
  → isEndNode: true
```

### Example 3: Dialogue ที่มีเงื่อนไข

```
[Start Node] "start"
  → Text: "You want to enter?"
  → Next: "check_gold"

[Condition Node] "check_gold"
  → Condition: gold >= 100
  → True: "enter"
  → False: "no_money"

[Text Node] "enter"
  → Text: "Welcome! Come on in."
  → isEndNode: true

[Text Node] "no_money"
  → Text: "Sorry, you need 100 gold."
  → isEndNode: true
```

---

## 🎨 ตั้งค่า UI

### สร้าง UI Canvas

1. สร้าง Canvas ใน Scene
2. สร้าง Panel สำหรับ Dialogue
3. เพิ่ม:
   - Image สำหรับ Portrait
   - TextMeshPro สำหรับ Character Name
   - TextMeshPro สำหรับ Dialogue Text
   - Button สำหรับ Next
4. สร้าง Panel สำหรับ Choices พร้อม Vertical Layout Group
5. สร้าง Button Prefab สำหรับตัวเลือก

### ตั้งค่า DialogueTreeUI Component

1. Add Component → `DialogueTreeUI` บน Canvas
2. ลาก UI elements ทั้งหมดเข้า slots
3. ตั้งค่า Typewriter (ถ้าต้องการ)

---

## 🔍 Debugging

### Validate Tree
คลิกขวาบน DialogueTree → `Validate Tree` เพื่อเช็ค errors

### Common Errors
- **"Start node not found"** - ตั้ง Start Node ID ผิด
- **"Duplicate node ID"** - มี node ที่ใช้ ID ซ้ำกัน
- **"References non-existent node"** - Next Node ID ชี้ไปยัง node ที่ไม่มี

### Debug Tips
- ใช้ `editorNote` ใน Node เพื่อบันทึกข้อความช่วยจำ
- ใช้ Gizmos ใน Scene view เพื่อดู trigger zones

---

## ⚙️ Migration จากระบบเก่า

ถ้าใช้ `DialogueData` และ `DialogueLine` อยู่แล้ว:

### วิธีที่ 1: ใช้ทั้งสองระบบคู่กัน
ระบบเก่ายังใช้งานได้ปกติ ไม่จำเป็นต้องเปลี่ยน

### วิธีที่ 2: แปลงเป็น DialogueTree
1. สร้าง DialogueTree ใหม่
2. แปลง DialogueLine แต่ละบรรทัดเป็น Text Node
3. เชื่อม nodes ด้วย Next Node ID
4. เปลี่ยน Trigger ใช้ `DialogueTreeTrigger` แทน

---

## 📚 API Reference

### DialogueTreeManager

```csharp
// เริ่ม dialogue
void StartDialogue(DialogueTree tree)

// เช็คสถานะ
bool IsDialogueActive()
DialogueNode GetCurrentNode()
DialogueTree GetCurrentTree()

// จัดการ variables
void SetVariable(string name, float value)
float GetVariable(string name)
```

### DialogueTreeUI

```csharp
// แสดง nodes
void ShowTextNode(DialogueNode node)
void ShowChoiceNode(DialogueNode node, List<DialogueChoice> choices)

// ซ่อน UI
void HideUI()

// เช็คสถานะ
bool IsWaitingForInput()
bool HasSelectedChoice()
string GetSelectedChoiceTarget()
```

---

## 💡 Tips & Best Practices

1. **ใช้ Node ID ที่มีความหมาย** - เช่น `start`, `choice_help`, `ending_good`
2. **ใส่ Editor Note** - บันทึกว่า node นี้ทำอะไร
3. **Validate บ่อยๆ** - เช็คว่า tree ไม่มี errors
4. **ใช้ Variables** - เก็บ state แทนการ hard-code
5. **แยก Tree ตาม Scene** - อย่าใส่ทุก dialogue ในไฟล์เดียว
6. **Test แต่ละ Branch** - ลองเล่นทุกเส้นทางเพื่อเช็ค bugs

---

## 🐛 Known Issues

- **Typewriter Effect** - อาจช้าถ้าข้อความยาวมาก (แก้ด้วยการลด typewriterSpeed)
- **Camera Shake** - อาจกระตุกถ้า magnitude สูงเกินไป
- **Choice Buttons** - ต้องมี prefab ที่ถูกต้อง ไม่งั้นจะ spawn ไม่ได้

---

## 📞 Support

หากมีปัญหา:
1. เช็ค Console ดู error messages
2. ใช้ Validate Tree เพื่อหา errors
3. ดู Examples ในโฟลเดอร์ `Assets/Dialogue/Examples/`

---

Made with ❤️ for GameJam Unity 2D Project
