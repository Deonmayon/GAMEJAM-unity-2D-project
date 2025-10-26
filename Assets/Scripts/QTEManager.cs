using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class QTEManager : MonoBehaviour
{
    public RectTransform targetZone;
    public RectTransform spawnPoint;
    public GameObject heartPrefab;
    public float speed = 400f;
    public float pauseOnFailDuration = 1f; // หยุดชั่วคราวเมื่อ fail (วินาที)
    public RectTransform qteTrack; // เพิ่ม reference ไป QTETrack
    public float minSpawnDelay = 0.5f; // ระยะห่างขั้นต่ำระหว่างหัวใจ
    public float maxSpawnDelay = 2f; // ระยะห่างสูงสุดระหว่างหัวใจ

    private List<RectTransform> activeHearts = new List<RectTransform>(); // เก็บหัวใจที่กำลังเคลื่อนไหว
    private bool isMoving = false;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private int score = 0;
    private int missCount = 0;
    private float nextSpawnTime = 0f;
    private bool isQTEActive = false; // ตัวแปรควบคุมการเปิด/ปิด QTE
    private CanvasGroup canvasGroup; // สำหรับซ่อน/แสดง UI
    private Coroutine spawnDelayCoroutine; // เก็บ reference ของ coroutine

    void Start()
    {
        // Auto-detect QTETrack ถ้าไม่ได้กำหนด
        if (qteTrack == null)
        {
            qteTrack = GetComponent<RectTransform>();
            if (qteTrack != null)
            {
                Debug.Log("🎯 Auto-detected QTETrack: " + gameObject.name);
            }
        }
        
        // หา หรือ เพิ่ม CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // ซ่อน QTE UI ตอนเริ่มต้น
        HideQTE();
    }

    void Update()
    {
        // ไม่ทำงานถ้า QTE ไม่ได้เปิดใช้งาน
        if (!isQTEActive) return;

        // ตรวจสอบ pause timer
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                isPaused = false;
            }
            return; // ไม่ทำอะไรต่อถ้ายัง pause อยู่
        }

        // เลื่อนหัวใจทั้งหมดที่กำลังเคลื่อนไหว
        MoveAllHearts();

        // ตรวจสอบเวลา spawn หัวใจคู่ใหม่
        if (Time.time >= nextSpawnTime && !isPaused)
        {
            SpawnHeartPair();
        }

        // รองรับทั้ง Input System เก่าและใหม่
        bool spacePressed = false;
        
        try
        {
            // ลองใช้ Input System ใหม่ก่อน
            if (Keyboard.current != null)
            {
                spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            }
        }
        catch
        {
            // หาก Input System ใหม่ไม่พร้อมใช้งาน ใช้แบบเก่า
            spacePressed = false;
        }
        
        // หากไม่ได้กด space จาก Input System ใหม่ ลองใช้แบบเก่า
        if (!spacePressed)
        {
            try
            {
                spacePressed = Input.GetKeyDown(KeyCode.Space);
            }
            catch
            {
                // ถ้าทั้งสองแบบไม่ทำงาน
                spacePressed = false;
            }
        }
        
        if (spacePressed)
        {
            CheckHit();
        }
    }

    void MoveAllHearts()
    {
        // คำนวณขอบซ้ายของ QTETrack
        float trackLeftBound = 0f;
        if (qteTrack != null)
        {
            trackLeftBound = qteTrack.anchoredPosition.x - (qteTrack.rect.width / 2);
        }
        else
        {
            trackLeftBound = -600f;
        }

        // เลื่อนหัวใจทั้งหมดและตรวจสอบหัวใจที่ออกนอกขอบ
        for (int i = activeHearts.Count - 1; i >= 0; i--)
        {
            if (activeHearts[i] != null)
            {
                activeHearts[i].anchoredPosition += Vector2.left * speed * Time.deltaTime;

                // ตรวจสอบว่าหัวใจเลื่อนออกนอก QTETrack หรือไม่
                float heartRightEdge = activeHearts[i].anchoredPosition.x + (activeHearts[i].rect.width / 2);
                if (heartRightEdge < trackLeftBound)
                {
                    missCount++;
                    Debug.Log($"❌ FAIL! หัวใจเลื่อนออกจาก QTETrack แล้ว! Miss: {missCount}");
                    Destroy(activeHearts[i].gameObject);
                    activeHearts.RemoveAt(i);
                    
                    // หยุดชั่วคราว
                    isPaused = true;
                    pauseTimer = pauseOnFailDuration;
                }
            }
            else
            {
                activeHearts.RemoveAt(i);
            }
        }
    }

    void SpawnHeartPair()
    {
        if (!isQTEActive) return; // ไม่ spawn ถ้า QTE ไม่ได้เปิด
        
        // Spawn หัวใจลูกแรก
        SpawnSingleHeart();
        
        // Spawn หัวใจลูกที่สองหลังจาก 0.2-0.5 วินาที (เหมือนจังหวะหัวใจ)
        if (spawnDelayCoroutine != null)
        {
            StopCoroutine(spawnDelayCoroutine);
        }
        spawnDelayCoroutine = StartCoroutine(SpawnSecondHeartDelayed());
        
        // กำหนดเวลา spawn คู่ต่อไป (random)
        nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    System.Collections.IEnumerator SpawnSecondHeartDelayed()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        
        // ตรวจสอบอีกครั้งว่า QTE ยังเปิดอยู่หรือไม่
        if (!isPaused && isQTEActive)
        {
            SpawnSingleHeart();
        }
        
        // เคลียร์ reference
        spawnDelayCoroutine = null;
    }

    void SpawnSingleHeart()
    {
        // ไม่ spawn ถ้า QTE ไม่ได้เปิดใช้งาน
        if (!isQTEActive)
        {
            Debug.Log("⚠️ ไม่ spawn หัวใจเพราะ QTE ปิดอยู่");
            return;
        }
        
        if (heartPrefab == null)
        {
            Debug.LogError("HeartPrefab ไม่ได้ถูกกำหนดใน Inspector!");
            return;
        }

        GameObject newHeart = Instantiate(heartPrefab, transform);
        
        // ตั้งชื่อให้ง่ายต่อการติดตาม
        newHeart.name = $"HeartIndicator(Clone)_{Time.time:F2}";
        
        RectTransform heartRect = newHeart.GetComponent<RectTransform>();
        
        // ตรวจสอบว่ามี RectTransform หรือไม่
        if (heartRect == null)
        {
            Debug.LogError("HeartPrefab ต้องมี RectTransform component! กำลังเพิ่มให้อัตโนมัติ...");
            heartRect = newHeart.AddComponent<RectTransform>();
            
            if (heartRect == null)
            {
                Debug.LogError("ไม่สามารถเพิ่ม RectTransform ได้! ลบ GameObject แล้ว");
                Destroy(newHeart);
                return;
            }
        }
        
        if (spawnPoint != null)
        {
            heartRect.anchoredPosition = spawnPoint.anchoredPosition;
        }
        else
        {
            Debug.LogError("SpawnPoint ไม่ได้ถูกกำหนดใน Inspector!");
            heartRect.anchoredPosition = Vector2.zero;
        }
        
        // เพิ่มเข้า list
        activeHearts.Add(heartRect);
        isMoving = true;
        
        Debug.Log($"💖 Spawn หัวใจใหม่: {newHeart.name} (รวม: {activeHearts.Count} ตัว)");
    }

    // ฟังก์ชันสำหรับดูคะแนนปัจจุบัน
    public int GetScore()
    {
        return score;
    }

    public int GetMissCount()
    {
        return missCount;
    }

    // ฟังก์ชันรีเซ็ตเกม (ไม่เคลียร์ clones)
    public void ResetGame()
    {
        score = 0;
        missCount = 0;
        isPaused = false;
        pauseTimer = 0f;
        nextSpawnTime = Time.time + 1f;
        
        // ทำลายหัวใจใน list เท่านั้น (ไม่ค้นหาใน scene)
        foreach (RectTransform heart in activeHearts)
        {
            if (heart != null)
            {
                Destroy(heart.gameObject);
            }
        }
        activeHearts.Clear();
        
        // เริ่มเกมใหม่
        SpawnHeartPair();
        Debug.Log("🔄 เกมรีเซ็ตแล้ว!");
    }

    // ฟังก์ชันเปิด QTE (เรียกจาก PlayerInteract)
    public void StartQTE()
    {
        isQTEActive = true;
        ShowQTE();
        
        // เคลียร์ clones ที่อาจเหลืออยู่ก่อนเริ่มใหม่
        ClearAllHeartClones();
        
        // รีเซ็ตค่าต่าง ๆ แต่ไม่เคลียร์ clones อีก
        score = 0;
        missCount = 0;
        isPaused = false;
        pauseTimer = 0f;
        nextSpawnTime = Time.time + 1f;
        
        // เริ่มเกมใหม่
        SpawnHeartPair();
        Debug.Log("🎮 เริ่ม QTE - กดเกมเริ่มแล้ว!");
    }

    // ฟังก์ชันปิด QTE (เรียกจาก PlayerInteract)
    public void StopQTE()
    {
        isQTEActive = false;
        
        // หยุด coroutine ที่อาจกำลัง spawn อยู่
        if (spawnDelayCoroutine != null)
        {
            StopCoroutine(spawnDelayCoroutine);
            spawnDelayCoroutine = null;
        }
        
        // หยุด coroutines ทั้งหมด (เผื่อมีหลายตัว)
        StopAllCoroutines();
        
        // เคลียร์ HeartIndicator clones ทั้งหมดทันที
        ClearAllHeartClones();
        
        // ซ่อน UI
        HideQTE();
        Debug.Log("🛑 หยุด QTE - ออกจากตู้แล้ว!");
    }

    // ฟังก์ชันซ่อน QTE UI
    void HideQTE()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // ฟังก์ชันแสดง QTE UI
    void ShowQTE()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // ฟังก์ชันเคลียร์ HeartIndicator clones ทั้งหมด
    void ClearAllHeartClones()
    {
        // ทำลายหัวใจทั้งหมดใน activeHearts list ก่อน
        foreach (RectTransform heart in activeHearts)
        {
            if (heart != null)
            {
                Destroy(heart.gameObject);
            }
        }
        activeHearts.Clear();
        
        // รอสักครู่แล้วค่อยเคลียร์ clones ใน scene
        StartCoroutine(DelayedClearAllClones());
    }
    
    // ฟังก์ชันเคลียร์ clones หลังจากรอสักครู่
    System.Collections.IEnumerator DelayedClearAllClones()
    {
        // รอให้ spawn ที่อาจกำลังเกิดขึ้นเสร็จก่อน
        yield return new WaitForSeconds(0.1f);
        
        // ค้นหาและทำลาย GameObject ทั้งหมดที่มีชื่อเกี่ยวกับ HeartIndicator
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int clearedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // ข้าม GameObject ที่เพิ่งถูกสร้างใน 0.2 วินาทีที่ผ่านมา
            if (obj != null && obj != heartPrefab)
            {
                // ตรวจสอบชื่อแบบหลายรูปแบบ
                if ((obj.name.Contains("HeartIndicator") && obj.name.Contains("Clone")) ||
                    obj.name.StartsWith("HeartIndicator(Clone)") ||
                    obj.name.Contains("HeartIndicator(Clone)_"))
                {
                    // ตรวจสอบว่าไม่อยู่ใน activeHearts (เผื่อเพิ่งถูกสร้าง)
                    bool isInActiveList = false;
                    RectTransform objRect = obj.GetComponent<RectTransform>();
                    if (objRect != null)
                    {
                        foreach (RectTransform activeHeart in activeHearts)
                        {
                            if (activeHeart == objRect)
                            {
                                isInActiveList = true;
                                break;
                            }
                        }
                    }
                    
                    // ทำลายเฉพาะที่ไม่อยู่ใน active list
                    if (!isInActiveList)
                    {
                        Destroy(obj);
                        clearedCount++;
                    }
                }
            }
        }
        
        if (clearedCount > 0)
        {
            Debug.Log($"🧹 เคลียร์ HeartIndicator clones จำนวน: {clearedCount} ตัว");
        }
    }

    void CheckHit()
    {
        if (activeHearts.Count == 0 || isPaused) 
        {
            return;
        }

        if (targetZone == null)
        {
            Debug.LogError("TargetZone ไม่ได้ถูกกำหนดใน Inspector!");
            return;
        }

        bool hitAny = false;
        
        // ตรวจสอบหัวใจทั้งหมดที่อยู่ในโซน
        for (int i = activeHearts.Count - 1; i >= 0; i--)
        {
            if (activeHearts[i] != null)
            {
                float heartLeft = activeHearts[i].anchoredPosition.x - (activeHearts[i].rect.width / 2);
                float heartRight = activeHearts[i].anchoredPosition.x + (activeHearts[i].rect.width / 2);
                float zoneLeft = targetZone.anchoredPosition.x - (targetZone.rect.width / 2);
                float zoneRight = targetZone.anchoredPosition.x + (targetZone.rect.width / 2);

                // ตรวจสอบว่าหัวใจอยู่ในโซนหรือไม่
                if (heartRight > zoneLeft && heartLeft < zoneRight)
                {
                    score++;
                    hitAny = true;
                    Debug.Log($"✅ PERFECT HIT! Score: {score}");
                    
                    // ทำลายหัวใจที่โดนแล้ว (แต่หัวใจตัวอื่นยังลอยต่อ)
                    Destroy(activeHearts[i].gameObject);
                    activeHearts.RemoveAt(i);
                    break; // ตี 1 ลูกต่อ 1 ครั้งกด
                }
            }
        }
        
        if (!hitAny)
        {
            missCount++;
            Debug.Log($"❌ MISS! กดผิดจังหวะ! Miss: {missCount}");
            
            // หยุดชั่วคราวเมื่อ miss
            isPaused = true;
            pauseTimer = pauseOnFailDuration;
        }
    }
}
