using System.Collections;
using UnityEngine;

public class PlayerSleep : MonoBehaviour
{
    [Header("Sleep Configuration")]
    [SerializeField] private float maxSleepAmount = 100f;  // Maximum sleep bar value.
    [SerializeField] private float baseSleepDecreaseRate = 1f; // Base decrease rate of sleep.
    [SerializeField] private float sleepDecreaseInterval = 1f; // Time interval for sleep decrease.
    [SerializeField] private float sleepRecoveryRate = 5f; // Recovery rate during sleep.

    [Header("Events")]
    [SerializeField] private CustomEvent onPlayerFallAsleep; // Triggered when sleep reaches 0.
    [SerializeField] private CustomEvent onPlayerWakeUp; // Triggered when sleep is full.
    [SerializeField] private CustomEvent_Float onSleepUpdated; // Triggered when sleep value changes.

    [Header("Player Movement Control")]
    [SerializeField] private PlayerController playerMovementScript; // Reference to the player's movement script.

    private Coroutine sleepCoroutine;
    private bool isSleepingForced = false; // If the player is sleeping due to exhaustion.
    private float currentSleepAmount;
    private float dynamicSleepDecreaseRate; // The current decrease rate influenced by player actions.

    public float CurrentSleepAmount
    {
        get => currentSleepAmount;
        private set
        {
            currentSleepAmount = Mathf.Clamp(value, 0, maxSleepAmount);
            onSleepUpdated?.Raise(currentSleepAmount);

            // Handle falling asleep when sleep reaches 0
            if (currentSleepAmount <= 0 && !isSleepingForced)
            {
                ForceSleep();
            }
        }
    }

    private void Awake()
    {
        CurrentSleepAmount = maxSleepAmount; // Start fully rested.
        dynamicSleepDecreaseRate = baseSleepDecreaseRate; // Initialize to the base rate.
    }

    private void Start()
    {
        // Start the sleep depletion routine.
        sleepCoroutine = StartCoroutine(SleepDepletionCoroutine());

        // Test ForceSleep
        ForceSleep();
    }

    private void OnDestroy()
    {
        if (sleepCoroutine != null)
        {
            StopCoroutine(sleepCoroutine);
        }
    }

    /// <summary>
    /// Coroutine to gradually reduce sleep over time.
    /// </summary>
    private IEnumerator SleepDepletionCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(sleepDecreaseInterval);
        while (true)
        {
            if (!isSleepingForced)
            {
                CurrentSleepAmount -= dynamicSleepDecreaseRate;
            }
            yield return wait;
        }
    }

    /// <summary>
    /// Triggered when the player falls asleep involuntarily.
    /// </summary>
    private void ForceSleep()
    {
        if (isSleepingForced) return;

        isSleepingForced = true;
        onPlayerFallAsleep?.Raise();
        DisablePlayerMovement();

        StartCoroutine(SleepRecoveryCoroutine(() =>
        {
            EnablePlayerMovement();
            isSleepingForced = false;
            onPlayerWakeUp?.Raise();
        }));
    }

    /// <summary>
    /// Coroutine to recover sleep over time while the player is asleep.
    /// </summary>
    private IEnumerator SleepRecoveryCoroutine(System.Action onComplete)
    {
        while (CurrentSleepAmount < maxSleepAmount)
        {
            CurrentSleepAmount += sleepRecoveryRate * Time.deltaTime;
            yield return null;
        }

        // Ensure the bar is full.
        CurrentSleepAmount = maxSleepAmount;

        // Invoke the callback when recovery is complete.
        onComplete?.Invoke();
    }

    /// <summary>
    /// Manually trigger sleep (e.g., when the player chooses to rest).
    /// </summary>
    public void SleepManually()
    {
        if (isSleepingForced) return;

        StopCoroutine(sleepCoroutine); // Stop natural sleep depletion.

        StartCoroutine(SleepRecoveryCoroutine(() =>
        {
            // Resume natural depletion after resting.
            sleepCoroutine = StartCoroutine(SleepDepletionCoroutine());
        }));
    }

    /// <summary>
    /// Reset sleep to maximum value (e.g., after a specific event).
    /// </summary>
    public void ResetSleep()
    {
        CurrentSleepAmount = maxSleepAmount;
    }

    /// <summary>
    /// Set the current sleep amount to a specific value.
    /// </summary>
    public void SetSleep(float value)
    {
        CurrentSleepAmount = value;
    }

    /// <summary>
    /// Adjust the sleep decrease rate dynamically based on player actions.
    /// </summary>
    /// <param name="newRate">The new decrease rate to apply.</param>
    public void SetSleepDecreaseRate(float newRate)
    {
        dynamicSleepDecreaseRate = Mathf.Max(0, newRate); // Ensure the rate is non-negative.
    }

    /// <summary>
    /// Reset the sleep decrease rate to its base value.
    /// </summary>
    public void ResetSleepDecreaseRate()
    {
        dynamicSleepDecreaseRate = baseSleepDecreaseRate;
    }

    /// <summary>
    /// Disables player movement during sleep.
    /// </summary>
    private void DisablePlayerMovement()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
    }

    /// <summary>
    /// Enables player movement after waking up.
    /// </summary>
    private void EnablePlayerMovement()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
    }
}
