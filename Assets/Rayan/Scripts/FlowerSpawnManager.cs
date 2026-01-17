using UnityEngine;
using System.Collections.Generic;

// ================================================================================
// FLOWER SPAWN MANAGER - Zone-Based Spawning System
// ================================================================================
// Each flower type has its own zone where it can spawn.
// When a flower type is selected, it spawns at a random point within its zone.
//
// SETUP:
// 1. Add this script to an empty GameObject in your scene
// 2. Set the size of "flowerZones" to 4 (or however many flower types you have)
// 3. For each FlowerZone:
//    - Assign the flower prefab
//    - Assign the spawn points for that zone (the yellow dots in that colored area)
// 4. NightGameManager will call SpawnFlowersForNight() automatically
//
// BEHAVIOR:
// - Night 1 (2 flowers): Randomly picks 2 flower types, each spawns in its own zone
// - Night 2 (3 flowers): Randomly picks 3 flower types, each spawns in its own zone
// - Night 3 (4 flowers): All 4 flower types spawn, each in its own zone
//
// EXAMPLE:
// - FlowerZone 0: Rose prefab ? Blue zone spawn points
// - FlowerZone 1: Lily prefab ? Purple zone spawn points
// - FlowerZone 2: Sunflower prefab ? Pink zone spawn points
// - FlowerZone 3: Tulip prefab ? Orange zone spawn points
// ================================================================================

/// <summary>
/// Holds a flower prefab and its designated spawn zone points
/// </summary>
[System.Serializable]
public class FlowerZone
{
    [Tooltip("Name of this zone (for debugging)")]
    public string zoneName = "Zone";

    [Tooltip("The flower prefab that spawns in this zone")]
    public GameObject flowerPrefab;

    [Tooltip("Spawn points within this zone (the yellow dots in this colored area)")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("Gizmo color for this zone's spawn points")]
    public Color gizmoColor = Color.yellow;
}

