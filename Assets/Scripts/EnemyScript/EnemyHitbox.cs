using UnityEngine;
public class EnemyHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched enemy hitbox!");
            // TODO: call player damage/death method here
        }
    }
}
