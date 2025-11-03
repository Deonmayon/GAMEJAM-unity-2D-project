using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Global Event System สำหรับเชื่อมต่อ Dialogue กับระบบอื่นๆ
/// เช่น Hunt System, Quest System, Inventory, etc.
/// 
/// วิธีใช้:
/// 1. สร้าง GameObject ชื่อ "DialogueEventManager" ใน Scene
/// 2. Add Component นี้
/// 3. ลงทะเบียน events ใน Inspector หรือใน Code
/// 4. ใน Dialogue Choice → On Select Event → เลือก event ที่ต้องการ
/// </summary>
public class DialogueEventManager : MonoBehaviour
{
    public static DialogueEventManager Instance { get; private set; }

    [Header("🎯 Registered Events")]
    [Tooltip("Events ที่สามารถเรียกได้จาก Dialogue")]
    public List<DialogueEventEntry> registeredEvents = new List<DialogueEventEntry>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// เรียก event ตามชื่อ
    /// </summary>
    public void TriggerEvent(string eventName)
    {
        var entry = registeredEvents.Find(e => e.eventName == eventName);
        if (entry != null)
        {
            Debug.Log($"🎯 Triggering dialogue event: {eventName}");
            entry.onTrigger?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Event '{eventName}' not found in DialogueEventManager");
        }
    }

    /// <summary>
    /// เรียก event พร้อมส่ง parameter (string)
    /// </summary>
    public void TriggerEventWithString(string eventName, string parameter)
    {
        var entry = registeredEvents.Find(e => e.eventName == eventName);
        if (entry != null)
        {
            Debug.Log($"🎯 Triggering dialogue event: {eventName} with param: {parameter}");
            entry.onTriggerWithString?.Invoke(parameter);
        }
        else
        {
            Debug.LogWarning($"Event '{eventName}' not found");
        }
    }

    /// <summary>
    /// เรียก event พร้อมส่ง parameter (int)
    /// </summary>
    public void TriggerEventWithInt(string eventName, int parameter)
    {
        var entry = registeredEvents.Find(e => e.eventName == eventName);
        if (entry != null)
        {
            Debug.Log($"🎯 Triggering dialogue event: {eventName} with param: {parameter}");
            entry.onTriggerWithInt?.Invoke(parameter);
        }
        else
        {
            Debug.LogWarning($"Event '{eventName}' not found");
        }
    }

    /// <summary>
    /// ลงทะเบียน event ใหม่แบบ dynamic (ใน code)
    /// </summary>
    public void RegisterEvent(string eventName, UnityAction action)
    {
        var existing = registeredEvents.Find(e => e.eventName == eventName);
        if (existing != null)
        {
            existing.onTrigger.AddListener(action);
        }
        else
        {
            var newEntry = new DialogueEventEntry
            {
                eventName = eventName,
                onTrigger = new UnityEvent()
            };
            newEntry.onTrigger.AddListener(action);
            registeredEvents.Add(newEntry);
        }
    }

    /// <summary>
    /// ยกเลิกการลงทะเบียน event
    /// </summary>
    public void UnregisterEvent(string eventName, UnityAction action)
    {
        var entry = registeredEvents.Find(e => e.eventName == eventName);
        if (entry != null)
        {
            entry.onTrigger.RemoveListener(action);
        }
    }

    // ==================== ตัวอย่าง Built-in Events ====================
    
    /// <summary>
    /// ตัวอย่าง: เริ่มให้ Hunt ไล่ตาม
    /// </summary>
    public void StartHuntChase()
    {
        Debug.Log("🏃 Hunt is now chasing the player!");
        // TODO: เรียก HuntAI.StartChasing();
        var huntAI = FindFirstObjectByType<MonoBehaviour>(); // แทนด้วย HuntAI class จริง
        if (huntAI != null)
        {
            huntAI.SendMessage("StartChasing", SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// ตัวอย่าง: หยุด Hunt
    /// </summary>
    public void StopHuntChase()
    {
        Debug.Log("🛑 Hunt stopped chasing!");
        var huntAI = FindFirstObjectByType<MonoBehaviour>();
        if (huntAI != null)
        {
            huntAI.SendMessage("StopChasing", SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// ตัวอย่าง: เปิดใช้งาน Quest
    /// </summary>
    public void StartQuest(string questId)
    {
        Debug.Log($"📜 Starting quest: {questId}");
        // TODO: เรียก QuestManager.StartQuest(questId);
    }

    /// <summary>
    /// ตัวอย่าง: เพิ่มไอเทมให้ผู้เล่น
    /// </summary>
    public void GiveItem(string itemId)
    {
        Debug.Log($"🎁 Giving item: {itemId}");
        // TODO: เรียก InventoryManager.AddItem(itemId);
    }

    /// <summary>
    /// ตัวอย่าง: ลบไอเทมจากผู้เล่น
    /// </summary>
    public void RemoveItem(string itemId)
    {
        Debug.Log($"❌ Removing item: {itemId}");
        // TODO: เรียก InventoryManager.RemoveItem(itemId);
    }

    /// <summary>
    /// ตัวอย่าง: เปลี่ยน Scene
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"🚪 Loading scene: {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// ตัวอย่าง: Spawn Enemy
    /// </summary>
    public void SpawnEnemy(string enemyPrefabName)
    {
        Debug.Log($"👾 Spawning enemy: {enemyPrefabName}");
        // TODO: Instantiate enemy prefab
    }

    /// <summary>
    /// ตัวอย่าง: ตั้งค่า Game State
    /// </summary>
    public void SetGameState(string stateName, bool value)
    {
        Debug.Log($"⚙️ Setting game state: {stateName} = {value}");
        PlayerPrefs.SetInt(stateName, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}

/// <summary>
/// Entry สำหรับแต่ละ event ที่ลงทะเบียน
/// </summary>
[System.Serializable]
public class DialogueEventEntry
{
    [Tooltip("ชื่อ event (ใช้เรียกจาก Dialogue)")]
    public string eventName;

    [Header("Events")]
    public UnityEvent onTrigger;
    public UnityEvent<string> onTriggerWithString;
    public UnityEvent<int> onTriggerWithInt;
}
