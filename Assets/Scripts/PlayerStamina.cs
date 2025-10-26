using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    private float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float regenDelay = 1f;
    public float minStaminaToRun = 10f;

    private float regenTimer;
    private bool canRun = true;
    private PlayerMovement playerMovement;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => currentStamina / maxStamina;
    public bool CanRun => canRun;

    void Start()
    {
        currentStamina = maxStamina;
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("PlayerStamina not found playerMovement component!");
        }
    }

    void Update()
    {
        if (playerMovement == null) return;

        bool isRunningAndMoving = playerMovement.IsRunning && playerMovement.IsMoving;

        if (isRunningAndMoving && canRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);

            regenTimer = 0f;

            if (currentStamina <= 0)
            {
                canRun = false;
            }
        }
        else
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);

                if (currentStamina >= minStaminaToRun)
                {
                    canRun = true;
                }
            }
        }
    }
}