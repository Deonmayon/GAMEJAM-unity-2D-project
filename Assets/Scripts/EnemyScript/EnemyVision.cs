using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public Transform player;
    public float detectionRange = 6f;
    public LayerMask obstacleMask; // walls or objects blocking view

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectionRange) return false;

        // Raycast to detect if wall blocks the view
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleMask);
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
            return false;

        Debug.DrawRay(transform.position, directionToPlayer * distance, Color.red);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}