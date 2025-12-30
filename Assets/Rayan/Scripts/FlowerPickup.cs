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

        // Notify the spawn manager
        if (FlowerSpawnManager.Instance != null)
        {
            FlowerSpawnManager.Instance.OnFlowerCollected(gameObject, flowerPosition);
        }
        else
        {
            Debug.LogWarning("FlowerPickup: FlowerSpawnManager.Instance not found!");
        }

        // ============================================
        // TODO: MONSTER NOTIFICATION POINT
        // MonsterAI.Instance.InvestigatePosition(flowerPosition);
        // ============================================

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