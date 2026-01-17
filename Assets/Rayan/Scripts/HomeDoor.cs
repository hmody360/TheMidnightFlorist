using UnityEngine;

// ================================================================================
// HOME DOOR - Win Condition Trigger
// ================================================================================
// An interactable door that lets the player win when all flowers are collected.
// Uses the Iinteractable interface to work with RayInteractor.
//
// SETUP:
// 1. Add this script to your door GameObject
// 2. Set the GameObject layer to 6 (Interactible)
// 3. Add an Outline component for highlight effect
// 4. Add a Collider component (for raycast detection)
//
// BEHAVIOR:
// - Player looks at door ? ActionName shows based on flower status
// - All flowers collected ? Shows "readyActionName" ? Click to win
// - Flowers remaining ? Shows "notReadyActionName" ? Click plays error sound
// - Night not started ? Shows "nightNotStartedActionName" ? Click plays error sound
// ================================================================================

[RequireComponent(typeof(Collider))]
public class HomeDoor : MonoBehaviour, Iinteractable
{
    // ==================== INTERFACE IMPLEMENTATION ====================
    private string _actionName = "Go Home";
    public string ActionName
    {
        get
        {
            // Check if night has started
            if (!IsNightRunning())
            {
                return nightNotStartedActionName;
            }

            // Change prompt based on whether all flowers are collected
            if (AreAllFlowersCollected())
            {
                return readyActionName;
            }
            else
            {
                return notReadyActionName;
            }
        }
        set { _actionName = value; }
    }

    // ==================== SETTINGS ====================
    [Header("=== ACTION NAMES (Shown as UI Prompt) ===")]
    [Tooltip("Prompt shown when all flowers are collected")]
    public string readyActionName = "Go In";

    [Tooltip("Prompt shown when flowers are still remaining")]
    public string notReadyActionName = "You need to collect all flowers";

    [Tooltip("Prompt shown when night hasn't started yet")]
    public string nightNotStartedActionName = "Collect all flowers";

    [Header("=== PROMPT COLORS ===")]
    [Tooltip("Color for ready prompt (all flowers collected)")]
    public Color readyColor = Color.green;

    [Tooltip("Color for not ready prompt (flowers remaining)")]
    public Color notReadyColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Dark red

    [Tooltip("Color for night not started prompt")]
    public Color nightNotStartedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray

    [Header("=== AUDIO (Optional) ===")]
    [Tooltip("Sound played when trying to enter without all flowers")]
    public AudioClip errorSound;

    [Tooltip("Sound played when successfully entering (winning)")]
    public AudioClip successSound;

    [Tooltip("Volume for sounds")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("=== REFERENCES (Auto-found if not set) ===")]
    [Tooltip("Outline component for highlight effect")]
    public Outline outlineComponent;

    [Tooltip("RotatableObject component for door animation (from Seagull asset)")]
    public Seagull.Interior_I1.SceneProps.RotatableObject rotatableObject;

    [Tooltip("Rotatable component (the actual door rotation - from Seagull asset)")]
    public Seagull.Interior_I1.SceneProps.Rotatable rotatable;

    [Header("=== DOOR ANIMATION ===")]
    [Tooltip("Should the door animate open when player wins?")]
    public bool animateDoorOnWin = true;

    [Tooltip("How much to open the door (0 = fully open, 1 = fully closed). Try 0.7 for slightly open.")]
    [Range(0f, 1f)]
    public float doorOpenAmount = 0.7f;

    [Tooltip("Delay before showing Win UI (seconds) - lets player see door open")]
    public float winUIDelay = 1f;

    [Header("=== DEBUG ===")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private bool hasTriggeredWin = false;

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        // Auto-find Outline if not assigned
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
        }

