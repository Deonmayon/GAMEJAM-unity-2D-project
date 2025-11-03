# 🎯 Dialogue System - Advanced Features Guide

## ✨ คุณสมบัติใหม่ที่เพิ่มเข้ามา

### 1. ⏱️ Action Timing
กำหนดได้ว่า Action จะเล่นเมื่อไหร่:
- **OnNodeEnter** - เล่นทันทีที่เข้า node (ก่อนแสดงข้อความ)
- **OnTextComplete** - เล่นหลังจาก typewriter เสร็จ
- **OnNodeExit** - เล่นก่อนออกจาก node (กดปุ่ม next แล้ว)
- **OnDialogueEnd** - เล่นเมื่อ dialogue ทั้งหมดจบ

### 2. 🎬 Actions ในทุก Node Type
ตอนนี้ Text Node, Choice Node, Condition Node สามารถมี Actions ได้แล้ว!

### 3. 🎭 NPC Animation Controller
จัดการ animation ของ NPC อัตโนมัติ ไม่ต้องใส่ action ทุก node

### 4. ⚡ Choice Events
แต่ละตัวเลือกสามารถ trigger events, actions, และแก้ไขตัวแปรได้

### 5. 🌐 Global Event System
เชื่อมต่อกับระบบอื่นๆ เช่น Hunt, Quest, Inventory

---

## 📚 ตัวอย่างการใช้งาน

### Example 1: NPC พูดพร้อม Animation

```
[Text Node] "greeting"
├─ Character: "NPC"
├─ Text: "Hello there, traveler!"
└─ Actions:
   └─ Play Animation
      - Timing: OnNodeEnter
      - Animator: NPC_Animator
      - Trigger: "wave"
```

**ผลลัพธ์:** NPC จะโบกมือทันทีที่เริ่มพูด

---

### Example 2: Cutscene หลัง Dialogue จบ

```
[Text Node] "warning"
├─ Text: "You shouldn't have come here..."
├─ Next Node: (none)
├─ Is End Node: true
└─ Actions:
   ├─ Action 1: Play Sound
   │  - Timing: OnDialogueEnd
   │  - Sound: thunder_sound
   │
   ├─ Action 2: Spawn Effect
   │  - Timing: OnDialogueEnd
   │  - Effect: lightning_effect
   │
   └─ Action 3: Camera Shake
      - Timing: OnDialogueEnd
      - Duration: 1.0
```

**ผลลัพธ์:** หลังจาก dialogue จบ จะเล่น sound + effect + camera shake พร้อมกัน

---

### Example 3: Choice ที่ trigger Hunt ไล่ตาม

```
[Choice Node] "steal_or_not"
├─ Text: "Will you steal the treasure?"
└─ Choices:
   ├─ Choice 1: "Steal it!"
   │  ├─ Target Node: "stole_treasure"
   │  └─ On Select Event:
   │     └─ DialogueEventManager.StartHuntChase()
   │
   └─ Choice 2: "Leave it alone"
      └─ Target Node: "didnt_steal"
```

**ผลลัพธ์:** ถ้าเลือก "Steal it!" Hunt จะเริ่มไล่ตามทันที

---

### Example 4: Choice ที่แก้ไขตัวแปร

```
[Choice Node] "donation"
├─ Text: "Would you like to donate?"
└─ Choices:
   ├─ Choice 1: "Donate 100 gold"
   │  ├─ Requirement: gold >= 100
   │  ├─ Variable Modifiers:
   │  │  ├─ gold: Subtract 100
   │  │  └─ karma: Add 10
   │  └─ Target Node: "thanks"
   │
   └─ Choice 2: "No thanks"
      └─ Target Node: "goodbye"
```

**ผลลัพธ์:** ถ้าเลือก donate จะลดเงิน 100 และเพิ่ม karma 10

---

### Example 5: NPC เคลื่อนไหวหลัง Dialogue จบ

```
[Text Node] "farewell"
├─ Text: "I must go now!"
├─ Is End Node: true
└─ Actions:
   └─ Move Character
      - Timing: OnDialogueEnd
      - Move Target: NPC_Transform
      - Destination: ExitPoint
      - Speed: 3.0
      - Look At Direction: true
```

