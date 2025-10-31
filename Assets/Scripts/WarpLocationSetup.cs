using UnityEngine;

public class WarpLocationSetup : MonoBehaviour
{
    [Header("Warp Info")]
    public string currentFloor = "Floor1";
    public string destinationFloor = "Floor2";

    private Interactable interactable;

    void Start()
    {
        interactable = GetComponent<Interactable>();

        if (interactable != null &&
            (interactable.type == InteractionType.Door ||
             interactable.type == InteractionType.Stair))
        {
            // Register this warp location
            WarpLocationManager.Instance.RegisterWarpLocation(
                interactable,
                currentFloor,
                destinationFloor
            );
        }
    }
}