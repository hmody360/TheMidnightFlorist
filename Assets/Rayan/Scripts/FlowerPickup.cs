using UnityEngine;


[RequireComponent(typeof(Collider))]
public class FlowerPickup : MonoBehaviour, Iinteractable
{
    // ==================== INTERFACE IMPLEMENTATION ====================
    private string _actionName = "Pick Up Flower";
    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }

    // ==================== SETTINGS ====================
    [Header("=== SETTINGS ===")]
    [Tooltip("Custom action name shown to player")]
    public string customActionName = "Pick Up Flower";

    // ==================== REFERENCES ====================
    [Header("=== REFERENCES (Auto-found if not set) ===")]
    [Tooltip("Outline component for highlight effect")]
    public Outline outlineComponent;

    [Tooltip("Audio manager for sounds")]
    public FlowerAudioManager audioManager;

    // ==================== PRIVATE VARIABLES ====================
    private bool isCollected = false;

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        // Set action name
        ActionName = customActionName;

        // Auto-find Outline if not assigned
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
        }

        // Auto-find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = GetComponent<FlowerAudioManager>();
        }

        // Disable outline by default
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }

    void Start()
    {
        // Verify tag
        if (!gameObject.CompareTag("CollectFlower"))
        {
            Debug.LogWarning("FlowerPickup: GameObject tag is not 'CollectFlower'. RayInteractor won't detect E key for this flower!");
        }
    }

    // ==================== INTERFACE METHOD ====================
    /// <summary>
    /// Called when player interacts with the flower (presses E)
    /// </summary>
    public void Interact()
    {
        // Prevent double collection
        if (isCollected)
        {
            Debug.Log("FlowerPickup: Already collected, ignoring.");
            return;
        }

        isCollected = true;

        // Store position before destroying
        Vector3 flowerPosition = transform.position;

        // Play pickup sound
        if (audioManager != null && audioManager.pickupSound != null)
        {
            // Play sound at position (survives destroy)
            FlowerAudioManager.PlaySoundAtPosition(
                audioManager.pickupSound,
                flowerPosition,
                audioManager.pickupVolume
            );
        }

        // ============================================
        // MONSTER NOTIFICATION (MUST BE BEFORE FlowerSpawnManager!)
        // ============================================
        if (MonsterAI.Instance != null)
        {
            // Check if this is the last flower BEFORE FlowerSpawnManager updates the count
            bool isLastFlower = false;
            if (FlowerSpawnManager.Instance != null)
            {
                int remaining = FlowerSpawnManager.Instance.GetFlowersRemaining();
                // GetFlowersRemaining returns how many are LEFT (including this one)
                // So if it's 1, this is the last one
                isLastFlower = remaining <= 1;

                Debug.Log($"FlowerPickup: GetFlowersRemaining() = {remaining}, IsLastFlower = {isLastFlower}");
            }

            MonsterAI.Instance.OnFlowerCollected(flowerPosition, isLastFlower);
        }
        else
        {
            Debug.LogWarning("FlowerPickup: MonsterAI.Instance not found!");
        }

        // ============================================
        // FLOWER SPAWN MANAGER (AFTER Monster notification!)
        // This updates the flower count
        // ============================================
        if (FlowerSpawnManager.Instance != null)
        {
            FlowerSpawnManager.Instance.OnFlowerCollected(gameObject, flowerPosition);
        }
        else
        {
            Debug.LogWarning("FlowerPickup: FlowerSpawnManager.Instance not found!");
        }

        Debug.Log("FlowerPickup: Flower collected at " + flowerPosition);

        // Destroy the flower
        Destroy(gameObject);
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Collect This Flower")]
    public void TestCollect()
    {
        Interact();
    }
}