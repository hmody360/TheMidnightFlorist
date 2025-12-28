using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// GameOverUI - Handles Win and Loss screens
/// 
/// LOSS CONDITIONS:
/// - Monster catches player - "You have been killed" + shows time
/// - Time ran out + had all flowers - "You opened late"
/// - Time ran out + missing flowers - "You did not collect the flowers"
/// 
/// WIN CONDITION:
/// - Player reaches home with all flowers before 6:00 AM - "You Survived" + shows time
/// </summary>
public class GameOverUI : MonoBehaviour
{
    // ==================== UI REFERENCES ====================
    [Header("=== MAIN PANEL ===")]
    [Tooltip("The main panel container (disable this to hide screen)")]
    public GameObject mainPanel;

    [Tooltip("The big stone frame background image")]
    public Image stoneFrameImage;

    [Tooltip("Full screen background image behind everything")]
    public Image backgroundOverlay;

    [Header("=== BACKGROUND COLORS ===")]
    [Tooltip("Background color for WIN screen")]
    public Color winBackgroundColor = Color.black;

    [Tooltip("Background color for LOSS screen")]
    public Color lossBackgroundColor = Color.black;

    [Header("=== TEXT ELEMENTS ===")]
    [Tooltip("Title text (YOU LOST or YOU SURVIVED)")]
    public TextMeshProUGUI titleText;

    [Tooltip("Night status text (Night 2 Failed or Night 2 Complete!)")]
    public TextMeshProUGUI nightStatusText;

    [Tooltip("Reason text (why player lost/won)")]
    public TextMeshProUGUI reasonText;

    [Tooltip("Time text (Time: 3:47 AM) - hidden for time ran out")]
    public TextMeshProUGUI timeText;

    [Header("=== BUTTONS ===")]
    [Tooltip("Restart/Start New Day button")]
    public Button actionButton;

    [Tooltip("Text on the action button")]
    public TextMeshProUGUI actionButtonText;

    [Tooltip("Menu button")]
    public Button menuButton;

    [Tooltip("Text on the menu button")]
    public TextMeshProUGUI menuButtonText;

    // ==================== SCENE NAMES ====================
    [Header("=== SCENE NAMES (Change in Inspector) ===")]
    [Tooltip("Scene to load for Day/Restart")]
    public string daySceneName = "DayScene";

    [Tooltip("Scene to load for Main Menu")]
    public string menuSceneName = "MainMenu";

    // ==================== TEXT SETTINGS ====================
    [Header("=== WIN TEXT ===")]
    public string winTitle = "YOU SURVIVED";
    public string winNightStatus = "Night {0} Complete!";
    public string winReason = "You made it home safely!";
    public string winButtonText = "Start New Day";

    [Header("=== LOSS TEXT ===")]
    public string lossTitle = "YOU LOST";
    public string lossNightStatus = "Night {0} Failed";
    public string lossButtonText = "Restart";

    [Header("=== LOSS REASONS ===")]
    public string lossReason_Monster = "You have been killed";
    public string lossReason_TimeOut_HadFlowers = "You opened late";
    public string lossReason_TimeOut_NoFlowers = "You did not collect the flowers";

    [Header("=== OTHER TEXT ===")]
    public string menuButtonLabel = "Menu";
    public string timeFormat = "Time: {0}";

    // ==================== STYLE SETTINGS ====================
    [Header("=== COLORS ===")]
    public Color winTitleColor = new Color(0.2f, 0.8f, 0.2f);
    public Color lossTitleColor = new Color(0.8f, 0.2f, 0.2f);
    public Color normalTextColor = Color.white;

    // ==================== REFERENCES TO OTHER UI ====================
    [Header("=== GAME REFERENCES (Optional) ===")]
    [Tooltip("Reference to TimerUI to get current time")]
    public TimerUI timerUI;

    [Tooltip("Reference to FlowerCounterUI to check flowers")]
    public FlowerCounterUI flowerCounterUI;

    [Tooltip("Reference to PlayerMovement to disable movement")]
    public PlayerMovement playerMovement;

