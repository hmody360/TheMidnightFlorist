using UnityEngine;
using UnityEngine.SceneManagement;

// ????????????????????????????????????????????????????????????????????????????????
// ?                              GAME MANAGER                                     ?
// ?  Central controller for the game - persists across scenes                     ?
// ?  - Tracks current night (1, 2, 3)                                             ?
// ?  - Handles win/loss conditions                                                ?
// ?  - Coordinates UIManager, FlowerSpawnManager, Monster                         ?
// ?  - Manages scene transitions                                                  ?
// ?                                                                               ?
// ?  MERGE NOTES: Look for "===== MERGE SECTION =====" comments                   ?
// ????????????????????????????????????????????????????????????????????????????????

public class NightGameManager : MonoBehaviour
{
    // ????????????????????????????????????????????????????????????????????????????
    // ?                           SINGLETON PATTERN                               ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: SINGLETON =====
    public static NightGameManager Instance { get; private set; }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                           SCENE NAMES                                     ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: SCENE NAMES =====
    [Header("=== SCENE NAMES ===")]
    [Tooltip("Name of the Day Scene")]
    public string daySceneName = "DayScene";

    [Tooltip("Name of the Night Scene")]
    public string nightSceneName = "NightScene";

    [Tooltip("Name of the Main Menu Scene")]
    public string mainMenuSceneName = "MainMenu";

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         NIGHT SETTINGS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: NIGHT SETTINGS =====
    [Header("=== NIGHT SETTINGS ===")]
    [Tooltip("Current night number (1, 2, or 3)")]
    public int currentNight = 1;

    [Tooltip("Maximum number of nights in the game")]
    public int maxNights = 3;

    [Header("=== FLOWERS PER NIGHT ===")]
    [Tooltip("Flowers to collect on Night 1")]
    public int flowersNight1 = 2;

    [Tooltip("Flowers to collect on Night 2")]
    public int flowersNight2 = 3;

    [Tooltip("Flowers to collect on Night 3")]
    public int flowersNight3 = 4;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                          GAME STATE                                       ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: GAME STATE =====
    [Header("=== GAME STATE (Read Only) ===")]
    [Tooltip("Is the night currently running?")]
    [SerializeField] private bool isNightRunning = false;

    [Tooltip("Is the game paused?")]
    [SerializeField] private bool isPaused = false;

    [Tooltip("Has the game ended (win or loss)?")]
    [SerializeField] private bool isGameOver = false;

    [Tooltip("Total flowers collected this night")]
    [SerializeField] private int flowersCollectedThisNight = 0;

