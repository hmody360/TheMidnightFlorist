using UnityEngine;
using TMPro;
using System.Collections.Generic;

// ================================================================================
// TASK UI MANAGER - Shows Flower Collection Objectives
// ================================================================================
// Displays a task list showing which flowers the player needs to collect.
// Completed tasks get a strikethrough effect.
//
// SETUP:
// 1. Create UI structure (see below)
// 2. Add this script to the TaskPanel GameObject
// 3. Assign the TaskList container in Inspector
// 4. Assign the TaskItemPrefab (or let it auto-create)
//
// UI HIERARCHY TO CREATE:
// Canvas
// ??? TaskPanel (add this script here) ? Entire panel hides/shows
//     ??? TaskHeader (TextMeshPro) - "Tasks:"
//     ??? TaskList (Empty GameObject - assign to this script)
//         ??? (Task items will be created automatically)
//
// FLOWER NAMES: Set in FlowerSpawnManager's FlowerZone.zoneName
// Example zone names: "Red Rose", "White Lily", "Yellow Tulip", "Pink Peony"
// ================================================================================

public class TaskUIManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static TaskUIManager Instance { get; private set; }

    // ==================== UI REFERENCES ====================
    [Header("=== UI REFERENCES ===")]
    [Tooltip("Container for task items (with Vertical Layout Group)")]
    public Transform taskListContainer;

    [Tooltip("Prefab for task item (optional - will create if not assigned)")]
    public GameObject taskItemPrefab;

    // ==================== APPEARANCE SETTINGS ====================
    [Header("=== TASK APPEARANCE ===")]
    [Tooltip("Font size for task items")]
    public float fontSize = 24f;

    [Tooltip("Color for incomplete tasks")]
    public Color incompleteColor = Color.white;

    [Tooltip("Color for completed tasks (with strikethrough)")]
    public Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gray, semi-transparent

    [Tooltip("Prefix for incomplete tasks")]
    public string incompletePrefix = "? Pick ";

    [Tooltip("Prefix for completed tasks")]
    public string completedPrefix = "? ";

    [Tooltip("Suffix for flower names")]
    public string flowerSuffix = " flower";

    // ==================== ANIMATION SETTINGS ====================
    [Header("=== ANIMATION (Optional) ===")]
    [Tooltip("Animate task completion")]
    public bool animateCompletion = true;

    [Tooltip("Duration of completion animation")]
    public float animationDuration = 0.3f;

    // ==================== DEBUG ====================
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private List<TextMeshProUGUI> taskItems = new List<TextMeshProUGUI>();
    private List<string> flowerNames = new List<string>();
    private List<bool> taskCompleted = new List<bool>();
    private Dictionary<string, int> flowerNameToIndex = new Dictionary<string, int>();

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("TaskUIManager: Multiple instances detected!");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Validate references
        if (taskListContainer == null)
        {
            Debug.LogError("TaskUIManager: TaskList container not assigned!");
        }

        // Hide entire TaskPanel initially (will show when night starts)
        // This hides both "Tasks:" header AND the task list
        gameObject.SetActive(false);

        if (showDebugLogs)
        {
            Debug.Log("TaskUIManager: Initialized and hidden until night starts.");
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Show "Go Back To The Shop" message when all flowers collected
    /// </summary>
    public void ShowReturnMessage()
    {
        // Clear all task items
        foreach (TextMeshProUGUI item in taskItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        taskItems.Clear();

        // Create the return message
        GameObject messageObj = new GameObject("ReturnMessage");
        messageObj.transform.SetParent(taskListContainer, false);

        // Add RectTransform
        RectTransform rect = messageObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(300, fontSize + 10);

        // Add TextMeshProUGUI
        TextMeshProUGUI textComponent = messageObj.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = fontSize;
        textComponent.color = Color.green; // Green color for success!
        textComponent.alignment = TextAlignmentOptions.Left;
        textComponent.text = "Go Back To The Shop";

        // Store reference so it gets cleaned up later
        taskItems.Add(textComponent);

        if (showDebugLogs)
        {
            Debug.Log("TaskUIManager: Showing return message");
        }
    }

    /// <summary>
    /// Initialize tasks for the night. Called by NightGameManager.StartNight()
    /// </summary>
    public void InitializeTasksForNight(List<string> flowers)
    {
        if (flowers == null || flowers.Count == 0)
        {
            Debug.LogWarning("TaskUIManager: No flowers provided!");
            return;
        }

        if (taskListContainer == null)
        {
            Debug.LogError("TaskUIManager: TaskList container not assigned!");
            return;
        }

        // Clear previous tasks
        ClearTasks();

        // Store flower names
        flowerNames = new List<string>(flowers);

        // Initialize completion tracking
        taskCompleted = new List<bool>();
        flowerNameToIndex.Clear();

        // Create task items
        for (int i = 0; i < flowers.Count; i++)
        {
            string flowerName = flowers[i];

            // Create task item
            TextMeshProUGUI taskText = CreateTaskItem(flowerName);
            taskItems.Add(taskText);
            taskCompleted.Add(false);

            // Map flower name to index (for quick lookup)
            // Use lowercase for case-insensitive matching
            string lowerName = flowerName.ToLower();
            if (!flowerNameToIndex.ContainsKey(lowerName))
            {
                flowerNameToIndex.Add(lowerName, i);
            }

            if (showDebugLogs)
            {
                Debug.Log($"TaskUIManager: Added task [{i}]: {flowerName}");
            }
        }

        // Show the entire TaskPanel
        gameObject.SetActive(true);

        if (showDebugLogs)
        {
            Debug.Log($"TaskUIManager: Initialized {flowers.Count} tasks for the night");
        }
    }

    /// <summary>
    /// Mark a flower as collected (by flower name)
    /// Called by NightGameManager.OnFlowerCollected() with the zone name
    /// </summary>
    public void OnFlowerCollected(string flowerName)
    {
        if (string.IsNullOrEmpty(flowerName))
        {
            Debug.LogWarning("TaskUIManager: Empty flower name provided!");
            return;
        }

        // Try exact match first (case-insensitive)
        string lowerName = flowerName.ToLower();
        if (flowerNameToIndex.TryGetValue(lowerName, out int index))
        {
            MarkTaskComplete(index);
            return;
        }

        // Try partial match (in case zone name differs slightly)
        for (int i = 0; i < flowerNames.Count; i++)
        {
            if (!taskCompleted[i]) // Only check incomplete tasks
            {
                string taskName = flowerNames[i].ToLower();
                if (taskName.Contains(lowerName) || lowerName.Contains(taskName))
                {
                    MarkTaskComplete(i);
                    return;
                }
            }
        }

        // No match found - use fallback
        if (showDebugLogs)
        {
            Debug.LogWarning($"TaskUIManager: Flower '{flowerName}' not found in task list! Using fallback.");
        }
        MarkNextTaskComplete();
    }

    /// <summary>
    /// Mark a task as complete by index
    /// </summary>
    public void MarkTaskComplete(int index)
    {
        if (index < 0 || index >= taskItems.Count)
        {
            Debug.LogWarning($"TaskUIManager: Invalid task index {index}");
            return;
        }

        if (taskCompleted[index])
        {
            if (showDebugLogs)
            {
                Debug.Log($"TaskUIManager: Task [{index}] already completed");
            }
            return;
        }

        // Mark as completed
        taskCompleted[index] = true;

        // Update the visual
        UpdateTaskVisual(index, true);

        if (showDebugLogs)
        {
            Debug.Log($"TaskUIManager: Task [{index}] '{flowerNames[index]}' completed!");
        }
    }

    /// <summary>
    /// Mark the first incomplete task as complete (fallback method)
    /// </summary>
    public void MarkNextTaskComplete()
    {
        for (int i = 0; i < taskCompleted.Count; i++)
        {
            if (!taskCompleted[i])
            {
                MarkTaskComplete(i);
                return;
            }
        }
    }

    /// <summary>
    /// Clear all tasks
    /// </summary>
    public void ClearTasks()
    {
        // Destroy existing task items
        foreach (TextMeshProUGUI item in taskItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        taskItems.Clear();
        flowerNames.Clear();
        taskCompleted.Clear();
        flowerNameToIndex.Clear();

        if (showDebugLogs)
        {
            Debug.Log("TaskUIManager: Tasks cleared");
        }
    }

    /// <summary>
    /// Hide the entire task panel
    /// </summary>
    public void HideTaskPanel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the entire task panel
    /// </summary>
    public void ShowTaskPanel()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check if all tasks are completed
    /// </summary>
    public bool AreAllTasksComplete()
    {
        foreach (bool completed in taskCompleted)
        {
            if (!completed) return false;
        }
        return taskCompleted.Count > 0;
    }

    // ==================== PRIVATE METHODS ====================

    /// <summary>
    /// Creates a single task item UI element
    /// </summary>
    private TextMeshProUGUI CreateTaskItem(string flowerName)
    {
        GameObject taskObj;

        if (taskItemPrefab != null)
        {
            // Use prefab
            taskObj = Instantiate(taskItemPrefab, taskListContainer);
        }
        else
        {
            // Create new GameObject
            taskObj = new GameObject($"Task_{flowerName}");
            taskObj.transform.SetParent(taskListContainer, false);

            // Add RectTransform
            RectTransform rect = taskObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, fontSize + 10);
        }

        // Get or add TextMeshProUGUI
        TextMeshProUGUI textComponent = taskObj.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = taskObj.AddComponent<TextMeshProUGUI>();
        }

        // Configure text
        textComponent.fontSize = fontSize;
        textComponent.color = incompleteColor;
        textComponent.alignment = TextAlignmentOptions.Left;
        textComponent.text = $"{incompletePrefix}{flowerName}{flowerSuffix}";

        return textComponent;
    }

    /// <summary>
    /// Updates the visual appearance of a task item
    /// </summary>
    private void UpdateTaskVisual(int index, bool completed)
    {
        if (index < 0 || index >= taskItems.Count) return;

        TextMeshProUGUI taskText = taskItems[index];
        if (taskText == null) return;

        string flowerName = flowerNames[index];

        if (completed)
        {
            // Strikethrough effect using TMP rich text tags
            taskText.text = $"{completedPrefix}<s>{flowerName}{flowerSuffix}</s>";
            taskText.color = completedColor;

            // Optional: Animate
            if (animateCompletion)
            {
                StartCoroutine(AnimateTaskCompletion(taskText));
            }
        }
        else
        {
            taskText.text = $"{incompletePrefix}{flowerName}{flowerSuffix}";
            taskText.color = incompleteColor;
        }
    }

    /// <summary>
    /// Simple scale animation for task completion
    /// </summary>
    private System.Collections.IEnumerator AnimateTaskCompletion(TextMeshProUGUI taskText)
    {
        if (taskText == null) yield break;

        RectTransform rect = taskText.rectTransform;
        Vector3 originalScale = rect.localScale;
        Vector3 targetScale = originalScale * 1.1f;

        // Scale up
        float elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (animationDuration / 2f);
            rect.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (animationDuration / 2f);
            rect.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        rect.localScale = originalScale;
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Initialize Sample Tasks")]
    public void TestInitializeTasks()
    {
        List<string> sampleFlowers = new List<string>
        {
            "Red Rose",
            "White Lily",
            "Yellow Tulip",
            "Pink Peony"
        };
        InitializeTasksForNight(sampleFlowers);
    }

    [ContextMenu("Test: Complete First Incomplete Task")]
    public void TestCompleteNextTask()
    {
        MarkNextTaskComplete();
    }

    [ContextMenu("Test: Clear All Tasks")]
    public void TestClearTasks()
    {
        ClearTasks();
    }
}