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

    [Header("=== UI REFERENCE ===")]
    [Tooltip("Reference to FlowerCounterUI to update flower count")]
    public FlowerCounterUI flowerCounterUI;

    [Header("=== NIGHT INDICATOR ===")]
    [Tooltip("Reference to NightIndicatorUI to get current night")]
    public NightIndicatorUI nightIndicatorUI;

    [Header("=== FLOWERS PER NIGHT ===")]
    [Tooltip("How many flowers spawn on Night 1")]
    public int flowersNight1 = 2;

    [Tooltip("How many flowers spawn on Night 2")]
    public int flowersNight2 = 3;

    [Tooltip("How many flowers spawn on Night 3")]
    public int flowersNight3 = 4;

    [Header("=== DEBUG ===")]
    [Tooltip("Show spawn point gizmos in editor")]
    public bool showGizmos = true;

    [Tooltip("Gizmo color for spawn points")]
    public Color gizmoColor = Color.yellow;

    [Tooltip("Gizmo size")]
    public float gizmoSize = 0.5f;

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
        // Auto-spawn flowers based on current night
        if (nightIndicatorUI != null)
        {
            SpawnFlowersForNight(nightIndicatorUI.GetNight());
        }
        else
        {
            Debug.LogWarning("FlowerSpawnManager: NightIndicatorUI not assigned. Call SpawnFlowersForNight() manually.");
        }
    }

    // ==================== SPAWN METHODS ====================
    /// <summary>
    /// Spawns flowers based on the night number
    /// </summary>
    public void SpawnFlowersForNight(int nightNumber)
    {
        // Clear any existing flowers first
        ClearAllFlowers();

        // Determine how many flowers to spawn
        int flowerCount = GetFlowerCountForNight(nightNumber);

        // Spawn the flowers
        SpawnFlowers(flowerCount);

        // Update the UI
        if (flowerCounterUI != null)
        {
            flowerCounterUI.ResetForNewNight(flowerCount);
        }

        Debug.Log("FlowerSpawnManager: Spawned " + flowerCount + " flowers for Night " + nightNumber);
    }

    /// <summary>
    /// Gets how many flowers should spawn for a given night
    /// </summary>
    private int GetFlowerCountForNight(int nightNumber)
    {
        switch (nightNumber)
        {
            case 1: return flowersNight1;
            case 2: return flowersNight2;
            case 3: return flowersNight3;
            default: return flowersNight3; // Night 4+ uses same as night 3
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
            Debug.LogWarning("FlowerSpawnManager: Not enough spawn points! Requested " + count + " but only have " + spawnPoints.Count);
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
        // Update counter UI
        if (flowerCounterUI != null)
        {
            flowerCounterUI.CollectFlower();
        }

        // Track collection
        flowersCollected++;

        // Remove from list
        if (spawnedFlowers.Contains(flower))
        {
            spawnedFlowers.Remove(flower);
        }

        // ============================================
        // TODO: NOTIFY MONSTER HERE
        // The monster should head to: flowerPosition
        // Example: MonsterAI.Instance.InvestigatePosition(flowerPosition);
        // ============================================

        Debug.Log("FlowerSpawnManager: Flower collected at position " + flowerPosition + ". Total collected: " + flowersCollected + "/" + totalFlowersThisNight);

        // Check if all flowers collected
        if (flowersCollected >= totalFlowersThisNight)
        {
            OnAllFlowersCollected();
        }
    }

    /// <summary>
    /// Called when all flowers have been collected
    /// </summary>
    private void OnAllFlowersCollected()
    {
        Debug.Log("FlowerSpawnManager: ALL FLOWERS COLLECTED! Player can now return home.");

        // ============================================
        // TODO: Add any "all collected" effects here
        // Example: Play special sound, show UI message, etc.
        // ============================================
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