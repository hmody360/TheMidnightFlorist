using UnityEngine;

// ================================================================================
// MAZE ENTRANCE - Trigger to Start the Night
// ================================================================================
// Place this script on a trigger collider at the entrance of the maze.
// When the player walks into the trigger, it calls NightGameManager.StartNight()
//
// SETUP:
// 1. Create an empty GameObject at the maze entrance
// 2. Add a Collider component (Box Collider recommended)
// 3. Check "Is Trigger" on the Collider
// 4. Add this script to the GameObject
// 5. Set the Player Tag (default is "Player")
//
// OPTIONAL:
// - Assign an AudioSource and clip to play an entrance sound
// - Enable "Disable After Trigger" to prevent re-triggering
// - Enable "Show Debug Gizmos" to see the trigger area in editor
// ================================================================================

[RequireComponent(typeof(Collider))]
public class MazeEntrance : MonoBehaviour
{
    // ==================== SETTINGS ====================
    [Header("=== PLAYER DETECTION ===")]
    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    [Header("=== TRIGGER BEHAVIOR ===")]
    [Tooltip("Disable this trigger after it's been activated once")]
    public bool disableAfterTrigger = true;

    [Tooltip("Delay before starting the night (in seconds)")]
    public float startDelay = 0f;

    [Header("=== AUDIO (Optional) ===")]
    [Tooltip("Sound to play when entering the maze")]
    public AudioClip entranceSound;

    [Tooltip("Volume of the entrance sound")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("=== DEBUG ===")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugLogs = true;

    [Tooltip("Show trigger area gizmo in editor")]
    public bool showDebugGizmos = true;

    [Tooltip("Gizmo color")]
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.3f); // Green with transparency

    // ==================== PRIVATE VARIABLES ====================
    private bool hasTriggered = false;
    private Collider triggerCollider;

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        // Get and validate collider
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider == null)
        {
            Debug.LogError("MazeEntrance: No Collider found! Add a Collider component.");
            return;
        }

        // Make sure it's a trigger
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("MazeEntrance: Collider is not set as trigger. Setting it now.");
            triggerCollider.isTrigger = true;
        }
    }

    void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log("MazeEntrance: Ready and waiting for player.");
        }
    }

    // ==================== TRIGGER DETECTION ====================
    void OnTriggerEnter(Collider other)
    {
        // Check if already triggered
        if (hasTriggered && disableAfterTrigger)
        {
            return;
        }

        // Check if it's the player
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log("MazeEntrance: Player entered the maze!");
        }

        // Mark as triggered
        hasTriggered = true;

        // Play entrance sound
        PlayEntranceSound();

        // Start the night (with optional delay)
        if (startDelay > 0f)
        {
            Invoke(nameof(TriggerStartNight), startDelay);
        }
        else
        {
            TriggerStartNight();
        }

        // Disable the trigger if needed
        if (disableAfterTrigger)
        {
            // Disable the collider so it won't trigger again
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }

    // ==================== START NIGHT ====================
    /// <summary>
    /// Calls NightGameManager to start the night
    /// </summary>
    private void TriggerStartNight()
    {
        if (NightGameManager.Instance != null)
        {
            NightGameManager.Instance.StartNight();

            if (showDebugLogs)
            {
                Debug.Log("MazeEntrance: NightGameManager.StartNight() called successfully!");
            }
        }
        else
        {
            Debug.LogError("MazeEntrance: NightGameManager.Instance not found! Make sure NightGameManager exists in the scene.");
        }
    }

    // ==================== AUDIO ====================
    /// <summary>
    /// Plays the entrance sound effect
    /// </summary>
    private void PlayEntranceSound()
    {
        if (entranceSound != null)
        {
            AudioSource.PlayClipAtPoint(entranceSound, transform.position, soundVolume);

            if (showDebugLogs)
            {
                Debug.Log("MazeEntrance: Entrance sound played.");
            }
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Resets the trigger so it can be activated again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        if (showDebugLogs)
        {
            Debug.Log("MazeEntrance: Trigger reset.");
        }
    }

    /// <summary>
    /// Manually trigger the maze entrance (for testing or cutscenes)
    /// </summary>
    public void ManualTrigger()
    {
        if (hasTriggered && disableAfterTrigger)
        {
            Debug.LogWarning("MazeEntrance: Already triggered!");
            return;
        }

        hasTriggered = true;
        PlayEntranceSound();
        TriggerStartNight();

        if (disableAfterTrigger && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    // ==================== EDITOR GIZMOS ====================
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = gizmoColor;

        // Draw based on collider type
        if (col is BoxCollider box)
        {
            // Draw filled box
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);

            // Draw wireframe
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider capsule)
        {
            // Simple representation for capsule
            Gizmos.DrawSphere(transform.position, capsule.radius);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw an arrow pointing into the maze
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = transform.position + transform.forward * 2f;
        Gizmos.DrawLine(start, end);

        // Arrow head
        Vector3 right = transform.position + transform.forward * 1.5f + transform.right * 0.3f;
        Vector3 left = transform.position + transform.forward * 1.5f - transform.right * 0.3f;
        Gizmos.DrawLine(end, right);
        Gizmos.DrawLine(end, left);
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Manual Trigger")]
    public void TestManualTrigger()
    {
        ManualTrigger();
    }

    [ContextMenu("Test: Reset Trigger")]
    public void TestResetTrigger()
    {
        ResetTrigger();
    }
}