using UnityEngine;
using System.Collections.Generic;

public class FlowerSpawnManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static FlowerSpawnManager Instance { get; private set; }

    // ==================== REFERENCES ====================
    [Header("=== PREFAB ===")]
    [Tooltip("The flower prefab to spawn")]
    public GameObject flowerPrefab;

    [Header("=== SPAWN POINTS ===")]
    [Tooltip("List of all possible spawn points (empty GameObjects in the maze)")]
    public List<Transform> spawnPoints = new List<Transform>();

    // ===== REMOVED: Old UI References =====
    // [Header("=== UI REFERENCE ===")]
    // public FlowerCounterUI flowerCounterUI;  // <-- REMOVED, now uses GameManager
    // public NightIndicatorUI nightIndicatorUI; // <-- REMOVED, now uses GameManager

    // ===== REMOVED: Flowers Per Night (GameManager handles this now) =====
    // [Header("=== FLOWERS PER NIGHT ===")]
    // public int flowersNight1 = 2;  // <-- REMOVED
    // public int flowersNight2 = 3;  // <-- REMOVED
    // public int flowersNight3 = 4;  // <-- REMOVED

    [Header("=== DEBUG ===")]
    [Tooltip("Show spawn point gizmos in editor")]
    public bool showGizmos = true;

    [Tooltip("Gizmo color for spawn points")]
    public Color gizmoColor = Color.yellow;

    [Tooltip("Gizmo size")]
    public float gizmoSize = 0.5f;

    [Tooltip("Show debug logs")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private List<GameObject> spawnedFlowers = new List<GameObject>();
    private int totalFlowersThisNight = 0;
    private int flowersCollected = 0;

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("FlowerSpawnManager: Multiple instances detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ===== CHANGED: Don't auto-spawn - GameManager will call SpawnFlowersForNight() =====
        // GameManager.StartNight() will call SpawnFlowersForNight() when ready

        if (showDebugLogs)
        {
            Debug.Log("FlowerSpawnManager: Initialized. Waiting for GameManager to spawn flowers.");
        }
    }

    // ==================== SPAWN METHODS ====================
    /// <summary>
    /// Spawns flowers based on the night number
    /// Called by GameManager.StartNight()
    /// </summary>
    public void SpawnFlowersForNight(int nightNumber)
    {
        // Clear any existing flowers first
        ClearAllFlowers();

        // Get flower count from GameManager
        int flowerCount = 4; // Default fallback

        if (NightGameManager.Instance != null)
        {
            flowerCount = NightGameManager.Instance.GetFlowerCountForNight(nightNumber);
        }
        else
        {
            Debug.LogWarning("FlowerSpawnManager: GameManager not found! Using default flower count.");
        }

        // Spawn the flowers
        SpawnFlowers(flowerCount);

        // ===== REMOVED: UI update - GameManager handles this now =====
        // GameManager.StartNight() already calls UIManager.ResetFlowersForNewNight()

        if (showDebugLogs)
        {
            Debug.Log($"FlowerSpawnManager: Spawned {flowerCount} flowers for Night {nightNumber}");
        }
    }

    /// <summary>
    /// Spawns a specific number of flowers at random spawn points
    /// </summary>
    private void SpawnFlowers(int count)
    {
        // Validate
        if (flowerPrefab == null)
        {
            Debug.LogError("FlowerSpawnManager: Flower prefab not assigned!");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("FlowerSpawnManager: No spawn points assigned!");
            return;
        }

        if (count > spawnPoints.Count)
        {
            Debug.LogWarning($"FlowerSpawnManager: Not enough spawn points! Requested {count} but only have {spawnPoints.Count}");
            count = spawnPoints.Count;
        }

        // Reset tracking
        totalFlowersThisNight = count;
        flowersCollected = 0;

        // Get random spawn points (no duplicates)
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        // Spawn flowers at selected points
        foreach (Transform spawnPoint in selectedPoints)
        {
            GameObject flower = Instantiate(flowerPrefab, spawnPoint.position, spawnPoint.rotation);
            flower.name = "Flower_" + spawnedFlowers.Count;
            spawnedFlowers.Add(flower);
        }
    }

    /// <summary>
    /// Clears all spawned flowers from the scene
    /// </summary>
    public void ClearAllFlowers()
    {
        foreach (GameObject flower in spawnedFlowers)
        {
            if (flower != null)
            {
                Destroy(flower);
            }
        }
        spawnedFlowers.Clear();
        flowersCollected = 0;
    }

    // ==================== CALLED BY FLOWERPICKUP ====================
    /// <summary>
    /// Called by FlowerPickup when a flower is collected
    /// </summary>
    public void OnFlowerCollected(GameObject flower, Vector3 flowerPosition)
    {
        // Track collection locally
        flowersCollected++;

        // Remove from list
        if (spawnedFlowers.Contains(flower))
        {
            spawnedFlowers.Remove(flower);
        }

        // ===== UPDATED: Notify GameManager instead of UI directly =====
        if (NightGameManager.Instance != null)
        {
            NightGameManager.Instance.OnFlowerCollected(flowerPosition);
        }
        else
        {
            Debug.LogWarning("FlowerSpawnManager: GameManager not found! Flower collection not tracked.");
        }

        if (showDebugLogs)
        {
            Debug.Log($"FlowerSpawnManager: Flower collected at {flowerPosition}. Local count: {flowersCollected}/{totalFlowersThisNight}");
        }

        // ===== REMOVED: All flowers collected check - GameManager handles this now =====
        // GameManager.OnFlowerCollected() will check and call OnAllFlowersCollected()
    }

    // ==================== PUBLIC HELPER METHODS ====================
    /// <summary>
    /// Check if all flowers are collected this night (local tracking)
    /// </summary>
    public bool AreAllFlowersCollected()
    {
        return flowersCollected >= totalFlowersThisNight;
    }

    /// <summary>
    /// Get how many flowers remain (local tracking)
    /// </summary>
    public int GetFlowersRemaining()
    {
        return totalFlowersThisNight - flowersCollected;
    }

    /// <summary>
    /// Get total flowers this night
    /// </summary>
    public int GetTotalFlowersThisNight()
    {
        return totalFlowersThisNight;
    }

    /// <summary>
    /// Get list of currently spawned flowers
    /// </summary>
    public List<GameObject> GetSpawnedFlowers()
    {
        return spawnedFlowers;
    }

    // ==================== EDITOR GIZMOS ====================
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                // Draw sphere at spawn point
                Gizmos.DrawWireSphere(point.position, gizmoSize);

                // Draw flower icon (simple cross)
                Gizmos.DrawLine(point.position + Vector3.up * gizmoSize, point.position - Vector3.up * gizmoSize);
                Gizmos.DrawLine(point.position + Vector3.right * gizmoSize, point.position - Vector3.right * gizmoSize);
                Gizmos.DrawLine(point.position + Vector3.forward * gizmoSize, point.position - Vector3.forward * gizmoSize);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // When selected, show labels
        Gizmos.color = Color.green;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null)
            {
                Gizmos.DrawSphere(spawnPoints[i].position, gizmoSize * 0.3f);
            }
        }
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Spawn Night 1 Flowers")]
    public void TestSpawnNight1()
    {
        SpawnFlowersForNight(1);
    }

    [ContextMenu("Test: Spawn Night 2 Flowers")]
    public void TestSpawnNight2()
    {
        SpawnFlowersForNight(2);
    }

    [ContextMenu("Test: Spawn Night 3 Flowers")]
    public void TestSpawnNight3()
    {
        SpawnFlowersForNight(3);
    }

    [ContextMenu("Test: Clear All Flowers")]
    public void TestClearFlowers()
    {
        ClearAllFlowers();
    }
}