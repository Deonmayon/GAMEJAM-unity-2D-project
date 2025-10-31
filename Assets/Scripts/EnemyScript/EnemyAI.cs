using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Stalk, Hunt }
    public EnemyState currentState = EnemyState.Patrol;
    private bool wasPatrolling = true;
    public bool isGuard = false; // false = stalker

    [Header("Patrol Settings")]
    public string currentFloor = "Floor1";
    public string playerCurrentFloor = "Floor1";
    public float patrolDuration = 10f;
    public float patrolSpeed = 2f;
    public float patrolDistance = 15f;

    [Header("Stalk Settings")]
    public float stalkSpeed = 2.5f;
    public float preferredDistance = 10f;
    public float stalkMaintainRange = 12f;

    [Header("Hunt Settings")]
    public float huntSpeed = 5f;

    [Header("References")]
    public EnemyVision vision;
    public PlayerVision playerVision;

    private Transform player;
    private Rigidbody2D rb;

    // Patrol variables
    private float patrolTimer = 0f;
    private WarpLocationManager.WarpLocationInfo targetWarp;
    private bool movingToWarp = false;
    private Vector2 startPosition;
    private bool movingRight = true;
    private float leftBound;
    private float rightBound;

    private bool playerIsHiding = false;
    private bool inHuntMode = false;
    private bool inStalkMode = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.RegisterEnemy(this);
        }
        // Setup patrol boundaries
        patrolTimer = patrolDuration;
        startPosition = transform.position;
        leftBound = startPosition.x - patrolDistance;
        rightBound = startPosition.x + patrolDistance;
        movingRight = Random.value > 0.5f;

        // ✅ Register this NPC to WarpLocationManager
        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.RegisterNPC(transform, currentFloor);
            Debug.Log($"✅ {gameObject.name} registered on floor '{currentFloor}'");
        }
        else
        {
            Debug.LogError($"❌ {gameObject.name}: WarpLocationManager not found!");
        }
    }
    public void ResetPatrol()
    {
        // Recalculate patrol boundaries based on the NPC's current transform position
        startPosition = transform.position;
        leftBound = startPosition.x - patrolDistance;
        rightBound = startPosition.x + patrolDistance;
        movingRight = Random.value > 0.5f;
        patrolTimer = patrolDuration;
        currentState = EnemyState.Patrol;
        movingToWarp = false;
        targetWarp = null;
        // You may need to call StopMoving() here if your EnemyAI has one.
        // Example: StopMoving(); 
        Debug.Log("✅ Enemy state fully reset.");
    }

    void Update()
    {
        if (player == null) return;

        UpdateState();
        HandleBehavior();
        FlipSprite();
    }

    // ใน EnemyAI.cs
