using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Action ที่สามารถเกิดขึ้นระหว่างหรือหลัง Dialogue
/// รองรับ: การเคลื่อนไหว, animation, sound, camera, effects, custom events
/// </summary>
[System.Serializable]
public class DialogueAction
{
    [Header("⚙️ Action Type")]
    public ActionType actionType = ActionType.PlayAnimation;

    [Header("⏱️ Timing")]
    [Tooltip("กำหนดว่า Action นี้จะเล่นเมื่อไหร่")]
    public ActionTiming timing = ActionTiming.OnNodeEnter;
    [Tooltip("รอกี่วินาทีก่อนเริ่ม action นี้")]
    public float delayBefore = 0f;
    [Tooltip("รอให้ action นี้เสร็จก่อนไป action ถัดไป?")]
    public bool waitForCompletion = true;

    [Header("🎭 Animation")]
    public Animator targetAnimator;
    public string animationTrigger;
    public string animationStateName; // สำหรับ SetBool/SetFloat

    [Header("🎵 Sound")]
    public AudioClip soundClip;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("🚶 Movement")]
    public Transform moveTarget; // Object ที่จะเคลื่อนที่
    public Transform moveDestination; // ไปที่ไหน
    public float moveSpeed = 3f;
    public bool lookAtDirection = true; // หันหน้าไปทางที่เดิน

    [Header("📹 Camera")]
    public CameraActionType cameraAction = CameraActionType.FocusOnTarget;
    public Transform cameraTarget;
    public float cameraZoom = 5f;
    public float cameraDuration = 1f;

    [Header("✨ Effects")]
    public GameObject effectPrefab;
    public Transform effectSpawnPoint;
    public bool destroyEffectOnComplete = true;
    public float effectDuration = 2f;

    [Header("👻 Object Control")]
    public GameObject targetObject;
    public bool setActive = true; // true = เปิด, false = ปิด

    [Header("🔧 Custom")]
    [Tooltip("เรียก UnityEvent ที่กำหนดเอง")]
    public UnityEvent customEvent;
    
    [Tooltip("เรียก method จาก GameObject ที่ระบุ")]
    public GameObject customEventTarget;
    public string customMethodName;
}

[System.Serializable]
public enum ActionType
{
    PlayAnimation,      // เล่น animation
    PlaySound,          // เล่นเสียง
    MoveCharacter,      // ย้ายตัวละคร/NPC
    CameraAction,       // ควบคุมกล้อง
    SpawnEffect,        // สร้าง effect/particle
    SetObjectActive,    // เปิด/ปิด GameObject
    CustomEvent,        // เรียก UnityEvent หรือ Method ที่กำหนดเอง
    Wait                // รอเวลาที่กำหนด
}

[System.Serializable]
public enum ActionTiming
{
    OnNodeEnter,        // เล่นทันทีที่เข้า node นี้
    OnTextComplete,     // เล่นเมื่อข้อความแสดงเสร็จ (typewriter เสร็จ)
    OnNodeExit,         // เล่นก่อนออกจาก node (กดปุ่ม next แล้ว)
    OnDialogueEnd       // เล่นเมื่อ dialogue ทั้งหมดจบ
}

[System.Serializable]
public enum CameraActionType
{
    FocusOnTarget,      // ย้ายกล้องไปโฟกัสที่เป้าหมาย
    Zoom,               // ซูมเข้า/ออก
    Shake,              // เขย่ากล้อง
    ReturnToPlayer      // กลับไปติดตาม Player
}

