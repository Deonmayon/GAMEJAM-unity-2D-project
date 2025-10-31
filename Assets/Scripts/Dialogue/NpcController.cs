using System.Collections;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    // ฟังก์ชันนี้ DialogueUI จะเรียกใช้
    public void StartMovement(NpcMovementData data)
    {
        // (เราลบโค้ด rb.Kinematic และ col.isTrigger ที่ซ้ำซ้อนออกจากตรงนี้)

        // เริ่ม Coroutine เพื่อทำการเคลื่อนที่
        StartCoroutine(MoveToDestination(data));
    }

    IEnumerator MoveToDestination(NpcMovementData data)
    {
        if (data == null || data.npcTransform == null || data.destinationTransform == null)
        {
            Debug.LogError("NpcMovementData ไม่สมบูรณ์!");
            yield break;
        }

        Debug.Log("NPC " + data.npcTransform.name + " กำลังเดินไปที่ " + data.destinationTransform.name);

        // --- 1. ดึง Components จาก 'data.npcTransform' (ตัว NPC จริงๆ) ---
        Animator anim = data.npcTransform.GetComponent<Animator>();
        SpriteRenderer sr = data.npcTransform.GetComponent<SpriteRenderer>(); // (แคชไว้)
        Rigidbody2D rb = data.npcTransform.GetComponent<Rigidbody2D>(); // (เปลี่ยนจาก InChildren)
        Collider2D col = data.npcTransform.GetComponent<Collider2D>(); // (เปลี่ยนจาก InChildren)

        // --- 2. ปิดฟิสิกส์/Collision (เหมือนที่คุณต้องการ) ---
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero; // (แก้ linearVelocity เป็น velocity)
        }
        if (col != null)
        {
            col.isTrigger = true;
        }

        // --- 3. สั่ง Animator ให้เดิน ---
        if (anim != null)
        {
            anim.SetFloat("Speed", data.moveSpeed);
        }

        // --- 4. ลูปการเดิน ---
        while (Vector2.Distance(data.npcTransform.position, data.destinationTransform.position) > data.arrivalDistance)
        {
            // (แก้ไข Flip Logic)
            if (sr != null)
            {
                // ถ้าเป้าหมายอยู่ทางซ้าย (x น้อยกว่า)
                if (data.destinationTransform.position.x < data.npcTransform.position.x)
                {
                    sr.flipX = false; // ให้หันซ้าย
                }
                // ถ้าเป้าหมายอยู่ทางขวา (x มากกว่า)
                else if (data.destinationTransform.position.x > data.npcTransform.position.x)
                {
                    sr.flipX = true; // ให้หันขวา
                }
            }

            data.npcTransform.position = Vector2.MoveTowards(
                data.npcTransform.position,
                data.destinationTransform.position,
                data.moveSpeed * Time.deltaTime
            );
            yield return null; // รอเฟรมถัดไป
        }

        Debug.Log("NPC ถึงที่หมายแล้ว");

        // --- 5. หยุด Animator ---
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }

        // --- 6. หายตัว (ถ้าตั้งค่าไว้) ---
        if (data.disappearOnArrival)
        {
            yield return new WaitForSeconds(data.disappearDelay);
            Debug.Log("NPC หายตัว");
            data.npcTransform.gameObject.SetActive(false);
        }
    }
}