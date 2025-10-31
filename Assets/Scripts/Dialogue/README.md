# 📖 Dialogue System - คู่มือการใช้งาน

## 🎯 ภาพรวมระบบ

ระบบ Dialogue นี้รองรับการสนทนาแบบมีขั้นตอน พร้อมฟีเจอร์:
- ✅ Player เดินไปหา NPC อัตโนมัติเมื่อเข้า Trigger
- ✅ ล็อกการเคลื่อนไหวของ Player ขณะคุย
- ✅ NPC เดินไปยังจุดหมายและหายไปหลัง Dialogue จบ
- ✅ Fade out effect เมื่อ NPC หายไป

---

## 📁 ไฟล์ที่เกี่ยวข้อง

| ไฟล์ | หน้าที่ |
|------|---------|
| `DialogueData.cs` | ScriptableObject เก็บข้อมูลบทสนทนา |
| `DialogueLine.cs` | ข้อมูลของแต่ละบรรทัดสนทนา |
| `DialogueTrigger.cs` | ตรวจจับ Player และเริ่มบทสนทนา |
| `DialogueUI.cs` | แสดงผล UI และจัดการ Dialogue |
| `NpcMovementData.cs` | ข้อมูลการเคลื่อนที่ของ NPC |
| `NpcController.cs` | ควบคุมการเดินและหายตัวของ NPC |

---

## 🚀 วิธีการตั้งค่า (Setup)

### 1️⃣ สร้าง Dialogue Data (ScriptableObject)

1. คลิกขวาใน Project → `Create` → `Dialogue` → `DialogueData`
2. ตั้งชื่อ เช่น `UncleChaiDialogue`
3. กรอกข้อมูลบทสนทนา:

```
Lines:
  [0] Character Name: "ลุงชัย"
      Portrait: (ใส่ Sprite รูปลุงชัย)
      Line Text: "สวัสดีครับ มีอะไรให้ช่วยไหม?"
      Button Text: "ต่อไป"
  
  [1] Character Name: "ลุงชัย"
      Portrait: (ใส่ Sprite รูปลุงชัย)
      Line Text: "ขอบคุณที่มาคุยด้วยนะ ฉันต้องไปแล้ว"
      Button Text: "จบ"
```

4. **ตั้งค่า NPC Movement** (ถ้าต้องการให้ NPC เดินไปหายตัวหลังคุยจบ):

```
NPC Movement:
  ✓ Npc Transform: (ลาก GameObject ของลุงชัย)
  ✓ Destination Transform: (ลาก Empty GameObject ที่เป็นจุดหมาย)
  Move Speed: 3
  Arrival Distance: 0.1
  ✓ Disappear On Arrival: true
  Disappear Delay: 0.5
```

---

### 2️⃣ สร้าง Trigger Zone

1. สร้าง Empty GameObject ใหม่ → ตั้งชื่อ `DialogueTrigger_UncleChat`
2. เพิ่ม Component:
   - `Box Collider 2D` (หรือ Collider อื่นๆ)
   - ✅ เปิด `Is Trigger`
   - เพิ่ม Component `DialogueTrigger`

3. ตั้งค่า DialogueTrigger:

```
Refs:
  Dialogue UI: (ลาก GameObject ที่มี DialogueUI)
  Dialogue Data: (ลาก ScriptableObject ที่สร้างไว้)

Who Can Trigger:
  Player Tag: "Player"

Behavior:
  ✓ Require Exit Before Retrigger: true

Player Auto-Walk to NPC:
  ✓ Auto Walk To Npc: true  ← เปิดถ้าต้องการให้ Player เดินไปหา NPC
  Npc Position: (ลาก GameObject ของลุงชัย)
  Walk Speed: 3
  Stop Distance: 1
```

---

### 3️⃣ สร้าง Dialogue UI

1. สร้าง Canvas UI (ถ้ายังไม่มี)
2. สร้าง Panel สำหรับ Dialogue พร้อม:
   - `Image` - รูปภาพตัวละคร (Portrait)
   - `TextMeshPro` - ชื่อตัวละคร (Name)
   - `TextMeshPro` - ข้อความสนทนา (Line Text)
   - `Button` - ปุ่ม Next/ปิด
   - `TextMeshPro` - ข้อความในปุ่ม (Button Label)

3. เพิ่ม Component `DialogueUI` ใน Canvas หรือ Panel
4. ลากเชื่อม UI Elements:

```
UI References:
  Panel: (Panel Dialogue)
  Portrait Image: (Image component)
  Name Text: (TextMeshPro ชื่อ)
  Line Text: (TextMeshPro บทสนทนา)
  Next Button: (Button)
  Button Label: (TextMeshPro ในปุ่ม)

Data:
  Dialogue: (เว้นว่างไว้ได้ จะถูกตั้งค่าจาก Trigger)

Lock Player While Talking:
  Movement Scripts To Disable: (ลาก Script ที่ควบคุม Player เช่น PlayerMovement)
  Player Rb: (ลาก Rigidbody2D ของ Player)
  ✓ Hard Freeze Position: true
```