    // ==================== PRIVATE VARIABLES ====================
    private bool isWin = false;
    private int currentNightNumber = 1;
    private bool gameOverTriggered = false;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        HideScreen();

        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        if (menuButtonText != null)
        {
            menuButtonText.text = menuButtonLabel;
        }
    }

    // ==================== CHECK IF GAME ALREADY OVER ====================
    public bool IsGameOver()
    {
        return gameOverTriggered;
    }

    // ==================== SHOW WIN SCREEN ====================
    public void ShowWin(int nightNumber, string timeWhenWon)
    {
        // Prevent double game over
        if (gameOverTriggered)
        {
            Debug.Log("GameOverUI: Game over already triggered, ignoring ShowWin");
            return;
        }
        gameOverTriggered = true;

        isWin = true;
        currentNightNumber = nightNumber;

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        // Set background color for WIN
        if (backgroundOverlay != null)
        {
            backgroundOverlay.color = winBackgroundColor;
            backgroundOverlay.gameObject.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = winTitle;
            titleText.color = winTitleColor;
        }

        if (nightStatusText != null)
        {
            nightStatusText.text = string.Format(winNightStatus, nightNumber);
            nightStatusText.color = normalTextColor;
        }

        if (reasonText != null)
        {
            reasonText.text = winReason;
            reasonText.color = normalTextColor;
        }

        if (timeText != null)
        {
            timeText.text = string.Format(timeFormat, timeWhenWon);
            timeText.gameObject.SetActive(true);
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = winButtonText;
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("GameOverUI: WIN - Night " + nightNumber + " at " + timeWhenWon);
    }

    public void ShowWin(int nightNumber)
    {
        string currentTime = "5:00 AM";

        if (timerUI != null)
        {
            currentTime = timerUI.GetClockTime();
        }

        ShowWin(nightNumber, currentTime);
    }

    // ==================== SHOW LOSS SCREEN ====================
    public void ShowLoss_CaughtByMonster(int nightNumber, string timeWhenCaught)
    {
        ShowLossScreen(nightNumber, lossReason_Monster, timeWhenCaught, true);
    }

    public void ShowLoss_CaughtByMonster(int nightNumber)
    {
        string currentTime = "3:00 AM";

        if (timerUI != null)
        {
            currentTime = timerUI.GetClockTime();
        }

        ShowLoss_CaughtByMonster(nightNumber, currentTime);
    }

    public void ShowLoss_TimeRanOut(int nightNumber, bool hadAllFlowers)
    {
        string reason = hadAllFlowers ? lossReason_TimeOut_HadFlowers : lossReason_TimeOut_NoFlowers;
        ShowLossScreen(nightNumber, reason, "", false);
    }

    public void ShowLoss_TimeRanOut(int nightNumber)
    {
        bool hadAllFlowers = true;

        if (flowerCounterUI != null)
        {
            hadAllFlowers = flowerCounterUI.AllFlowersCollected();
        }

        ShowLoss_TimeRanOut(nightNumber, hadAllFlowers);
    }

    private void ShowLossScreen(int nightNumber, string reason, string time, bool showTime)
    {
        // Prevent double game over
        if (gameOverTriggered)
        {
            Debug.Log("GameOverUI: Game over already triggered, ignoring ShowLoss");
            return;
        }
        gameOverTriggered = true;

        isWin = false;
        currentNightNumber = nightNumber;

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        // Set background color for LOSS
        if (backgroundOverlay != null)
        {
            backgroundOverlay.color = lossBackgroundColor;
            backgroundOverlay.gameObject.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = lossTitle;
            titleText.color = lossTitleColor;
        }

        if (nightStatusText != null)
        {
            nightStatusText.text = string.Format(lossNightStatus, nightNumber);
            nightStatusText.color = normalTextColor;
        }

        if (reasonText != null)
        {
            reasonText.text = reason;
            reasonText.color = normalTextColor;
        }

        if (timeText != null)
        {
            if (showTime && !string.IsNullOrEmpty(time))
            {
                timeText.text = string.Format(timeFormat, time);
                timeText.gameObject.SetActive(true);
            }
            else
            {
                timeText.gameObject.SetActive(false);
            }
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = lossButtonText;
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("GameOverUI: LOSS - Night " + nightNumber + ", Reason: " + reason);
    }

    // ==================== HIDE SCREEN ====================
    public void HideScreen()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Reset game over flag
        gameOverTriggered = false;
    }

    // ==================== BUTTON CALLBACKS ====================
    public void OnActionButtonClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(daySceneName))
        {
            Debug.Log("GameOverUI: Loading " + daySceneName);
            SceneManager.LoadScene(daySceneName);
        }
        else
        {
            Debug.LogWarning("GameOverUI: Day scene name not set!");
        }
    }

    public void OnMenuButtonClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            Debug.Log("GameOverUI: Loading " + menuSceneName);
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning("GameOverUI: Menu scene name not set!");
        }
    }

    // ==================== PUBLIC HELPER METHODS ====================
    public void SetNightNumber(int night)
    {
        currentNightNumber = night;
    }

    public bool IsShowing()
    {
        return mainPanel != null && mainPanel.activeSelf;
    }

    public bool DidPlayerWin()
    {
        return isWin;
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Show Win (Night 1)")]
    public void TestShowWin1()
    {
        ShowWin(1, "4:32 AM");
    }

    [ContextMenu("Test: Show Win (Night 2)")]
    public void TestShowWin2()
    {
        ShowWin(2, "5:15 AM");
    }

    [ContextMenu("Test: Show Loss - Monster (Night 1)")]
    public void TestShowLossMonster1()
    {
        ShowLoss_CaughtByMonster(1, "2:47 AM");
    }

    [ContextMenu("Test: Show Loss - Time Out Had Flowers (Night 2)")]
    public void TestShowLossTimeHadFlowers()
    {
        ShowLoss_TimeRanOut(2, true);
    }

    [ContextMenu("Test: Show Loss - Time Out No Flowers (Night 1)")]
    public void TestShowLossTimeNoFlowers()
    {
        ShowLoss_TimeRanOut(1, false);
    }

    [ContextMenu("Test: Hide Screen")]
    public void TestHideScreen()
    {
        HideScreen();
    }
}