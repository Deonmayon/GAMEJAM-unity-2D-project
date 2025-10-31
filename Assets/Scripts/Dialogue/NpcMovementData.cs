using UnityEngine;

[System.Serializable] // ทำให้มันโชว์ใน Inspector ได้
public class NpcMovementData
{
    public Transform npcTransform;
    public Transform destinationTransform;
    public float moveSpeed;
    public float arrivalDistance;
    public bool disappearOnArrival;
    public float disappearDelay;
}