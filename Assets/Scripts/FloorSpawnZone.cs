using UnityEngine;
using System.Collections.Generic;

public class FloorSpawnZone : MonoBehaviour
{
    [Header("Zone Info")]
    public string zoneName = "Floor1";

    [Header("Spawn Points")]
    public Transform[] spawnPoints; 

    [HideInInspector]
    public List<Transform> npcsInZone = new List<Transform>();

    // Get a random spawn point that's not occupied
    public Transform GetAvailableSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError($"No spawn points in {zoneName}!");
            return null;
        }

        // Shuffle and find first available
        List<Transform> shuffled = new List<Transform>(spawnPoints);

        for (int i = 0; i < shuffled.Count; i++)
        {
            Transform temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        return shuffled[0];
    }

    public bool IsOccupied()
    {
        // Clean up null references
        npcsInZone.RemoveAll(npc => npc == null);
        return npcsInZone.Count > 0;
    }

    // Add NPC to this zone
    public void AddNPC(Transform npc)
    {
        if (!npcsInZone.Contains(npc))
        {
            npcsInZone.Add(npc);
            Debug.Log($"✅ {npc.name} entered {zoneName}");
        }
    }

    // Remove NPC from this zone
    public void RemoveNPC(Transform npc)
    {
        if (npcsInZone.Contains(npc))
        {
            npcsInZone.Remove(npc);
            Debug.Log($"🚪 {npc.name} left {zoneName}");
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = IsOccupied() ? Color.red : Color.green;

            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}