        // Disable outline by default (RayInteractor will enable it)
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }

        // Auto-find RotatableObject if not assigned
        if (rotatableObject == null)
        {
            rotatableObject = GetComponent<Seagull.Interior_I1.SceneProps.RotatableObject>();
        }

        // Auto-find Rotatable if not assigned
        if (rotatable == null)
        {
            rotatable = GetComponent<Seagull.Interior_I1.SceneProps.Rotatable>();
        }
    }

    void Start()
    {
        // Verify layer
        if (gameObject.layer != 6)
        {
            Debug.LogWarning("HomeDoor: GameObject layer is not 6 (Interactible). RayInteractor won't detect this door!");
        }

        // Verify collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("HomeDoor: No Collider found! Add a Collider component for raycast detection.");
        }

        // Make sure door starts closed (call with small delay to override RotatableObject)
        StartCoroutine(CloseDoorOnStart());

        if (showDebugLogs)
        {
            Debug.Log("HomeDoor: Initialized and ready.");
        }
    }

    /// <summary>
    /// Closes the door after a tiny delay to ensure it overrides RotatableObject's default
    /// </summary>
    private System.Collections.IEnumerator CloseDoorOnStart()
    {
        // Wait for Rotatable's first FixedUpdate to run (it sets lastRotation = rotation)
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Force the door to close by changing value first (triggers the rotation)
        // NOTE: For this door, rotation=1 is CLOSED, rotation=0 is OPEN
        if (rotatable != null)
        {
            // Set to slightly different value first to trigger change detection
            rotatable.rotation = 0.99f;

            // Wait for FixedUpdate to process the change
            yield return new WaitForFixedUpdate();

            // Now set to 1 (closed)
            rotatable.rotation = 1f;

            if (showDebugLogs)
            {
                Debug.Log("HomeDoor: Door set to closed position (rotation=1).");
            }
        }
        else if (rotatableObject != null)
        {
            // Fallback to RotatableObject
            rotatableObject.rotate(0.99f);
            yield return new WaitForFixedUpdate();
            rotatableObject.rotate(1f);
        }
    }

    // ==================== INTERFACE METHOD ====================
    /// <summary>
    /// Called when player interacts with the door (clicks)
    /// </summary>
    public void Interact()
    {
        // Prevent double trigger
        if (hasTriggeredWin)
        {
            if (showDebugLogs)
            {
                Debug.Log("HomeDoor: Win already triggered, ignoring.");
            }
            return;
        }

        // Check if night has started
        if (!IsNightRunning())
        {
            OnNightNotStarted();
            return;
        }

        // Check if all flowers are collected
        if (AreAllFlowersCollected())
        {
            // SUCCESS - Player wins!
            OnPlayerWins();
        }
        else
        {
            // NOT READY - Play error sound
            OnNotReady();
        }
    }

    // ==================== PROMPT COLOR ====================
    /// <summary>
    /// Gets the current prompt color based on game state
    /// Can be used by RayInteractor to color the prompt text
    /// </summary>
    public Color GetPromptColor()
    {
        if (!IsNightRunning())
        {
            return nightNotStartedColor;
        }

        if (AreAllFlowersCollected())
        {
            return readyColor;
        }

        return notReadyColor;
    }

    // ==================== WIN CONDITION ====================
    /// <summary>
    /// Checks if the night is currently running
    /// </summary>
    private bool IsNightRunning()
    {
        if (NightGameManager.Instance != null)
        {
            return NightGameManager.Instance.IsNightRunning();
        }
        return false;
    }

    /// <summary>
    /// Checks if all flowers have been collected
    /// </summary>
    private bool AreAllFlowersCollected()
    {
        // First check if night is running - if not, can't have collected flowers
        if (!IsNightRunning())
        {
            return false;
        }

        // Check FlowerSpawnManager first
        if (FlowerSpawnManager.Instance != null)
        {
            return FlowerSpawnManager.Instance.AreAllFlowersCollected();
        }

        // Fallback: Check NightGameManager
        if (NightGameManager.Instance != null)
        {
            return NightGameManager.Instance.AreAllFlowersCollected();
        }

        // If neither exists, log warning and return false
        Debug.LogWarning("HomeDoor: Neither FlowerSpawnManager nor NightGameManager found!");
        return false;
    }

    /// <summary>
    /// Called when player tries to enter before starting the night
    /// </summary>
    private void OnNightNotStarted()
    {
        if (showDebugLogs)
        {
            Debug.Log("HomeDoor: Night hasn't started! Player needs to enter the maze first.");
        }

        // Play error sound
        if (errorSound != null)
        {
            AudioSource.PlayClipAtPoint(errorSound, transform.position, soundVolume);
        }
    }

    /// <summary>
    /// Called when player successfully wins (all flowers collected)
    /// </summary>
    private void OnPlayerWins()
    {
        hasTriggeredWin = true;

        if (showDebugLogs)
        {
            Debug.Log("HomeDoor: All flowers collected! Player wins!");
        }

        // Play success sound
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position, soundVolume);
        }

        // Animate door opening
        if (animateDoorOnWin)
        {
            if (rotatable != null)
            {
                rotatable.rotation = doorOpenAmount;

                if (showDebugLogs)
                {
                    Debug.Log($"HomeDoor: Door opened to {doorOpenAmount}");
                }
            }
            else if (rotatableObject != null)
            {
                rotatableObject.rotate(doorOpenAmount);

                if (showDebugLogs)
                {
                    Debug.Log($"HomeDoor: Door opened to {doorOpenAmount}");
                }
            }

            // Wait for delay, then trigger win UI
            StartCoroutine(TriggerWinAfterDelay());
        }
        else
        {
            // No animation, trigger win immediately
            TriggerWinNow();
        }
    }

    /// <summary>
    /// Waits for delay then triggers win UI
    /// </summary>
    private System.Collections.IEnumerator TriggerWinAfterDelay()
    {
        yield return new WaitForSeconds(winUIDelay);
        TriggerWinNow();
    }

    /// <summary>
    /// Triggers win in NightGameManager
    /// </summary>
    private void TriggerWinNow()
    {
        if (NightGameManager.Instance != null)
        {
            NightGameManager.Instance.TriggerWin();
        }
        else
        {
            Debug.LogError("HomeDoor: NightGameManager.Instance not found! Cannot trigger win.");
        }
    }

    /// <summary>
    /// Called when player tries to enter but hasn't collected all flowers
    /// </summary>
    private void OnNotReady()
    {
        if (showDebugLogs)
        {
            int remaining = GetFlowersRemaining();
            Debug.Log($"HomeDoor: Not all flowers collected! {remaining} remaining.");
        }

        // Play error sound
        if (errorSound != null)
        {
            AudioSource.PlayClipAtPoint(errorSound, transform.position, soundVolume);
        }
    }

    // ==================== HELPER METHODS ====================
    /// <summary>
    /// Gets how many flowers are remaining
    /// </summary>
    private int GetFlowersRemaining()
    {
        if (FlowerSpawnManager.Instance != null)
        {
            return FlowerSpawnManager.Instance.GetFlowersRemaining();
        }

        return -1; // Unknown
    }

    /// <summary>
    /// Resets the door (for restarting the night)
    /// </summary>
    public void ResetDoor()
    {
        hasTriggeredWin = false;

        if (showDebugLogs)
        {
            Debug.Log("HomeDoor: Door reset.");
        }
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Interact")]
    public void TestInteract()
    {
        Interact();
    }

    [ContextMenu("Test: Reset Door")]
    public void TestResetDoor()
    {
        ResetDoor();
    }

    [ContextMenu("Debug: Check Flower Status")]
    public void DebugCheckFlowerStatus()
    {
        bool nightRunning = IsNightRunning();
        bool allCollected = AreAllFlowersCollected();
        int remaining = GetFlowersRemaining();

        Debug.Log("===== HOME DOOR STATUS =====");
        Debug.Log($"Night Running: {nightRunning}");
        Debug.Log($"All Flowers Collected: {allCollected}");
        Debug.Log($"Flowers Remaining: {remaining}");
        Debug.Log($"Current Action Name: {ActionName}");
        Debug.Log($"Has Triggered Win: {hasTriggeredWin}");
        Debug.Log("============================");
    }
}