private void UpdateState()
{
    // Do NOT run if the GameObject is disabled (Enemy will be inactive when hiding)
    if (!gameObject.activeInHierarchy) return;

    EnemyState previousState = currentState;

    // ✅ NEW: ตรวจสอบสถานะการซ่อนตัวของผู้เล่นก่อนการตรวจสอบวิสัยทัศน์
    if (playerIsHiding)
    {
        // ถ้าผู้เล่นซ่อนตัวอยู่ ให้บังคับสถานะเป็น Patrol เสมอ
        currentState = EnemyState.Patrol;
        inHuntMode = false;
        inStalkMode = false;
    }
    else // ผู้เล่นไม่ได้ซ่อนตัวอยู่ (สามารถตรวจสอบวิสัยทัศน์ได้)
    {
        // --- State Determination (based on vision only) ---
        bool enemySeesPlayer = vision != null && vision.CanSeePlayer();
        bool playerSeesEnemy = playerVision != null && playerVision.CanSeeEnemy(this.transform);

        if (playerSeesEnemy || inHuntMode)
        {
            currentState = EnemyState.Hunt;
            inHuntMode = true;
        }
        else if (enemySeesPlayer || inStalkMode)
        {
            currentState = EnemyState.Stalk;
            inStalkMode = true;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }

    if (WarpLocationManager.Instance != null)
    {
        // Transition: Patrol -> Aggressive. Disable the Guard.
        if (previousState == EnemyState.Patrol && currentState != EnemyState.Patrol)
        {
            WarpLocationManager.Instance.DisableGuard();
        }
        // Transition: Aggressive -> Patrol. Respawn the Guard.
        else if (previousState != EnemyState.Patrol && currentState == EnemyState.Patrol)
        {
            WarpLocationManager.Instance.RespawnGuard();
        }
    }
}

    // In EnemyAI.cs, replace the existing HandleBehavior() method

    private void HandleBehavior()
    {
        // Check if player is on a different floor and we are NOT hiding
        bool playerOnDifferentFloor = currentFloor != playerCurrentFloor;
        bool isAggressive = currentState == EnemyState.Stalk || currentState == EnemyState.Hunt;

        if (isAggressive && playerOnDifferentFloor)
        {
            if (!movingToWarp)
            {
                DecideToChangeFloorForChase();
            }

            if (movingToWarp)
            {
                MoveTowardsWarp();
                return; // Stop other behaviors while warping
            }
        }
        switch (currentState)
        {
            case EnemyState.Hunt:
                Hunt();
                break;

            case EnemyState.Stalk:
                Stalk();
                break;

            case EnemyState.Patrol:
                if (!movingToWarp)
                {
                    SimplePatrol();
                    // Timer runs down only during simple patrol
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
                break;
        }
    }

    public void OnPlayerHiding()
    {
        playerIsHiding = true;
        // The user's request: Disable the Enemy to force a complete reservation reset
        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.DisableEnemy(); // Disables the Enemy GameObject and unregisters it
        }

        // Also respawn the Guard when the player hides
        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.RespawnGuard();
        }
    }

    public void OnPlayerUnHiding()
    {
        playerIsHiding = false;
        if (WarpLocationManager.Instance != null)
        {
            WarpLocationManager.Instance.RespawnEnemy();
        }
    }

    private void SimplePatrol()
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

        rb.MovePosition(newPos);
    }

    private void Stalk()
    {
        float distance = Mathf.Abs(transform.position.x - player.position.x);
        float targetX = transform.position.x;

        // Too far - move closer
        if (distance > preferredDistance + 1f && distance < stalkMaintainRange)
        {
            Debug.Log("Too far bro");
            targetX = player.position.x;
            MoveTowardsX(targetX, stalkSpeed);
        }
        // Too close - back off
        else if (distance < preferredDistance - 1f)
        {
            targetX = transform.position.x + (transform.position.x > player.position.x ? 1.5f : -1.5f);
            MoveTowardsX(targetX, stalkSpeed);
        }
        else
        {
            StopMoving();
        }
    }

    // 🏃 Hunt behavior - chase player
    private void Hunt()
    {
        MoveTowardsX(player.position.x, huntSpeed);
    }

    // ⏩ Move towards warp location
    private void MoveTowardsWarp()
    {
        if (targetWarp == null) return;

        Transform warpTransform = targetWarp.warpInteractable.transform;

        Vector2 newPos = transform.position;
        newPos.x = Mathf.MoveTowards(transform.position.x, warpTransform.position.x, patrolSpeed * Time.deltaTime);
        rb.MovePosition(newPos);

        // Check if reached
        float distance = Mathf.Abs(transform.position.x - warpTransform.position.x);
        if (distance < 0.5f)
        {
            UseWarp();
        }
    }

    // 🚪 Use the warp
    private void UseWarp()
    {
        if (targetWarp == null) return;

        Debug.Log($"🚪 {gameObject.name} using warp {targetWarp.locationName}");

        Interactable warpInteractable = targetWarp.warpInteractable;

        if (warpInteractable.warpTarget != null)
        {
            string oldFloor = currentFloor;

            // Teleport
            transform.position = warpInteractable.warpTarget.position;
            currentFloor = targetWarp.destinationFloor;

            Debug.Log($"✅ {gameObject.name} moved: '{oldFloor}' → '{currentFloor}'");

            // Reset patrol boundaries on new floor
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

    // 🚪 Decide to find a warp and change floors
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

    private void DecideToChangeFloorForChase()
    {
        Debug.Log($"🏃‍♂️ {gameObject.name} in {currentState} state. Chasing to floor '{playerCurrentFloor}'!");

        // No need to clear old destination here, as the Guard logic handles the release.
        // We just need a random available warp from the current floor.
        targetWarp = WarpLocationManager.Instance.GetRandomAvailableWarp(currentFloor, transform);

        if (targetWarp != null)
        {
            // Check if the warp destination is the player's floor
            if (targetWarp.destinationFloor == playerCurrentFloor)
            {
                if (WarpLocationManager.Instance.ReserveWarpLocation(targetWarp, transform))
                {
                    Debug.Log($"✅ {gameObject.name} heading to {targetWarp.locationName} to chase!");
                    movingToWarp = true;
                }
                else
                {
                    Debug.Log($"⏳ {gameObject.name} warp taken by other NPC, waiting 1s...");
                    // Note: The timer logic is complex to manage here, so we just wait for the next frame.
                }
            }
            else
            {
                // If the random warp doesn't lead to the player, try again next frame.
                targetWarp = null;
                Debug.Log("Warp destination doesn't match player floor. Will re-check next frame.");
            }
        }
        else
        {
            Debug.LogWarning($"❌ {gameObject.name}: No available warps on '{currentFloor}' to chase to.");
        }
    }

    // ⏩ Move towards X position
    private void MoveTowardsX(float targetX, float speed)
    {
        if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
        {
            StopMoving();
            return;
        }

        Vector2 newPos = transform.position;
        newPos.x = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    // 🛑 Stop movement
    private void StopMoving()
    {
        if (rb != null)
        {
            float vx = Mathf.Abs(rb.linearVelocity.x) < 0.01f ? 0f : rb.linearVelocity.x;
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        }
    }

    // 🔄 Flip sprite toward player or movement direction
    private void FlipSprite()
    {
        Vector3 scale = transform.localScale;

        if (currentState == EnemyState.Patrol && !movingToWarp)
        {
            // Flip based on patrol direction
            if (movingRight)
                scale.x = Mathf.Abs(scale.x);
            else
                scale.x = -Mathf.Abs(scale.x);
        }
        else if (player != null)
        {
            // Flip toward player (Stalk/Hunt) or warp
            float targetX = movingToWarp && targetWarp != null
                ? targetWarp.warpInteractable.transform.position.x
                : player.position.x;

            if (targetX > transform.position.x)
                scale.x = Mathf.Abs(scale.x);
            else
                scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector2(leftBound, transform.position.y), new Vector2(rightBound, transform.position.y));

            Gizmos.color = currentState == EnemyState.Hunt ? Color.red :
                          currentState == EnemyState.Stalk ? Color.orange :
                          Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (movingToWarp && targetWarp != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, targetWarp.warpInteractable.transform.position);
            }
        }
    }
}