using UnityEngine;
using System.Collections.Generic;

// ================================================================================
// FLOWER SPAWN MANAGER - Updated for Multiple Flower Prefabs
// ================================================================================
// Spawns different flower prefabs at random locations.
// Each flower prefab will only spawn ONCE per night (no duplicates).
//
// SETUP:
// 1. Add this script to an empty GameObject in your scene
// 2. Assign your flower prefabs to the "flowerPrefabs" list (up to 4)
// 3. Assign spawn point Transforms to the "spawnPoints" list
// 4. NightGameManager will call SpawnFlowersForNight() automatically
//
// BEHAVIOR:
// - Night 1 (2 flowers): Randomly picks 2 different prefabs from your list
// - Night 2 (3 flowers): Randomly picks 3 different prefabs from your list
// - Night 3 (4 flowers): Uses all 4 prefabs (each spawns once)
// ================================================================================

public class FlowerSpawnManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static FlowerSpawnManager Instance { get; private set; }

    // ==================== REFERENCES ====================
    [Header("=== FLOWER PREFABS ===")]
    [Tooltip("List of different flower prefabs to spawn (up to 4). Each will only spawn ONCE per night.")]
    public List<GameObject> flowerPrefabs = new List<GameObject>();

    [Header("=== SPAWN POINTS ===")]
    [Tooltip("List of all possible spawn points (empty GameObjects in the maze)")]
    public List<Transform> spawnPoints = new List<Transform>();

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
        // Validate prefabs
        if (flowerPrefabs.Count == 0)
        {
            Debug.LogError("FlowerSpawnManager: No flower prefabs assigned! Add prefabs to the flowerPrefabs list.");
        }

        if (showDebugLogs)
        {
            Debug.Log($"FlowerSpawnManager: Initialized with {flowerPrefabs.Count} flower prefabs and {spawnPoints.Count} spawn points.");
        }
    }

    // ==================== SPAWN METHODS ====================
    /// <summary>
    /// Spawns flowers based on the night number.
    /// Called by NightGameManager.StartNight()
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
            Debug.LogWarning("FlowerSpawnManager: NightGameManager not found! Using default flower count.");
        }

        // Spawn the flowers
        SpawnFlowers(flowerCount);

        if (showDebugLogs)
        {
            Debug.Log($"FlowerSpawnManager: Spawned {flowerCount} unique flowers for Night {nightNumber}");
        }
    }

    /// <summary>
    /// Spawns a specific number of UNIQUE flowers at random spawn points.
    /// Each flower prefab will only be used ONCE.
    /// </summary>
    private void SpawnFlowers(int count)
    {
        // ========== VALIDATE PREFABS ==========
        if (flowerPrefabs.Count == 0)
        {
            Debug.LogError("FlowerSpawnManager: No flower prefabs assigned!");
            return;
        }

        // Check if we have enough prefabs
        if (count > flowerPrefabs.Count)
        {
            Debug.LogWarning($"FlowerSpawnManager: Requested {count} flowers but only have {flowerPrefabs.Count} prefabs! " +
                           $"Will spawn {flowerPrefabs.Count} unique flowers instead.");
            count = flowerPrefabs.Count;
        }

        // ========== VALIDATE SPAWN POINTS ==========
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

        // ========== RESET TRACKING ==========
        totalFlowersThisNight = count;
        flowersCollected = 0;

        // ========== SELECT RANDOM SPAWN POINTS (No Duplicates) ==========
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        // ========== SELECT RANDOM PREFABS (No Duplicates) ==========
        List<GameObject> availablePrefabs = new List<GameObject>(flowerPrefabs);
        List<GameObject> selectedPrefabs = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availablePrefabs.Count);
            selectedPrefabs.Add(availablePrefabs[randomIndex]);
            availablePrefabs.RemoveAt(randomIndex);
        }

        // ========== SPAWN UNIQUE FLOWERS AT RANDOM POINTS ==========
        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = selectedPoints[i];
            GameObject prefab = selectedPrefabs[i];

            // Spawn the flower
            GameObject flower = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            flower.name = $"Flower_{i}_{prefab.name}";
            spawnedFlowers.Add(flower);

            if (showDebugLogs)
            {
                Debug.Log($"FlowerSpawnManager: Spawned '{prefab.name}' at {spawnPoint.name}");
            }
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

        if (showDebugLogs)
        {
            Debug.Log("FlowerSpawnManager: All flowers cleared.");
        }
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

        // Notify NightGameManager
        if (NightGameManager.Instance != null)
        {
            NightGameManager.Instance.OnFlowerCollected(flowerPosition);
        }
        else
        {
            Debug.LogWarning("FlowerSpawnManager: NightGameManager not found! Flower collection not tracked.");
        }

        if (showDebugLogs)
        {
            Debug.Log($"FlowerSpawnManager: Flower collected at {flowerPosition}. Progress: {flowersCollected}/{totalFlowersThisNight}");
        }
    }

    // ==================== PUBLIC HELPER METHODS ====================
    /// <summary>
    /// Check if all flowers are collected this night
    /// </summary>
    public bool AreAllFlowersCollected()
    {
        return flowersCollected >= totalFlowersThisNight;
    }

    /// <summary>
    /// Get how many flowers remain
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

    /// <summary>
    /// Get number of flower prefabs available
    /// </summary>
    public int GetAvailablePrefabCount()
    {
        return flowerPrefabs.Count;
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
    [ContextMenu("Test: Spawn Night 1 Flowers (2 unique)")]
    public void TestSpawnNight1()
    {
        SpawnFlowersForNight(1);
    }

    [ContextMenu("Test: Spawn Night 2 Flowers (3 unique)")]
    public void TestSpawnNight2()
    {
        SpawnFlowersForNight(2);
    }

    [ContextMenu("Test: Spawn Night 3 Flowers (4 unique)")]
    public void TestSpawnNight3()
    {
        SpawnFlowersForNight(3);
    }

    [ContextMenu("Test: Clear All Flowers")]
    public void TestClearFlowers()
    {
        ClearAllFlowers();
    }

    [ContextMenu("Debug: Print Prefab Info")]
    public void DebugPrintPrefabInfo()
    {
        Debug.Log("===== FLOWER SPAWN MANAGER INFO =====");
        Debug.Log($"Flower Prefabs: {flowerPrefabs.Count}");
        for (int i = 0; i < flowerPrefabs.Count; i++)
        {
            string prefabName = flowerPrefabs[i] != null ? flowerPrefabs[i].name : "NULL";
            Debug.Log($"  [{i}] {prefabName}");
        }
        Debug.Log($"Spawn Points: {spawnPoints.Count}");
        Debug.Log($"Currently Spawned: {spawnedFlowers.Count}");
        Debug.Log($"Flowers Collected: {flowersCollected}/{totalFlowersThisNight}");
        Debug.Log("=====================================");
    }
}