---

### 4️⃣ สร้างจุดหมายสำหรับ NPC (Destination)

1. สร้าง Empty GameObject ใหม่
2. ตั้งชื่อ `UncleChaiDestination`
3. วางที่ตำแหน่งที่ต้องการให้ NPC เดินไปหายไป
4. (Optional) ใส่ Gizmo Icon เพื่อดูง่ายใน Scene View

---

## 🎮 Flow การทำงาน

```
┌─────────────────────────────────────┐
│ 1. Player เข้า Trigger Zone        │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 2. (Optional) Player เดินไปหา NPC  │
│    อัตโนมัติ                        │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 3. Player ถูกล็อก (ไม่สามารถ       │
│    เคลื่อนที่ได้)                   │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 4. แสดง Dialogue UI                │
│    - แสดงรูป Portrait               │
│    - แสดงชื่อตัวละคร                │
│    - แสดงข้อความบรรทัดแรก          │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 5. Player กดปุ่ม "Next"            │
│    → แสดงบรรทัดถัดไป               │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 6. บรรทัดสุดท้าย → Dialogue จบ    │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 7. Player ถูกปลดล็อก (เคลื่อนที่   │
│    ได้อีกครั้ง)                     │
└─────────────┬───────────────────────┘
              │
              ↓
┌─────────────────────────────────────┐
│ 8. (Optional) NPC เดินไปยังจุดหมาย │
│    - เดินด้วยความเร็วที่กำหนด       │
│    - ถึงแล้ว → Fade out            │
│    - SetActive(false) หายไป        │
└─────────────────────────────────────┘
```

---

## ⚙️ ตัวเลือกการตั้งค่า (Options)

### DialogueTrigger

| ตัวเลือก | คำอธิบาย |
|---------|----------|
| `requireExitBeforeRetrigger` | ถ้า `true` = ต้องออกจาก Trigger Zone ก่อนถึงจะเริ่มบทสนทนาใหม่ได้ |
| `autoWalkToNpc` | ถ้า `true` = Player จะเดินไปหา NPC อัตโนมัติ |
| `npcPosition` | ตำแหน่งของ NPC ที่ Player จะเดินไป |
| `walkSpeed` | ความเร็วที่ Player เดิน |
| `stopDistance` | ระยะห่างที่หยุดเดิน (หน่วย Unity Unit) |

### DialogueUI

| ตัวเลือก | คำอธิบาย |
|---------|----------|
| `movementScriptsToDisable` | Script ที่จะถูกปิดขณะคุย (เช่น PlayerMovement) |
| `playerRb` | Rigidbody2D ของ Player เพื่อหยุดความเร็ว |
| `hardFreezePosition` | ถ้า `true` = Freeze ตำแหน่ง Player ทั้งหมด |

### NpcMovementData

| ตัวเลือก | คำอธิบาย |
|---------|----------|
| `npcTransform` | ตัว NPC ที่จะเคลื่อนที่ |
| `destinationTransform` | จุดหมายที่ NPC จะเดินไป |
| `moveSpeed` | ความเร็วการเดิน (Unity Units/วินาที) |
| `arrivalDistance` | ระยะที่ถือว่า "ถึงแล้ว" |
| `disappearOnArrival` | ถ้า `true` = NPC จะหายไปเมื่อถึงจุดหมาย |
| `disappearDelay` | เวลารอก่อนหายไป (วินาที) |

---

## 🔧 การแก้ไขและปรับแต่ง

### เพิ่ม Animation ให้ NPC

แก้ไขใน `NpcController.cs`:

```csharp
private IEnumerator MoveToDestination(NpcMovementData data)
{
    // เพิ่ม Animator
    Animator animator = GetComponent<Animator>();
    if (animator != null)
    {
        animator.SetBool("IsWalking", true);
    }

    // ... โค้ดเดินไปยังจุดหมาย ...

    // หยุดเดิน
    if (animator != null)
    {
        animator.SetBool("IsWalking", false);
    }
}
```

### เพิ่มเสียงพูด

แก้ไขใน `DialogueUI.cs` → `ShowLine()`:

```csharp
void ShowLine()
{
    // ... โค้ดเดิม ...

    // เล่นเสียงพูด
    AudioSource audioSource = GetComponent<AudioSource>();
    if (audioSource != null && current.voiceClip != null)
    {
        audioSource.PlayOneShot(current.voiceClip);
    }
}
```

### ทำให้ Player เดินแบบมี Animation

แก้ไขใน `DialogueTrigger.cs` → `WalkToNpcThenDialogue()`:

