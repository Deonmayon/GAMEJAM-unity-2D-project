using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public float detectionRange = 10f;
    public LayerMask obstacleMask;
    public Transform eyes;
    public bool debugVision = true;

    private void Start()
    {
        if (eyes == null)
            eyes = this.transform;
    }

    public bool CanSeeEnemy(Transform enemy)
    {
        if (enemy == null) return false;

        Vector2 directionToEnemy = (enemy.position - eyes.position).normalized;
        float distance = Vector2.Distance(eyes.position, enemy.position);

        // Only check distance (360° detection)
        if (distance > detectionRange) return false;

        // Raycast to detect if wall blocks the view
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToEnemy, distance, obstacleMask);
        if (hit.collider != null && hit.collider.transform != enemy)
            return false;

        // Debug ray
        Debug.DrawRay(eyes.position, directionToEnemy * distance, Color.green);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugVision || eyes == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, detectionRange);
    }
}