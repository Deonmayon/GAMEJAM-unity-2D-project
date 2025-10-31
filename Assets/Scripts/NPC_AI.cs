using UnityEngine;
using System.Collections;

public class NPC_AI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public string currentFloor = "Floor1";
    public float patrolDuration = 10f;
    public float patrolSpeed = 2f;
    public float patrolDistance = 15f;

    [Header("NPC Type")]
    public bool isGuard = true;

    private float patrolTimer = 0f;
    private WarpLocationManager.WarpLocationInfo targetWarp;
    private bool movingToWarp = false;

    private Vector2 startPosition;
    private bool movingRight = true;
    private float leftBound;
    private float rightBound;

    void Start()
    {
        patrolTimer = patrolDuration;
        startPosition = transform.position;

        // ✅ Register this as the controllable Guard
        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.RegisterGuard(this);
        }
        else
        {
            Debug.LogError($"❌ {gameObject.name}: WarpLocationManager not found!");
        }

        ResetPatrol(); // Call it once at the start
    }

    // Add this NEW public method to NPC_AI.cs
    public void ResetPatrol()
    {
        Debug.Log($"🔄 {name} is resetting patrol boundaries on floor '{currentFloor}'.");
        startPosition = transform.position;
        leftBound = startPosition.x - patrolDistance;
        rightBound = startPosition.x + patrolDistance;
        movingRight = Random.value > 0.5f;
        patrolTimer = patrolDuration;
    }

    void Update()
    {
        if (!movingToWarp)
        {
            SimplePatrol();
            FlipSprite();

            patrolTimer -= Time.deltaTime;

            if (patrolTimer <= 0f)
            {
                DecideToChangeFloor();
            }
        }
        else
        {
            MoveTowardsWarp();
        }
    }

    void SimplePatrol()
    {
        Vector2 newPos = transform.position;

        if (movingRight)
        {
            newPos.x += patrolSpeed * Time.deltaTime;
            if (newPos.x >= rightBound)
            {
                newPos.x = rightBound;
                movingRight = false;
            }
        }
        else
        {
            newPos.x -= patrolSpeed * Time.deltaTime;
            if (newPos.x <= leftBound)
            {
                newPos.x = leftBound;
                movingRight = true;
            }
        }

        transform.position = newPos;
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;

        if (movingRight)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    private void DecideToChangeFloor()
    {
        Debug.Log($"🤔 {gameObject.name} on '{currentFloor}' deciding to change floor...");

        // ✅ Clear this NPC from any destination tracking before finding new warp
        WarpLocationManager.Instance.ClearNPCFromDestination(transform, currentFloor);

        targetWarp = WarpLocationManager.Instance.GetRandomAvailableWarp(currentFloor, transform);

        if (targetWarp != null)
        {
            if (WarpLocationManager.Instance.ReserveWarpLocation(targetWarp, transform))
            {
                Debug.Log($"✅ {gameObject.name} heading to {targetWarp.locationName}");
                movingToWarp = true;
            }
            else
            {
                Debug.Log($"⏳ {gameObject.name} warp taken, will try again soon");
                patrolTimer = 2f;
            }
        }
        else
        {
            Debug.LogWarning($"❌ {gameObject.name}: No available warps on '{currentFloor}' - continuing patrol");
            patrolTimer = patrolDuration * 0.5f;
        }
    }

    void MoveTowardsWarp()
    {
        if (targetWarp == null) return;

        Transform warpTransform = targetWarp.warpInteractable.transform;

        Vector2 newPos = transform.position;
        newPos.x = Mathf.MoveTowards(transform.position.x, warpTransform.position.x, patrolSpeed * Time.deltaTime);
        transform.position = newPos;

        Vector3 scale = transform.localScale;
        if (warpTransform.position.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        float distance = Mathf.Abs(transform.position.x - warpTransform.position.x);
        if (distance < 0.5f)
        {
            UseWarp();
        }
    }

    void UseWarp()
    {
        if (targetWarp == null) return;

        Debug.Log($"🚪 {gameObject.name} using warp {targetWarp.locationName}");

        Interactable warpInteractable = targetWarp.warpInteractable;

        if (warpInteractable.warpTarget != null)
        {
            string oldFloor = currentFloor;

            transform.position = warpInteractable.warpTarget.position;
            currentFloor = targetWarp.destinationFloor;

            Debug.Log($"✅ {gameObject.name} now on '{currentFloor}'");

            startPosition = transform.position;
            leftBound = startPosition.x - patrolDistance;
            rightBound = startPosition.x + patrolDistance;

            // ✅ Release warp and update floor tracking
            WarpLocationManager.Instance.ReleaseWarpLocation(targetWarp, transform);
        }

        targetWarp = null;
        movingToWarp = false;
        patrolTimer = patrolDuration;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector2(leftBound, transform.position.y), new Vector2(rightBound, transform.position.y));
            Gizmos.DrawWireSphere(new Vector2(leftBound, transform.position.y), 0.3f);
            Gizmos.DrawWireSphere(new Vector2(rightBound, transform.position.y), 0.3f);
        }

        if (movingToWarp && targetWarp != null)
        {
            Gizmos.color = isGuard ? Color.blue : Color.red;
            Gizmos.DrawLine(transform.position, targetWarp.warpInteractable.transform.position);
        }
    }
}