**ผลลัพธ์:** NPC จะเดินออกไปหลังจากคุยจบ

---

### Example 6: Animation ระหว่างพูด

```
[Text Node] "casting_spell"
├─ Text: "Behold my power!"
└─ Actions:
   ├─ Action 1: Play Animation
   │  - Timing: OnNodeEnter
   │  - Trigger: "start_cast"
   │
   ├─ Action 2: Spawn Effect
   │  - Timing: OnTextComplete
   │  - Effect: magic_circle
   │
   └─ Action 3: Play Sound
      - Timing: OnTextComplete
      - Sound: magic_whoosh
```

**ผลลัพธ์:**
1. เริ่ม casting animation ทันที
2. พอข้อความแสดงเสร็จ → เล่น effect + sound

---

## 🎭 NPC Animation Controller - Setup

### 1. เพิ่ม Component ให้ NPC

```
1. เลือก NPC GameObject
2. Add Component → DialogueNpcAnimator
3. ตั้งค่า:
   - Talking Parameter: "isTalking"
   - Idle Parameter: "isIdle"
   - Walking Parameter: "isWalking"
   - Auto Play Talk Animation: ✓
   - Auto Look At Player: ✓
```

### 2. ตั้งค่า Animator Controller

สร้าง parameters ใน Animator:
- `isTalking` (Bool)
- `isIdle` (Bool)
- `isWalking` (Bool)

### 3. เชื่อมกับ Dialogue

ใน DialogueTree → ลาก NPC ที่มี DialogueNpcAnimator เข้า Inspector
ระบบจะจัดการ animation อัตโนมัติ!

---

## 🌐 Global Event System - Setup

### 1. สร้าง Event Manager

```
1. สร้าง GameObject ชื่อ "DialogueEventManager"
2. Add Component → DialogueEventManager
```

### 2. ลงทะเบียน Events ใน Inspector

```
Registered Events:
├─ Event 1:
│  ├─ Event Name: "StartHunt"
│  └─ On Trigger:
│     └─ DialogueEventManager.StartHuntChase()
│
├─ Event 2:
│  ├─ Event Name: "GiveReward"
│  └─ On Trigger:
│     └─ InventoryManager.AddGold(100)
│
└─ Event 3:
   ├─ Event Name: "LoadNextLevel"
   └─ On Trigger:
      └─ SceneManager.LoadScene("Level2")
```

### 3. ใช้งานใน Dialogue Choice

```
[Choice Node]
└─ Choice: "Accept the challenge"
   └─ On Select Event:
      └─ DialogueEventManager.TriggerEvent("StartHunt")
```

---

## 🔧 ตัวอย่าง Built-in Events

DialogueEventManager มี methods พร้อมใช้:

```csharp
// Hunt System
DialogueEventManager.Instance.StartHuntChase();
DialogueEventManager.Instance.StopHuntChase();

// Quest System
DialogueEventManager.Instance.StartQuest("quest_id");

// Inventory
DialogueEventManager.Instance.GiveItem("sword");
DialogueEventManager.Instance.RemoveItem("key");

// Scene
DialogueEventManager.Instance.LoadScene("BossRoom");

// Game State
DialogueEventManager.Instance.SetGameState("boss_defeated", true);
```

---

## 💡 Best Practices

### 1. Action Timing

**❌ ไม่ดี:**
```
ใส่ action ทุกอย่างเป็น OnNodeEnter
→ เล่นพร้อมกันหมด สับสน
```

**✅ ดี:**
```
OnNodeEnter: Setup (camera focus, turn to player)
OnTextComplete: Main action (animation, effects)
OnNodeExit: Cleanup (reset camera)
OnDialogueEnd: Final cutscene (NPC leaves)
```

### 2. NPC Animation

**❌ ไม่ดี:**
```
ใส่ animation action ทุก node
→ ซ้ำซ้อน ยุ่งยาก
```