```csharp
// เพิ่ม Animator
Animator animator = player.GetComponent<Animator>();
if (animator != null)
{
    animator.SetBool("IsWalking", true);
}

// เดินไปหา NPC
while (...)
{
    // ... โค้ดเดินไปหา NPC ...
}

// หยุดเดิน
if (animator != null)
{
    animator.SetBool("IsWalking", false);
}
```

---

## 🐛 การแก้ปัญหา (Troubleshooting)

### ❌ Player ไม่เดินไปหา NPC

**ตรวจสอบ:**
- ✅ `autoWalkToNpc` = true
- ✅ `npcPosition` ถูกลากเข้าไปแล้ว
- ✅ Player มี Tag = "Player"

### ❌ NPC ไม่เดินไปยังจุดหมาย

**ตรวจสอบ:**
- ✅ DialogueData → NPC Movement → มีการตั้งค่า
- ✅ `npcTransform` และ `destinationTransform` ถูกลากเข้าไปแล้ว
- ✅ NPC ไม่มี Rigidbody2D แบบ Kinematic หรือ Static

### ❌ Player ยังเคลื่อนที่ได้ขณะคุย

**ตรวจสอบ:**
- ✅ DialogueUI → `movementScriptsToDisable` ลาก Script ที่ควบคุม Player แล้ว
- ✅ `playerRb` ลาก Rigidbody2D ของ Player แล้ว
- ✅ `hardFreezePosition` = true

### ❌ UI ไม่ขึ้น

**ตรวจสอบ:**
- ✅ DialogueUI → Panel ถูกลากเข้าไปแล้ว
- ✅ Canvas → Render Mode = Screen Space - Overlay
- ✅ Canvas → Order in Layer สูงพอ

---

## 📚 ตัวอย่างการใช้งาน

### ตัวอย่าง 1: Dialogue ธรรมดา (ไม่มีการเดิน)

```
DialogueTrigger:
  ✓ Dialogue UI: [DialogueCanvas]
  ✓ Dialogue Data: [SimpleDialogue]
  ✗ Auto Walk To Npc: false  ← ปิดไว้

DialogueData:
  Lines: [3 บรรทัด]
  NPC Movement: (ไม่ตั้งค่า)  ← ไม่มีการเดิน
```

### ตัวอย่าง 2: Player เดินไปหา NPC + NPC หายไป

```
DialogueTrigger:
  ✓ Dialogue UI: [DialogueCanvas]
  ✓ Dialogue Data: [UncleChaiDialogue]
  ✓ Auto Walk To Npc: true   ← เปิด
  ✓ Npc Position: [UncleChaiGameObject]
  Walk Speed: 3
  Stop Distance: 1

DialogueData:
  Lines: [2 บรรทัด]
  NPC Movement:
    ✓ Npc Transform: [UncleChaiGameObject]
    ✓ Destination Transform: [UncleChaiDestination]
    ✓ Disappear On Arrival: true
```

### ตัวอย่าง 3: NPC เดินไป แต่ไม่หายไป

```
DialogueData:
  NPC Movement:
    ✓ Npc Transform: [GuardGameObject]
    ✓ Destination Transform: [GatePosition]
    ✗ Disappear On Arrival: false  ← ปิดการหายตัว
```

---

## 📝 Notes

- **Tag ของ Player**: ต้องเป็น "Player" (ปรับได้ใน DialogueTrigger)
- **Collider 2D**: Trigger Zone ต้องมี Collider 2D แบบ Is Trigger = true
- **Layer**: ตรวจสอบ Layer Collision Matrix ให้ Player ชนกับ Trigger ได้
- **Performance**: ถ้ามี NPC เยอะ อาจพิจารณาใช้ Object Pooling แทน SetActive(false)

---

## 🎨 Tips & Best Practices

1. **ใช้ Empty GameObject เป็นจุดหมาย** - ง่ายต่อการย้ายตำแหน่ง
2. **ตั้งชื่อให้ชัดเจน** - เช่น `Trigger_TalkToUncle`, `Destination_UncleExit`
3. **ใช้ Gizmo** - วาดเส้นเชื่อมระหว่าง Trigger → NPC → Destination
4. **ทดสอบบ่อยๆ** - เล่นดูทุกครั้งที่เปลี่ยนค่า
5. **Backup ScriptableObject** - ก่อนแก้ไข Dialogue Data

---

## 📞 Support

หากมีปัญหาหรือข้อสงสัย:
1. ตรวจสอบ Console Log (มี Debug.Log ใน Code)
2. ตรวจสอบการตั้งค่าตาม Checklist ด้านบน
3. ลองปิดเปิด Inspector เพื่อ Refresh
4. ลอง Reimport Scripts ใหม่

---

**สร้างโดย**: GitHub Copilot  
**วันที่**: October 31, 2025  
**Version**: 1.0
