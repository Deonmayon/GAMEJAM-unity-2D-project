using UnityEngine;
using System.Collections;

public class NpcController : MonoBehaviour
{
    private bool isMoving = false;

    public void StartMovement(NpcMovementData movementData)
    {
        if (isMoving) return;
        StartCoroutine(MoveToDestination(movementData));
    }

    private IEnumerator MoveToDestination(NpcMovementData data)
    {
        isMoving = true;

        if (data.destinationTransform == null)
        {
            Debug.LogWarning("[NpcController] No destination set!", this);
            isMoving = false;
            yield break;
        }

        // เดินไปยังจุดหมาย
        while (Vector2.Distance(transform.position, data.destinationTransform.position) > data.arrivalDistance)
        {
            Vector2 direction = (data.destinationTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(
                transform.position,
                data.destinationTransform.position,
                data.moveSpeed * Time.deltaTime
            );

            // หมุนหน้า NPC ไปทางที่เดิน (optional)
            if (direction.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
                transform.localScale = scale;
            }

            yield return null;
        }

        // ถึงจุดหมายแล้ว
        Debug.Log($"[NpcController] {gameObject.name} reached destination!");

        // ถ้าตั้งค่าให้หายไป
        if (data.disappearOnArrival)
        {
            yield return new WaitForSeconds(data.disappearDelay);

            // Fade out หรือ disable ทันที
            // ถ้ามี SpriteRenderer สามารถทำ fade effect ได้
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float fadeTime = 0.5f;
                float elapsed = 0f;
                Color originalColor = sr.color;

                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }
            }

            gameObject.SetActive(false);
            Debug.Log($"[NpcController] {gameObject.name} disappeared!");
        }

        isMoving = false;
    }
}