/// <summary>
/// Component สำหรับ Execute DialogueAction
/// </summary>
public class DialogueActionExecutor : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Execute action เดียว
    /// </summary>
    public IEnumerator ExecuteAction(DialogueAction action)
    {
        // รอตาม delay
        if (action.delayBefore > 0f)
        {
            yield return new WaitForSeconds(action.delayBefore);
        }

        // Execute ตาม type
        switch (action.actionType)
        {
            case ActionType.PlayAnimation:
                yield return ExecuteAnimation(action);
                break;

            case ActionType.PlaySound:
                ExecuteSound(action);
                break;

            case ActionType.MoveCharacter:
                yield return ExecuteMovement(action);
                break;

            case ActionType.CameraAction:
                yield return ExecuteCameraAction(action);
                break;

            case ActionType.SpawnEffect:
                yield return ExecuteSpawnEffect(action);
                break;

            case ActionType.SetObjectActive:
                ExecuteSetActive(action);
                break;

            case ActionType.CustomEvent:
                ExecuteCustomEvent(action);
                break;

            case ActionType.Wait:
                yield return new WaitForSeconds(action.delayBefore);
                break;
        }
    }

    IEnumerator ExecuteAnimation(DialogueAction action)
    {
        if (action.targetAnimator == null)
        {
            Debug.LogWarning("No animator assigned for animation action");
            yield break;
        }

        if (!string.IsNullOrEmpty(action.animationTrigger))
        {
            action.targetAnimator.SetTrigger(action.animationTrigger);
        }

        if (action.waitForCompletion && !string.IsNullOrEmpty(action.animationStateName))
        {
            // รอให้ animation state นั้นจบ
            yield return new WaitForSeconds(0.1f); // ให้เวลา transition
            while (action.targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(action.animationStateName))
            {
                yield return null;
            }
        }
    }

    void ExecuteSound(DialogueAction action)
    {
        if (action.soundClip == null)
        {
            Debug.LogWarning("No sound clip assigned");
            return;
        }

        AudioSource.PlayClipAtPoint(action.soundClip, mainCamera.transform.position, action.volume);
    }

    IEnumerator ExecuteMovement(DialogueAction action)
    {
        if (action.moveTarget == null || action.moveDestination == null)
        {
            Debug.LogWarning("Movement target or destination not assigned");
            yield break;
        }

        Vector3 startPos = action.moveTarget.position;
        Vector3 targetPos = action.moveDestination.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / action.moveSpeed;
        float elapsed = 0f;

        // ถ้าต้องการหันหน้าไปทางที่เดิน
        if (action.lookAtDirection)
        {
            Vector3 direction = (targetPos - startPos).normalized;
            if (direction.x != 0)
            {
                action.moveTarget.localScale = new Vector3(
                    Mathf.Sign(direction.x) * Mathf.Abs(action.moveTarget.localScale.x),
                    action.moveTarget.localScale.y,
                    action.moveTarget.localScale.z
                );
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            action.moveTarget.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        action.moveTarget.position = targetPos;
    }

    IEnumerator ExecuteCameraAction(DialogueAction action)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        switch (action.cameraAction)
        {
            case CameraActionType.FocusOnTarget:
                if (action.cameraTarget != null)
                {
                    Vector3 startPos = mainCamera.transform.position;
                    Vector3 targetPos = new Vector3(
                        action.cameraTarget.position.x,
                        action.cameraTarget.position.y,
                        startPos.z
                    );

                    float elapsed = 0f;
                    while (elapsed < action.cameraDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / action.cameraDuration;
                        mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                        yield return null;
                    }
                }
                break;

            case CameraActionType.Shake:
                yield return CameraShake(action.cameraDuration, 0.2f);
                break;

            // เพิ่ม case อื่นๆ ตามต้องการ
        }
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    IEnumerator ExecuteSpawnEffect(DialogueAction action)
    {
        if (action.effectPrefab == null)
        {
            Debug.LogWarning("No effect prefab assigned");
            yield break;
        }

        Vector3 spawnPos = action.effectSpawnPoint != null 
            ? action.effectSpawnPoint.position 
            : Vector3.zero;

        GameObject effect = Instantiate(action.effectPrefab, spawnPos, Quaternion.identity);

        if (action.destroyEffectOnComplete)
        {
            Destroy(effect, action.effectDuration);
        }

        if (action.waitForCompletion)
        {
            yield return new WaitForSeconds(action.effectDuration);
        }
    }

    void ExecuteSetActive(DialogueAction action)
    {
        if (action.targetObject != null)
        {
            action.targetObject.SetActive(action.setActive);
        }
    }

    void ExecuteCustomEvent(DialogueAction action)
    {
        // เรียก UnityEvent
        action.customEvent?.Invoke();

        // เรียก method ที่ระบุ
        if (action.customEventTarget != null && !string.IsNullOrEmpty(action.customMethodName))
        {
            action.customEventTarget.SendMessage(action.customMethodName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