    [Tooltip("Total flowers needed this night")]
    [SerializeField] private int flowersNeededThisNight = 0;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         DEBUG SETTINGS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: DEBUG =====
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         PRIVATE VARIABLES                                 ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== PRIVATE REFERENCES (Found at runtime) =====
    private NightUIManager uiManager;
    private FlowerSpawnManager flowerSpawnManager;
    private PlayerMovement playerMovement;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                          UNITY METHODS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: AWAKE =====
    void Awake()
    {
        // ----- Singleton Setup with DontDestroyOnLoad -----
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (showDebugLogs)
            {
                Debug.Log("GameManager: Instance created and set to DontDestroyOnLoad.");
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log("GameManager: Duplicate instance detected, destroying this one.");
            }
            Destroy(gameObject);
            return;
        }
    }

    // ===== MERGE SECTION: ON ENABLE =====
    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ===== MERGE SECTION: ON DISABLE =====
    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ===== MERGE SECTION: START =====
    void Start()
    {
        // Find references in the current scene
        FindSceneReferences();

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Started on Night {currentNight}");
        }
    }

    // ===== MERGE SECTION: ON SCENE LOADED =====
    /// <summary>
    /// Called whenever a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Scene '{scene.name}' loaded.");
        }

        // Unsubscribe from old UIManager event (if any)
        if (uiManager != null)
        {
            uiManager.OnTimerEnded -= OnTimeUp;
        }

        // Find references in the new scene
        FindSceneReferences();

        // Reset game over state when loading a new scene
        isGameOver = false;

        // Subscribe to timer event if UIManager exists
        if (uiManager != null)
        {
            uiManager.OnTimerEnded += OnTimeUp;
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                      FIND SCENE REFERENCES                                ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: FIND REFERENCES =====
    /// <summary>
    /// Finds all necessary references in the current scene
    /// </summary>
    private void FindSceneReferences()
    {
        // Find NightUIManager
        uiManager = NightUIManager.instance;
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<NightUIManager>();
        }

        // Find FlowerSpawnManager
        flowerSpawnManager = FlowerSpawnManager.Instance;
        if (flowerSpawnManager == null)
        {
            flowerSpawnManager = FindFirstObjectByType<FlowerSpawnManager>();
        }

        // Find PlayerMovement
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: References found - NightUIManager: {uiManager != null}, FlowerSpawnManager: {flowerSpawnManager != null}, PlayerMovement: {playerMovement != null}");
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         NIGHT MANAGEMENT                                  ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: SET NIGHT =====
    /// <summary>
    /// Sets which night the game is on (called by Day scene when player clicks door)
    /// </summary>
    public void SetNight(int night)
    {
        currentNight = Mathf.Clamp(night, 1, maxNights);

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Night set to {currentNight}");
        }
    }

    // ===== MERGE SECTION: GET NIGHT =====
    /// <summary>
    /// Gets the current night number
    /// </summary>
    public int GetCurrentNight()
    {
        return currentNight;
    }

    // ===== MERGE SECTION: START NIGHT =====
    /// <summary>
    /// Starts the night - called when player enters the maze
    /// Sets up UI, spawns flowers, starts timer
    /// </summary>
    public void StartNight()
    {
        if (isNightRunning)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("GameManager: Night already running!");
            }
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Starting Night {currentNight}");
        }

        // Reset state
        isNightRunning = true;
        isGameOver = false;
        isPaused = false;
        flowersCollectedThisNight = 0;
        flowersNeededThisNight = GetFlowerCountForNight(currentNight);

        // Make sure time is running
        Time.timeScale = 1f;

        // ----- Setup UIManager -----
        if (uiManager != null)
        {
            // Set night indicator
            uiManager.SetNight(currentNight);

            // Reset flower counter
            uiManager.ResetFlowersForNewNight(flowersNeededThisNight);

            // Reset and start timer
            uiManager.ResetTimer();
            uiManager.StartTimer();

            // Hide game over screen (in case it was showing)
            uiManager.HideGameOverScreen();

            if (showDebugLogs)
            {
                Debug.Log("GameManager: UIManager configured for night.");
            }
        }
        else
        {
            Debug.LogWarning("GameManager: UIManager not found! UI will not be updated.");
        }

        // ----- Spawn Flowers -----
        if (flowerSpawnManager != null)
        {
            flowerSpawnManager.SpawnFlowersForNight(currentNight);

            if (showDebugLogs)
            {
                Debug.Log("GameManager: Flowers spawned.");
            }
        }
        else
        {
            Debug.LogWarning("GameManager: FlowerSpawnManager not found! Flowers will not spawn.");
        }

        // ----- Enable Player Movement -----
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }

        // Lock cursor for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Night {currentNight} started! Collect {flowersNeededThisNight} flowers before 6:00 AM.");
        }
    }

    // ===== MERGE SECTION: GET FLOWER COUNT FOR NIGHT =====
    /// <summary>
    /// Gets how many flowers are needed for a specific night
    /// </summary>
    public int GetFlowerCountForNight(int night)
    {
        switch (night)
        {
            case 1: return flowersNight1;
            case 2: return flowersNight2;
            case 3: return flowersNight3;
            default: return flowersNight3; // Night 4+ uses same as night 3
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                       FLOWER COLLECTION                                   ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: ON FLOWER COLLECTED =====
    /// <summary>
    /// Called by FlowerSpawnManager when player collects a flower
    /// </summary>
    public void OnFlowerCollected(Vector3 flowerPosition)
    {
        if (isGameOver) return;

        flowersCollectedThisNight++;

        // Update UI
        if (uiManager != null)
        {
            uiManager.CollectFlower();
        }

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Flower collected! {flowersCollectedThisNight}/{flowersNeededThisNight}");
        }

        // ============================================
        // TODO: NOTIFY MONSTER HERE
        // The monster should investigate the flower position
        // Example: MonsterAI.Instance.InvestigatePosition(flowerPosition);
        // ============================================

        // Check if all flowers collected
        if (flowersCollectedThisNight >= flowersNeededThisNight)
        {
            OnAllFlowersCollected();
        }
    }

    // ===== MERGE SECTION: ON ALL FLOWERS COLLECTED =====
    /// <summary>
    /// Called when all flowers have been collected
    /// </summary>
    private void OnAllFlowersCollected()
    {
        if (showDebugLogs)
        {
            Debug.Log("GameManager: ALL FLOWERS COLLECTED! Player can now return home to win.");
        }

        // ============================================
        // TODO: Add any "all collected" effects here
        // Example: Play special sound, show UI message, etc.
        // ============================================
    }

    // ===== MERGE SECTION: CHECK ALL FLOWERS COLLECTED =====
    /// <summary>
    /// Check if all flowers have been collected this night
    /// </summary>
    public bool AreAllFlowersCollected()
    {
        return flowersCollectedThisNight >= flowersNeededThisNight;
    }

    // ===== MERGE SECTION: GET FLOWERS REMAINING =====
    /// <summary>
    /// Get how many flowers are still needed
    /// </summary>
    public int GetFlowersRemaining()
    {
        return flowersNeededThisNight - flowersCollectedThisNight;
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                        WIN CONDITION                                      ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: ON PLAYER REACHED HOME =====
    /// <summary>
    /// Called by HomeDoor when player interacts with it
    /// Returns true if player won, false if they still need flowers
    /// </summary>
    public bool OnPlayerReachedHome()
    {
        if (isGameOver) return false;

        // Check if all flowers collected
        if (AreAllFlowersCollected())
        {
            // WIN!
            TriggerWin();
            return true;
        }
        else
        {
            // Not all flowers collected - HomeDoor should show message
            if (showDebugLogs)
            {
                Debug.Log($"GameManager: Player reached home but still needs {GetFlowersRemaining()} flowers!");
            }
            return false;
        }
    }

    // ===== MERGE SECTION: TRIGGER WIN =====
    /// <summary>
    /// Triggers the win condition
    /// </summary>
    private void TriggerWin()
    {
        if (isGameOver) return;

        isGameOver = true;
        isNightRunning = false;

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: PLAYER WON Night {currentNight}!");
        }

        // Stop timer
        if (uiManager != null)
        {
            uiManager.PauseTimer();
            uiManager.ShowWin(currentNight);
        }

        // Disable player movement (UIManager.ShowWin already does this, but just in case)
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                        LOSS CONDITIONS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: ON PLAYER CAUGHT BY MONSTER =====
    /// <summary>
    /// Called by Monster when it catches the player (after jumpscare)
    /// </summary>
    public void OnPlayerCaughtByMonster()
    {
        if (isGameOver) return;

        isGameOver = true;
        isNightRunning = false;

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Player caught by monster on Night {currentNight}!");
        }

        // Stop timer
        if (uiManager != null)
        {
            uiManager.PauseTimer();
            uiManager.ShowLoss_CaughtByMonster(currentNight);
        }

        // Disable player movement (UIManager.ShowLoss already does this, but just in case)
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }
    }

    // ===== MERGE SECTION: ON TIME UP =====
    /// <summary>
    /// Called when timer reaches 6:00 AM (subscribed to UIManager.OnTimerEnded event)
    /// </summary>
    public void OnTimeUp()
    {
        if (isGameOver) return;

        isGameOver = true;
        isNightRunning = false;

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Time ran out on Night {currentNight}! Had all flowers: {AreAllFlowersCollected()}");
        }

        // Show loss screen
        if (uiManager != null)
        {
            uiManager.ShowLoss_TimeRanOut(currentNight, AreAllFlowersCollected());
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                      PAUSE / RESUME                                       ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: PAUSE GAME =====
    /// <summary>
    /// Pauses the game
    /// </summary>
    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;

        // Pause timer
        if (uiManager != null)
        {
            uiManager.PauseTimer();
        }

        // Show cursor for menu interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (showDebugLogs)
        {
            Debug.Log("GameManager: Game paused.");
        }
    }

    // ===== MERGE SECTION: RESUME GAME =====
    /// <summary>
    /// Resumes the game from pause
    /// </summary>
    public void ResumeGame()
    {
        if (isGameOver) return;

        isPaused = false;
        Time.timeScale = 1f;

        // Resume timer
        if (uiManager != null)
        {
            uiManager.ResumeTimer();
        }

        // Hide cursor for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (showDebugLogs)
        {
            Debug.Log("GameManager: Game resumed.");
        }
    }

    // ===== MERGE SECTION: TOGGLE PAUSE =====
    /// <summary>
    /// Toggles pause state
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // ===== MERGE SECTION: IS PAUSED =====
    /// <summary>
    /// Check if game is paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }

    // ===== MERGE SECTION: IS GAME OVER =====
    /// <summary>
    /// Check if game is over
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    // ===== MERGE SECTION: IS NIGHT RUNNING =====
    /// <summary>
    /// Check if night is currently running
    /// </summary>
    public bool IsNightRunning()
    {
        return isNightRunning;
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                      SCENE TRANSITIONS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: RESTART NIGHT =====
    /// <summary>
    /// Restarts the current night (reloads scene)
    /// </summary>
    public void RestartNight()
    {
        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Restarting Night {currentNight}");
        }

        // Reset state
        isNightRunning = false;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Reload the night scene
        SceneManager.LoadScene(nightSceneName);
    }

    // ===== MERGE SECTION: GO TO NEXT NIGHT =====
    /// <summary>
    /// Advances to the next night
    /// </summary>
    public void GoToNextNight()
    {
        currentNight++;

        if (currentNight > maxNights)
        {
            // Game complete! All nights finished
            if (showDebugLogs)
            {
                Debug.Log("GameManager: All nights completed! Game finished!");
            }

            // ============================================
            // TODO: Show game complete screen or credits
            // For now, go to main menu
            // ============================================
            GoToMainMenu();
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Advancing to Night {currentNight}");
        }

        // Reset state
        isNightRunning = false;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Go to day scene (player will then go to next night)
        GoToDayScene();
    }

    // ===== MERGE SECTION: GO TO DAY SCENE =====
    /// <summary>
    /// Loads the Day Scene
    /// </summary>
    public void GoToDayScene()
    {
        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Loading Day Scene (Day {currentNight})");
        }

        // Reset state
        isNightRunning = false;
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(daySceneName);
    }

    // ===== MERGE SECTION: GO TO NIGHT SCENE =====
    /// <summary>
    /// Loads the Night Scene
    /// </summary>
    public void GoToNightScene()
    {
        if (showDebugLogs)
        {
            Debug.Log($"GameManager: Loading Night Scene (Night {currentNight})");
        }

        // Reset state
        isNightRunning = false;
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(nightSceneName);
    }

    // ===== MERGE SECTION: GO TO MAIN MENU =====
    /// <summary>
    /// Loads the Main Menu
    /// </summary>
    public void GoToMainMenu()
    {
        if (showDebugLogs)
        {
            Debug.Log("GameManager: Loading Main Menu");
        }

        // Reset state
        currentNight = 1;
        isNightRunning = false;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Show cursor for menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         NEW GAME                                          ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: START NEW GAME =====
    /// <summary>
    /// Starts a new game from Night 1
    /// </summary>
    public void StartNewGame()
    {
        if (showDebugLogs)
        {
            Debug.Log("GameManager: Starting new game from Night 1");
        }

        // Reset to Night 1
        currentNight = 1;
        isNightRunning = false;
        isGameOver = false;
        isPaused = false;
        flowersCollectedThisNight = 0;
        Time.timeScale = 1f;

        // Go to Day 1
        GoToDayScene();
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         EDITOR TESTING                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== NIGHT TESTS =====
    [ContextMenu("Test: Start Night")]
    public void TestStartNight()
    {
        FindSceneReferences();
        StartNight();
    }

    [ContextMenu("Test: Set Night 1")]
    public void TestSetNight1()
    {
        SetNight(1);
    }

    [ContextMenu("Test: Set Night 2")]
    public void TestSetNight2()
    {
        SetNight(2);
    }

    [ContextMenu("Test: Set Night 3")]
    public void TestSetNight3()
    {
        SetNight(3);
    }

    // ===== FLOWER TESTS =====
    [ContextMenu("Test: Simulate Flower Collected")]
    public void TestFlowerCollected()
    {
        OnFlowerCollected(Vector3.zero);
    }

    [ContextMenu("Test: Simulate All Flowers Collected")]
    public void TestAllFlowersCollected()
    {
        flowersCollectedThisNight = flowersNeededThisNight;
        OnAllFlowersCollected();
    }

    // ===== WIN/LOSS TESTS =====
    [ContextMenu("Test: Simulate Win")]
    public void TestWin()
    {
        flowersCollectedThisNight = flowersNeededThisNight;
        TriggerWin();
    }

    [ContextMenu("Test: Simulate Loss - Monster")]
    public void TestLossMonster()
    {
        isGameOver = false; // Reset for testing
        OnPlayerCaughtByMonster();
    }

    [ContextMenu("Test: Simulate Loss - Time Up")]
    public void TestLossTimeUp()
    {
        isGameOver = false; // Reset for testing
        OnTimeUp();
    }

    // ===== PAUSE TESTS =====
    [ContextMenu("Test: Pause Game")]
    public void TestPause()
    {
        PauseGame();
    }

    [ContextMenu("Test: Resume Game")]
    public void TestResume()
    {
        ResumeGame();
    }

    // ===== SCENE TESTS =====
    [ContextMenu("Test: Go To Next Night")]
    public void TestGoToNextNight()
    {
        GoToNextNight();
    }

    [ContextMenu("Test: Restart Night")]
    public void TestRestartNight()
    {
        RestartNight();
    }

    [ContextMenu("Test: Go To Main Menu")]
    public void TestGoToMainMenu()
    {
        GoToMainMenu();
    }

    // ===== DEBUG INFO =====
    [ContextMenu("Debug: Print Current State")]
    public void DebugPrintState()
    {
        Debug.Log("===== GAME MANAGER STATE =====");
        Debug.Log($"Current Night: {currentNight}");
        Debug.Log($"Is Night Running: {isNightRunning}");
        Debug.Log($"Is Paused: {isPaused}");
        Debug.Log($"Is Game Over: {isGameOver}");
        Debug.Log($"Flowers Collected: {flowersCollectedThisNight}/{flowersNeededThisNight}");
        Debug.Log($"NightUIManager Found: {uiManager != null}");
        Debug.Log($"FlowerSpawnManager Found: {flowerSpawnManager != null}");
        Debug.Log($"PlayerMovement Found: {playerMovement != null}");
        Debug.Log("===============================");
    }
}