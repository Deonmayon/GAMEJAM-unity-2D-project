using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WarpLocationManager : MonoBehaviour
{
    public static WarpLocationManager Instance { get; private set; }

    [System.Serializable]
    public class WarpLocationInfo
    {
        public Interactable warpInteractable;
        public string locationName;
        public string currentFloor;
        public string destinationFloor;
        public Transform npcCurrentlyUsing;
        public List<Transform> npcsAtDestination = new List<Transform>();
    }

    [Header("Registered Warps")]
    public List<WarpLocationInfo> allWarpLocations = new List<WarpLocationInfo>();

    [Header("Debug Info")]
    public List<string> registeredFloors = new List<string>();
    public int totalWarps = 0;

    [Header("NPC Tracking")]
    public List<NPCDebugInfo> activeNPCs = new List<NPCDebugInfo>();

    // Track which NPCs are on which floors
    private Dictionary<string, List<Transform>> npcsOnFloor = new Dictionary<string, List<Transform>>();

    [System.Serializable]
    public class NPCDebugInfo
    {
        public Transform npcTransform;
        public string npcName;
        public string currentFloor;
        public bool isMovingToWarp;
        public string targetWarpName;
        public string arrivedViaWarp; // Track which warp they used to arrive
    }

    private GameObject guardNpcObject; // A reference to the Guard's GameObject
    private NPC_AI guardNpcScript;     // A reference to the Guard's script
    private string playerCurrentFloor = "Floor1"; // Track the player's floor

    private GameObject enemyNpcObject;
    private EnemyAI enemyNpcScript;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ WarpLocationManager initialized");
        }
        else
        {
            Debug.LogError("❌ Multiple WarpLocationManagers detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    // Add these THREE NEW methods to WarpLocationManager.cs
    // Call this from NPC_AI's Start() to let the manager control it.
    public void RegisterGuard(NPC_AI guard)
    {
        guardNpcScript = guard;
        guardNpcObject = guard.gameObject;
        Debug.Log($"🛡️ Guard NPC '{guard.name}' has been registered for dynamic control.");

        // Also do the normal registration
        RegisterNPC(guard.transform, guard.currentFloor);
    }

    // Call this from EnemyAI when it starts hunting.
    public void DisableGuard()
    {
        if (guardNpcObject != null && guardNpcObject.activeInHierarchy)
        {
            Debug.Log("❗️ Enemy is hunting. Disabling Guard NPC.");
            // Unregister the NPC so its floor becomes available
            if (activeNPCs.Any(npc => npc.npcTransform == guardNpcScript.transform))
            {
                UnregisterNPC(guardNpcScript.transform);
            }
            guardNpcObject.SetActive(false);
        }
    }

    // Call this from EnemyAI when it goes back to patrol.
    public void RespawnGuard()
    {
        if (guardNpcObject != null && !guardNpcObject.activeInHierarchy)
        {
            Debug.Log("...Player is safe. Attempting to respawn Guard NPC...");

            // Find the Enemy's current floor
            var enemyInfo = activeNPCs.FirstOrDefault(npc => npc.npcTransform.GetComponent<EnemyAI>() != null);
            if (enemyInfo == null)
            {
                Debug.LogError("Could not find Enemy in manager to determine safe floor.");
                return;
            }
            string enemyFloor = enemyInfo.currentFloor;

            // Find a "safe" floor (not the Player's floor, not the Enemy's floor)
            List<string> safeFloors = registeredFloors
                .Where(floor => floor != playerCurrentFloor && floor != enemyFloor)
                .ToList();

            if (safeFloors.Count > 0)
            {
                // Pick a random safe floor
                string spawnFloor = safeFloors[Random.Range(0, safeFloors.Count)];

                // Find a valid spawn point on that floor (any warp will do)
                var spawnPointWarp = allWarpLocations.FirstOrDefault(w => w.currentFloor == spawnFloor);
                if (spawnPointWarp != null)
                {
                    guardNpcObject.SetActive(true);

                    // Move the guard to the new location
                    guardNpcScript.transform.position = spawnPointWarp.warpInteractable.transform.position;

                    // IMPORTANT: Update the guard's internal floor variable BEFORE registering
                    guardNpcScript.currentFloor = spawnFloor;

                    // Re-register the guard in the system
                    RegisterNPC(guardNpcScript.transform, spawnFloor);
                    guardNpcScript.ResetPatrol(); // A new method we will add to NPC_AI

                    Debug.Log($"✅ Guard has respawned on safe floor: {spawnFloor}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Found safe floor '{spawnFloor}', but no warp points to spawn at. Guard remains disabled.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No safe floors available to respawn the Guard. It will remain disabled for now.");
            }
        }
    }

    public void RegisterEnemy(EnemyAI enemy)
    {
        enemyNpcScript = enemy;
        enemyNpcObject = enemy.gameObject;
        RegisterNPC(enemy.transform, enemy.currentFloor);
        Debug.Log($"🔪 Enemy NPC '{enemy.name}' has been registered for dynamic control.");
    }

    // 2. New Disable Method for the Enemy (Called when player HIDES)
    public void DisableEnemy()
    {
        if (enemyNpcObject != null && enemyNpcObject.activeInHierarchy)
        {
            Debug.Log("💤 Player is hiding. Disabling Enemy NPC to force state reset/warp clear.");

            // CRITICAL: Unregister to release floor occupancy and warp reservations
            if (activeNPCs.Any(npc => npc.npcTransform == enemyNpcScript.transform))
            {
                // The comprehensive UnregisterNPC (from previous steps) must be used here
                UnregisterNPC(enemyNpcScript.transform);
            }

            enemyNpcObject.SetActive(false);
        }
    }

    // 3. New Respawn Method for the Enemy (Called when player UN-HIDES)
    public void RespawnEnemy()
    {
        if (enemyNpcObject != null && !enemyNpcObject.activeInHierarchy)
        {
            Debug.Log("⚡️ Player unhid. Respawning Enemy NPC.");

            // Find a random warp on a random floor to move the Enemy
            List<string> allFloors = registeredFloors.ToList();
            if (allFloors.Count > 0)
            {
                string spawnFloor = allFloors[Random.Range(0, allFloors.Count)];
                var spawnPointWarp = allWarpLocations.FirstOrDefault(w => w.currentFloor == spawnFloor);

                if (spawnPointWarp != null)
                {
                    enemyNpcObject.SetActive(true);

                    // Move the enemy to the new location
                    enemyNpcScript.transform.position = spawnPointWarp.warpInteractable.transform.position;

                    // Update internal floor variable and re-register
                    enemyNpcScript.currentFloor = spawnFloor;
                    RegisterNPC(enemyNpcScript.transform, spawnFloor);
                    enemyNpcScript.ResetPatrol(); // Call the reset method we add to EnemyAI below

                    Debug.Log($"✅ Enemy has respawned on floor: {spawnFloor}");
                    return;
                }
            }

            // Fallback: just re-enable at current spot
            enemyNpcObject.SetActive(true);
            enemyNpcScript.ResetPatrol();
            Debug.LogWarning("⚠️ Could not find a safe warp for Enemy respawn. Re-enabled at current position.");
        }
    }

    // We also need a way to unregister NPCs. Add this new method.
    public void UnregisterNPC(Transform npc)
    {
        NPCDebugInfo info = activeNPCs.Find(n => n.npcTransform == npc);
        if (info != null)
        {
            activeNPCs.Remove(info);
            if (npcsOnFloor.ContainsKey(info.currentFloor))
            {
                npcsOnFloor[info.currentFloor].Remove(npc);
            }
            Debug.Log($"👤 Unregistered NPC: {info.npcName}");
        }
    }

    // And finally, a way to track the player. Add this method.
    public void UpdatePlayerFloor(string newFloor)
    {
        playerCurrentFloor = newFloor;
        Debug.Log($"📍 Player is now on floor: {playerCurrentFloor}");

        // NEW: Also tell the enemy where the player is
        UpdateEnemyPlayerFloor(newFloor);
    }
    public void CancelWarpReservation(Transform npc)
    {
        var reservedWarp = allWarpLocations.FirstOrDefault(w => w.npcCurrentlyUsing == npc);
        if (reservedWarp != null)
        {
            Debug.Log($"❌ {npc.name} cancelled reservation for {reservedWarp.locationName}");

            // 1. Clear the entrance lock
            reservedWarp.npcCurrentlyUsing = null;

            // 2. Clear the destination lock (This was the source of the bug!)
            reservedWarp.npcsAtDestination.Remove(npc);

            // Update manager status
            UpdateNPCStatus(npc, reservedWarp.currentFloor, false);
        }
    }
    public void UpdateEnemyPlayerFloor(string newFloor)
    {
        // Find the enemy script and update its internal floor tracker
        EnemyAI enemy = FindObjectOfType<EnemyAI>();
        if (enemy != null)
        {
            enemy.playerCurrentFloor = newFloor;
        }
    }
    // Register warp locations at start
    public void RegisterWarpLocation(Interactable interactable, string currentFloor, string destFloor)
    {
        if (allWarpLocations.Any(w => w.warpInteractable == interactable))
        {
            Debug.LogWarning($"⚠️ {interactable.name} already registered, skipping...");
            return;
        }

        WarpLocationInfo info = new WarpLocationInfo
        {
            warpInteractable = interactable,
            locationName = interactable.gameObject.name,
            currentFloor = currentFloor,
            destinationFloor = destFloor,
            npcCurrentlyUsing = null,
            npcsAtDestination = new List<Transform>()
        };

        allWarpLocations.Add(info);

        if (!registeredFloors.Contains(currentFloor))
            registeredFloors.Add(currentFloor);
        if (!registeredFloors.Contains(destFloor))
            registeredFloors.Add(destFloor);

        totalWarps = allWarpLocations.Count;

        Debug.Log($"✅ Registered: {info.locationName} | {currentFloor} → {destFloor}");
    }

    // Register NPC for tracking
    public void RegisterNPC(Transform npc, string startFloor)
    {
        NPCDebugInfo info = new NPCDebugInfo
        {
            npcTransform = npc, // Store the transform
            npcName = npc.name,
            currentFloor = startFloor,
            isMovingToWarp = false,
            targetWarpName = "None",
            arrivedViaWarp = "None"
        };

        activeNPCs.Add(info);

        if (!npcsOnFloor.ContainsKey(startFloor))
            npcsOnFloor[startFloor] = new List<Transform>();

        npcsOnFloor[startFloor].Add(npc);

        Debug.Log($"👤 Registered NPC: {npc.name} on {startFloor}");
    }

    // Update NPC floor location
    public void UpdateNPCFloor(Transform npc, string oldFloor, string newFloor)
    {
        // Remove from old floor
        if (npcsOnFloor.ContainsKey(oldFloor))
        {
            npcsOnFloor[oldFloor].Remove(npc);
        }

        // Add to new floor
        if (!npcsOnFloor.ContainsKey(newFloor))
            npcsOnFloor[newFloor] = new List<Transform>();

        npcsOnFloor[newFloor].Add(npc);

        // Update debug info
        NPCDebugInfo info = activeNPCs.Find(n => n.npcName == npc.name);
        if (info != null)
        {
            info.currentFloor = newFloor;
        }

        Debug.Log($"🔄 {npc.name}: {oldFloor} → {newFloor}");
    }

    // Update NPC status
    public void UpdateNPCStatus(Transform npc, string floor, bool movingToWarp, string targetWarp = "", string arrivedVia = "")
    {
        NPCDebugInfo info = activeNPCs.Find(n => n.npcName == npc.name);
        if (info != null)
        {
            info.currentFloor = floor;
            info.isMovingToWarp = movingToWarp;
            info.targetWarpName = movingToWarp ? targetWarp : "None";
            if (!string.IsNullOrEmpty(arrivedVia))
                info.arrivedViaWarp = arrivedVia;
        }
    }

    // Get how many NPCs are on a specific floor
    public int GetNPCCountOnFloor(string floorName)
    {
        if (npcsOnFloor.ContainsKey(floorName))
        {
            return npcsOnFloor[floorName].Count;
        }
        return 0;
    }

    // ✅ Check if an NPC is already at the destination via a specific warp
    private bool IsNPCAtDestination(WarpLocationInfo warp, Transform checkNPC = null)
    {
        // Clean up null references
        warp.npcsAtDestination.RemoveAll(npc => npc == null);
        
        // If checking for specific NPC, see if they're there
        if (checkNPC != null)
        {
            return warp.npcsAtDestination.Contains(checkNPC);
        }
        
        // Otherwise check if anyone is there
        return warp.npcsAtDestination.Count > 0;
    }

    private bool IsFloorOccupiedByOtherNpc(string floorName, Transform requester)
    {
        // Look through all active NPCs.
        // Return 'true' if we find any NPC who is on the target floor AND is not the requester.
        return activeNPCs.Any(npc => npc.currentFloor == floorName && npc.npcTransform != requester);
    }
    // Get available warp locations (checks warp usage AND destination occupancy)
    public List<WarpLocationInfo> GetAvailableWarpsOnFloor(string floorName, Transform requesterNpc)
    {
        var available = allWarpLocations
            .Where(w =>
                w.currentFloor == floorName &&
                (w.npcCurrentlyUsing == null || w.npcCurrentlyUsing == requesterNpc) &&
                !IsNPCAtDestination(w, requesterNpc) &&
                !IsFloorOccupiedByOtherNpc(w.destinationFloor, requesterNpc) // <-- THE KEY CHANGE
                )
            .ToList();

        Debug.Log($"🔍 Floor '{floorName}': Found {available.Count} available warps for {requesterNpc.name}");

        if (available.Count == 0)
        {
            Debug.LogWarning($"⚠️ No available warps for {requesterNpc.name} on floor '{floorName}' (destination likely occupied).");
        }

        return available;
    }

    // Reserve a warp location for an NPC
    public bool ReserveWarpLocation(WarpLocationInfo warpInfo, Transform npc)
    {
        if (warpInfo.npcCurrentlyUsing == null || warpInfo.npcCurrentlyUsing == npc)
        {
            warpInfo.npcCurrentlyUsing = npc;
            Debug.Log($"🔒 {npc.name} reserved {warpInfo.locationName} → {warpInfo.destinationFloor}");
            UpdateNPCStatus(npc, warpInfo.currentFloor, true, warpInfo.locationName);
            return true;
        }

        Debug.LogWarning($"⚠️ {warpInfo.locationName} already reserved by {warpInfo.npcCurrentlyUsing.name}");
        return false;
    }
    public bool IsGuardActive()
    {
        // Checks if the Guard GameObject is referenced AND is currently enabled in the hierarchy
        return guardNpcObject != null && guardNpcObject.activeInHierarchy;
    }

    // ✅ Release a warp location and mark NPC as arrived at destination
    public void ReleaseWarpLocation(WarpLocationInfo warpInfo, Transform npc)
    {
        if (warpInfo != null)
        {
            Debug.Log($"🔓 {npc.name} completed warp via {warpInfo.locationName}");

            // Update floor tracking
            UpdateNPCFloor(npc, warpInfo.currentFloor, warpInfo.destinationFloor);

            // ✅ Mark NPC as being at destination via this warp
            if (!warpInfo.npcsAtDestination.Contains(npc))
            {
                warpInfo.npcsAtDestination.Add(npc);
                Debug.Log($"📍 {npc.name} marked at destination of {warpInfo.locationName}");
            }

            // Clear warp usage
            warpInfo.npcCurrentlyUsing = null;

            UpdateNPCStatus(npc, warpInfo.destinationFloor, false, "", warpInfo.locationName);
        }
    }

    // ✅ Call this when NPC leaves a floor to clear destination tracking
    public void ClearNPCFromDestination(Transform npc, string leavingFloor)
    {
        // Find all warps where this NPC is at destination
        var warpsToUpdate = allWarpLocations.Where(w => 
            w.destinationFloor == leavingFloor && 
            w.npcsAtDestination.Contains(npc)).ToList();

        foreach (var warp in warpsToUpdate)
        {
            warp.npcsAtDestination.Remove(npc);
            Debug.Log($"🚪 {npc.name} cleared from destination of {warp.locationName}");
        }
    }

    // Find a random available warp on current floor
    public WarpLocationInfo GetRandomAvailableWarp(string currentFloor, Transform npc) // Simplified signature
    {
        // Pass the npc's transform as the requester
        var available = GetAvailableWarpsOnFloor(currentFloor, npc);

        if (available.Count > 0)
        {
            var selected = available[Random.Range(0, available.Count)];
            Debug.Log($"🎲 {npc.name} selected: {selected.locationName} (→ {selected.destinationFloor})");
            return selected;
        }

        return null;
    }

    // Debug: Print all warps
    [ContextMenu("Print All Warps")]
    public void PrintAllWarps()
    {
        Debug.Log($"=== WARP LOCATIONS ({totalWarps} total) ===");
        foreach (var warp in allWarpLocations)
        {
            string usingStatus = warp.npcCurrentlyUsing != null 
                ? $"[RESERVED by {warp.npcCurrentlyUsing.name}]" 
                : "[AVAILABLE]";
            
            string destStatus = warp.npcsAtDestination.Count > 0
                ? $"[Dest occupied: {string.Join(", ", warp.npcsAtDestination.Select(n => n.name))}]"
                : "[Dest clear]";
                
            Debug.Log($"{warp.locationName}: {warp.currentFloor} → {warp.destinationFloor} {usingStatus} {destStatus}");
        }

        Debug.Log($"\n=== REGISTERED FLOORS ===");
        foreach (var floor in registeredFloors)
        {
            int count = allWarpLocations.Count(w => w.currentFloor == floor);
            int npcCount = GetNPCCountOnFloor(floor);
            Debug.Log($"{floor}: {count} warps, {npcCount} NPCs");
        }

        Debug.Log($"\n=== ACTIVE NPCs ({activeNPCs.Count}) ===");
        foreach (var npc in activeNPCs)
        {
            string status = npc.isMovingToWarp ? $"→ {npc.targetWarpName}" : "Patrolling";
            string arrival = npc.arrivedViaWarp != "None" ? $"(arrived via {npc.arrivedViaWarp})" : "";
            Debug.Log($"{npc.npcName} on {npc.currentFloor} | {status} {arrival}");
        }
    }
}