**✅ ดี:**
```
ใช้ DialogueNpcAnimator
→ จัดการอัตโนมัติ แก้ไขง่าย
```

### 3. Choice Events

**❌ ไม่ดี:**
```
ใส่ logic ใน Dialogue
→ ผูกแน่น แก้ยาก
```

**✅ ดี:**
```
ใช้ DialogueEventManager
→ แยก logic ออกมา reusable
```

---

## 🎯 Use Cases

### Use Case 1: Boss Fight Intro

```
[Text 1] "You dare challenge me?"
  → OnNodeEnter: Camera focus on boss
  → OnTextComplete: Boss roar animation

[Text 2] "Prepare to die!"
  → OnNodeExit: Camera shake
  → OnDialogueEnd: Start boss music + Enable boss AI
```

### Use Case 2: Shop with Karma Check

```
[Choice] "What would you like?"
  ├─ "Buy sword (100g)" 
  │  → Requirement: gold >= 100
  │  → On Select: gold -100, karma +5
  │
  └─ "Steal sword"
     → On Select: karma -50, trigger "GuardChase" event
```

### Use Case 3: Quest Chain

```
[Text] "Help me find my cat!"
  → OnDialogueEnd: StartQuest("find_cat")

... (player finds cat) ...

[Text] "You found him! Thank you!"
  → On Select: CompleteQuest("find_cat")
  → Variable: karma +10
  → Event: GiveItem("cat_charm")
```

---

## 📋 Checklist

### ถ้าต้องการ Action Timing:
- [ ] เลือก Action Timing ที่เหมาะสม
- [ ] ตั้ง Wait For Completion ถ้าต้องรอ
- [ ] ทดสอบว่า timing ถูกต้อง

### ถ้าต้องการ NPC Animation:
- [ ] เพิ่ม DialogueNpcAnimator ให้ NPC
- [ ] ตั้งค่า Animator parameters
- [ ] ทดสอบ animation transitions

### ถ้าต้องการ Choice Events:
- [ ] สร้าง DialogueEventManager ใน Scene
- [ ] ลงทะเบียน events ที่ต้องใช้
- [ ] ผูก events กับ Choice
- [ ] ทดสอบว่า event ทำงาน

### ถ้าต้องการเชื่อมกับระบบอื่น:
- [ ] สร้าง method ใน DialogueEventManager
- [ ] เชื่อมกับ HuntAI / QuestManager / etc.
- [ ] ลงทะเบียนใน Registered Events
- [ ] ทดสอบการเชื่อมต่อ

---

## 🐛 Troubleshooting

### Actions ไม่เล่น
✅ เช็คว่า Action Timing ถูกต้อง
✅ เช็คว่ามี reference ครบ (Animator, Sound, etc.)
✅ ดู Console มี error หรือไม่

### NPC Animation ไม่เปลี่ยน
✅ เช็คว่า parameter name ตรงกัน
✅ เช็คว่ามี Animator Controller
✅ เช็คว่า DialogueNpcAnimator enabled

### Choice Event ไม่ทำงาน
✅ เช็คว่ามี DialogueEventManager ใน Scene
✅ เช็คว่า event name ถูกต้อง
✅ เช็คว่า event ลงทะเบียนแล้ว

### Hunt ไม่ไล่ตาม
✅ เช็คว่ามี HuntAI script
✅ เช็คว่า method name ถูกต้อง (StartChasing)
✅ ใช้ Debug.Log เช็คว่า event ถูกเรียก

---

## 📞 Summary

ระบบใหม่ตอบโจทย์ทั้งหมด:

✅ **Action Timing** - กำหนดได้ว่าจะเล่นเมื่อไหร่
✅ **Actions ในทุก Node** - ไม่จำกัดแค่ Action Node
✅ **NPC Animation** - จัดการอัตโนมัติ
✅ **Choice Events** - trigger ระบบอื่นได้ (Hunt, Quest, etc.)
✅ **Global Events** - เชื่อมต่อกับทุกระบบ

**ใช้งานง่าย ยืดหยุ่น ครอบคลุม!** 🎉