public class FlowerSpawnManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static FlowerSpawnManager Instance { get; private set; }

    // ==================== FLOWER ZONES ====================
    [Header("=== FLOWER ZONES ===")]
    [Tooltip("Each flower type with its designated spawn zone")]
    public List<FlowerZone> flowerZones = new List<FlowerZone>();

    // ==================== DEBUG SETTINGS ====================
    [Header("=== DEBUG ===")]
    [Tooltip("Show spawn point gizmos in editor")]
    public bool showGizmos = true;

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
        // Validate zones
        ValidateZones();

        if (showDebugLogs)
        {
            Debug.Log($"FlowerSpawnManager: Initialized with {flowerZones.Count} flower zones.");
        }
    }

    /// <summary>
    /// Validates all flower zones have prefabs and spawn points
    /// </summary>
    private void ValidateZones()
    {
        if (flowerZones.Count == 0)
        {
            Debug.LogError("FlowerSpawnManager: No flower zones assigned! Add FlowerZones to the list.");
            return;
        }

        for (int i = 0; i < flowerZones.Count; i++)
        {
            FlowerZone zone = flowerZones[i];

            if (zone.flowerPrefab == null)
            {
                Debug.LogError($"FlowerSpawnManager: Zone [{i}] '{zone.zoneName}' has no flower prefab assigned!");
            }

            if (zone.spawnPoints.Count == 0)
            {
                Debug.LogError($"FlowerSpawnManager: Zone [{i}] '{zone.zoneName}' has no spawn points assigned!");
            }
            else
            {
                // Check for null spawn points
                int nullCount = 0;
                foreach (Transform point in zone.spawnPoints)
                {
                    if (point == null) nullCount++;
                }
                if (nullCount > 0)
                {
                    Debug.LogWarning($"FlowerSpawnManager: Zone [{i}] '{zone.zoneName}' has {nullCount} null spawn points!");
                }
            }
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
            Debug.Log($"FlowerSpawnManager: Spawned {flowerCount} flowers for Night {nightNumber}");
        }
    }

    /// <summary>
    /// Spawns a specific number of flowers, each in their designated zone.
    /// Randomly selects which flower types to spawn if count < total zones.
    /// </summary>
    private void SpawnFlowers(int count)
    {
        // ========== VALIDATE ==========
        if (flowerZones.Count == 0)
        {
            Debug.LogError("FlowerSpawnManager: No flower zones assigned!");
            return;
        }

        // Check if we have enough zones
        if (count > flowerZones.Count)
        {
            Debug.LogWarning($"FlowerSpawnManager: Requested {count} flowers but only have {flowerZones.Count} zones! " +
                           $"Will spawn {flowerZones.Count} flowers instead.");
            count = flowerZones.Count;
        }

        // ========== RESET TRACKING ==========
        totalFlowersThisNight = count;
        flowersCollected = 0;

        // ========== SELECT RANDOM ZONES (No Duplicates) ==========
        // Create a list of zone indices and shuffle to pick randomly
        List<int> availableZoneIndices = new List<int>();
        for (int i = 0; i < flowerZones.Count; i++)
        {
            // Only add zones that have valid prefabs and spawn points
            if (flowerZones[i].flowerPrefab != null && flowerZones[i].spawnPoints.Count > 0)
            {
                availableZoneIndices.Add(i);
            }
        }

        if (availableZoneIndices.Count < count)
        {
            Debug.LogWarning($"FlowerSpawnManager: Only {availableZoneIndices.Count} valid zones available, but {count} requested!");
            count = availableZoneIndices.Count;
            totalFlowersThisNight = count;
        }

        // Randomly select which zones to use
        List<int> selectedZoneIndices = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableZoneIndices.Count);
            selectedZoneIndices.Add(availableZoneIndices[randomIndex]);
            availableZoneIndices.RemoveAt(randomIndex);
        }

        // ========== SPAWN FLOWERS IN THEIR ZONES ==========
        foreach (int zoneIndex in selectedZoneIndices)
        {
            FlowerZone zone = flowerZones[zoneIndex];

            // Pick a random spawn point within this zone
            int randomPointIndex = Random.Range(0, zone.spawnPoints.Count);
            Transform spawnPoint = zone.spawnPoints[randomPointIndex];

            // Skip if spawn point is null
            if (spawnPoint == null)
            {
                Debug.LogWarning($"FlowerSpawnManager: Null spawn point in zone '{zone.zoneName}', skipping...");
                continue;
            }

            // Spawn the flower
            GameObject flower = Instantiate(zone.flowerPrefab, spawnPoint.position, spawnPoint.rotation);
            flower.name = $"Flower_{zone.zoneName}_{zone.flowerPrefab.name}";
            spawnedFlowers.Add(flower);

            if (showDebugLogs)
            {
                Debug.Log($"FlowerSpawnManager: Spawned '{zone.flowerPrefab.name}' in {zone.zoneName} at {spawnPoint.name}");
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
    /// Get list of flower zone names that were spawned this night
    /// Used by TaskUIManager to show objectives
    /// </summary>
    public List<string> GetSpawnedFlowerNames()
    {
        List<string> names = new List<string>();
        foreach (GameObject flower in spawnedFlowers)
        {
            if (flower != null)
            {
                // Extract zone name from flower name (format: "Flower_ZoneName_PrefabName")
                string[] parts = flower.name.Split('_');
                if (parts.Length >= 2)
                {
                    names.Add(parts[1]); // Zone name (e.g., "Red Rose")
                }
            }
        }
        return names;
    }

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
    /// Get number of flower zones available
    /// </summary>
    public int GetAvailableZoneCount()
    {
        return flowerZones.Count;
    }

    // ==================== EDITOR GIZMOS ====================
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw each zone's spawn points with its own color
        foreach (FlowerZone zone in flowerZones)
        {
            if (zone == null) continue;

            Gizmos.color = zone.gizmoColor;

            foreach (Transform point in zone.spawnPoints)
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
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // When selected, show solid spheres
        foreach (FlowerZone zone in flowerZones)
        {
            if (zone == null) continue;

            Gizmos.color = zone.gizmoColor;

            foreach (Transform point in zone.spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, gizmoSize * 0.3f);
                }
            }
        }
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Spawn Night 1 Flowers (2 zones)")]
    public void TestSpawnNight1()
    {
        SpawnFlowersForNight(1);
    }

    [ContextMenu("Test: Spawn Night 2 Flowers (3 zones)")]
    public void TestSpawnNight2()
    {
        SpawnFlowersForNight(2);
    }

    [ContextMenu("Test: Spawn Night 3 Flowers (4 zones)")]
    public void TestSpawnNight3()
    {
        SpawnFlowersForNight(3);
    }

    [ContextMenu("Test: Clear All Flowers")]
    public void TestClearFlowers()
    {
        ClearAllFlowers();
    }

    [ContextMenu("Debug: Print Zone Info")]
    public void DebugPrintZoneInfo()
    {
        Debug.Log("===== FLOWER SPAWN MANAGER - ZONE INFO =====");
        Debug.Log($"Total Zones: {flowerZones.Count}");

        for (int i = 0; i < flowerZones.Count; i++)
        {
            FlowerZone zone = flowerZones[i];
            string prefabName = zone.flowerPrefab != null ? zone.flowerPrefab.name : "NULL";
            Debug.Log($"  Zone [{i}] '{zone.zoneName}': Prefab='{prefabName}', SpawnPoints={zone.spawnPoints.Count}");
        }

        Debug.Log($"Currently Spawned: {spawnedFlowers.Count}");
        Debug.Log($"Flowers Collected: {flowersCollected}/{totalFlowersThisNight}");
        Debug.Log("=============================================");
    }
}