using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPCs to Manage")]
    public Transform enemy; // The stalker
    public Transform guard; // The guard

    [Header("Available Zones")]
    public FloorSpawnZone[] allZones; // Drag all Floor zones here

    [Header("Respawn Settings")]
    public float checkInterval = 10f; // How often to check if should respawn (seconds)

    private float timer = 0f;

    void Start()
    {
        // Initial spawn - put them in different zones
        SpawnInitial();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckAndRespawn();
        }
    }

    // Initial spawn at game start
    void SpawnInitial()
    {
        if (allZones.Length < 2)
        {
            Debug.LogError("Need at least 2 zones!");
            return;
        }

        // Spawn enemy in first zone
        FloorSpawnZone enemyZone = allZones[0];
        SpawnNPCInZone(enemy, enemyZone);

        // Spawn guard in second zone (different from enemy)
        FloorSpawnZone guardZone = allZones[1];
        SpawnNPCInZone(guard, guardZone);
    }

    // Check if NPCs should be respawned
    void CheckAndRespawn()
    {
        // Get current zones of each NPC
        FloorSpawnZone enemyCurrentZone = GetNPCCurrentZone(enemy);
        FloorSpawnZone guardCurrentZone = GetNPCCurrentZone(guard);

        // If they're in the same zone, respawn one of them
        if (enemyCurrentZone == guardCurrentZone)
        {
            Debug.Log("⚠️ Both NPCs in same zone! Respawning guard...");
            RespawnToNewZone(guard, guardCurrentZone);
        }
        else
        {
            // Random chance to respawn each NPC to a new zone
            if (Random.value < 0.3f) // 30% chance
            {
                RespawnToNewZone(enemy, enemyCurrentZone);
            }

            if (Random.value < 0.3f) // 30% chance
            {
                RespawnToNewZone(guard, guardCurrentZone);
            }
        }
    }

    // Find which zone an NPC is currently in
    FloorSpawnZone GetNPCCurrentZone(Transform npc)
    {
        foreach (var zone in allZones)
        {
            if (zone.npcsInZone.Contains(npc))
            {
                return zone;
            }
        }

        return null;
    }

    // Respawn NPC to a different zone
    void RespawnToNewZone(Transform npc, FloorSpawnZone currentZone)
    {
        // Find available zones (not current, not occupied by other NPC)
        List<FloorSpawnZone> availableZones = new List<FloorSpawnZone>();

        Transform otherNPC = (npc == enemy) ? guard : enemy;
        FloorSpawnZone otherZone = GetNPCCurrentZone(otherNPC);

        foreach (var zone in allZones)
        {
            // Skip current zone and zone with other NPC
            if (zone == currentZone || zone == otherZone)
                continue;

            availableZones.Add(zone);
        }

        if (availableZones.Count == 0)
        {
            Debug.LogWarning($"No available zones for {npc.name}");
            return;
        }

        // Pick random available zone
        FloorSpawnZone newZone = availableZones[Random.Range(0, availableZones.Count)];

        // Remove from old zone
        if (currentZone != null)
        {
            currentZone.RemoveNPC(npc);
        }

        // Spawn in new zone
        SpawnNPCInZone(npc, newZone);

        Debug.Log($"🔄 Respawned {npc.name} to {newZone.zoneName}");
    }

    void SpawnNPCInZone(Transform npc, FloorSpawnZone zone)
    {
        Transform spawnPoint = zone.GetAvailableSpawnPoint();

        if (spawnPoint != null)
        {
            npc.position = spawnPoint.position;
            zone.AddNPC(npc);
            Debug.Log($"✅ Spawned {npc.name} at {zone.zoneName}");
        }
    }

    void OnGUI()
    {
        if (enemy != null && guard != null)
        {
            FloorSpawnZone enemyZone = GetNPCCurrentZone(enemy);
            FloorSpawnZone guardZone = GetNPCCurrentZone(guard);

            GUI.Label(new Rect(10, 10, 300, 20), $"Enemy: {(enemyZone != null ? enemyZone.zoneName : "Unknown")}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Guard: {(guardZone != null ? guardZone.zoneName : "Unknown")}");
            GUI.Label(new Rect(10, 50, 300, 20), $"Next check: {checkInterval - timer:F1}s");
        }
    }
}