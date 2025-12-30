using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// ????????????????????????????????????????????????????????????????????????????????
// ?                              UI MANAGER                                       ?
// ?  Handles all UI elements for the Night Scene                                  ?
// ?  - Flower Counter (top-right)                                                 ?
// ?  - Timer with Dawn Effects (top-center)                                       ?
// ?  - Night Indicator (top-left)                                                 ?
// ?  - Stamina Bar (bottom-center)                                                ?
// ?  - Game Over Screen (win/loss)                                                ?
// ?                                                                               ?
// ?  MERGE NOTES: Look for "===== MERGE SECTION =====" comments                   ?
// ????????????????????????????????????????????????????????????????????????????????

public class NightUIManager : MonoBehaviour
{
    // ????????????????????????????????????????????????????????????????????????????
    // ?                           SINGLETON PATTERN                               ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: SINGLETON =====
    public static NightUIManager instance { get; private set; }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         PLAYER REFERENCE                                  ?
    // ????????????????????????????????????????????????????????????????????????????

    [Header("=== PLAYER REFERENCE ===")]
    [Tooltip("Reference to PlayerMovement for stamina and movement control")]
    public PlayerMovement playerMovement;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                    FLOWER COUNTER UI ELEMENTS                             ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: FLOWER COUNTER REFERENCES =====
    [Header("=== FLOWER COUNTER UI ===")]
    [Tooltip("The flower/wreath background image")]
    public Image flowerImage;

    [Tooltip("The text showing the number of flowers remaining")]
    public TextMeshProUGUI flowerCountText;

    // ===== MERGE SECTION: FLOWER COUNTER SETTINGS =====
    [Header("=== FLOWER COUNTER SETTINGS ===")]
    [Tooltip("Total flowers to collect this night")]
    public int totalFlowers = 4;

    [Tooltip("Current flowers remaining (updated by game)")]
    public int flowersRemaining = 4;

    [Header("=== FLOWER TEXT STYLE ===")]
    [Tooltip("Color of the flower count text")]
    public Color flowerTextColor = Color.white;

    [Tooltip("Enable text outline for flower counter")]
    public bool useFlowerOutline = true;

    [Tooltip("Flower outline color")]
    public Color flowerOutlineColor = Color.black;

    [Tooltip("Flower outline thickness")]
    public float flowerOutlineThickness = 0.2f;

    [Header("=== FLOWER PULSE EFFECT ===")]
    [Tooltip("Pulse effect when flower count changes")]
    public bool useFlowerPulseEffect = true;

    [Tooltip("How fast the flower pulse effect is")]
    public float flowerPulseSpeed = 5f;

    [Tooltip("How much the flower text scales during pulse")]
    public float flowerPulseScale = 1.3f;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                       TIMER UI ELEMENTS                                   ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: TIMER REFERENCES =====
    [Header("=== TIMER UI ===")]
    [Tooltip("The stone frame behind the timer")]
    public Image timerFrameImage;

    [Tooltip("The timer text display")]
    public TextMeshProUGUI timerText;

    // ===== MERGE SECTION: TIMER SETTINGS =====
    [Header("=== TIMER SETTINGS ===")]
    [Tooltip("Total game time in seconds (how long from 12AM to 6AM in real time)")]
    public float totalGameTimeSeconds = 360f; // 6 minutes = 6 hours in game

    [Tooltip("Is the timer currently running?")]
    public bool isTimerRunning = false;

    [Tooltip("Start timer automatically when game starts")]
    public bool autoStartTimer = false;

    [Header("=== TIMER TEXT STYLE ===")]
    [Tooltip("Timer text color")]
    public Color timerTextColor = Color.white;

    [Tooltip("Timer frame tint color")]
    public Color timerFrameTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    // ????????????????????????????????????????????????????????????????????????????
    // ?                    POST-PROCESSING / DAWN EFFECTS                         ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: POST-PROCESSING REFERENCES =====
    [Header("=== POST-PROCESSING VOLUME ===")]
    [Tooltip("The Global Volume in your scene")]
    public Volume postProcessVolume;

    // ===== MERGE SECTION: DAWN TIMING =====
    [Header("=== DAWN TIMING ===")]
    [Tooltip("When dawn effects start (0.916 = 91.6% = 5:30 AM, 0.75 = 75% = 4:30 AM)")]
    [Range(0f, 1f)]
    public float dawnStartPercent = 0.916f; // 5:30 AM = 5.5/6 = 91.6%

    // ===== MERGE SECTION: BRIGHTNESS SETTINGS =====
    [Header("=== BRIGHTNESS (Post Exposure) ===")]
    [Tooltip("Enable brightness changes")]
    public bool useBrightnessEffect = true;

    [Tooltip("Post Exposure at night (dark)")]
    public float nightPostExposure = -0.8f;

    [Tooltip("Post Exposure at 6:00 AM (dawn complete)")]
    public float dawnPostExposure = 0.5f;

    // ===== MERGE SECTION: COLOR FILTER SETTINGS =====
    [Header("=== COLOR FILTER (Tint) ===")]
    [Tooltip("Enable color filter changes")]
    public bool useColorFilter = true;

    [Tooltip("Color tint at night (dark blue #1A2A4A)")]
    public Color nightColorFilter = new Color(0.102f, 0.165f, 0.290f, 1f); // #1A2A4A

    [Tooltip("Color tint at 6:00 AM (warm peach #FFE4C4)")]
    public Color dawnColorFilter = new Color(1f, 0.894f, 0.769f, 1f); // #FFE4C4

    // ===== MERGE SECTION: SATURATION SETTINGS =====
    [Header("=== SATURATION (Color Intensity) ===")]
    [Tooltip("Enable saturation changes")]
    public bool useSaturation = true;

    [Tooltip("Saturation at night (less colorful)")]
    public float nightSaturation = -25f;

    [Tooltip("Saturation at 6:00 AM")]
    public float dawnSaturation = 10f;

    // ===== MERGE SECTION: CONTRAST SETTINGS =====
    [Header("=== CONTRAST ===")]
    [Tooltip("Enable contrast changes")]
    public bool useContrast = true;

    [Tooltip("Contrast at night")]
    public float nightContrast = 15f;

    [Tooltip("Contrast at 6:00 AM")]
    public float dawnContrast = 20f;

    // ===== MERGE SECTION: VIGNETTE SETTINGS =====
    [Header("=== VIGNETTE (Edge Darkness) ===")]
    [Tooltip("Enable vignette fading")]
    public bool useVignetteFade = true;

    [Tooltip("Vignette intensity at night")]
    public float nightVignetteIntensity = 0.4f;

    [Tooltip("Vignette intensity at 6:00 AM (0 = no vignette)")]
    public float dawnVignetteIntensity = 0f;

    [Tooltip("Vignette smoothness at night")]
    public float nightVignetteSmoothness = 0.5f;

    [Tooltip("Vignette smoothness at 6:00 AM")]
    public float dawnVignetteSmoothness = 1f;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                     NIGHT INDICATOR UI ELEMENTS                           ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: NIGHT INDICATOR REFERENCES =====
    [Header("=== NIGHT INDICATOR UI ===")]
    [Tooltip("The moon icon image")]
    public Image moonIcon;

    [Tooltip("The text showing 'Night X'")]
    public TextMeshProUGUI nightText;

    // ===== MERGE SECTION: NIGHT INDICATOR SETTINGS =====
    [Header("=== NIGHT INDICATOR SETTINGS ===")]
    [Tooltip("Current night number (1, 2, or 3)")]
    public int currentNight = 1;

    [Tooltip("Text format (use {0} for night number)")]
    public string nightTextFormat = "Night {0}";

    [Header("=== NIGHT INDICATOR STYLE ===")]
    [Tooltip("Night text color")]
    public Color nightTextColor = Color.white;

    [Tooltip("Moon icon tint color (white = no tint)")]
    public Color moonTint = Color.white;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                       STAMINA UI ELEMENTS                                 ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: STAMINA REFERENCES =====
    [Header("=== STAMINA UI ===")]
    [Tooltip("The stone frame Image (your asset)")]
    public Image staminaStoneFrame;

    [Tooltip("The LEFT fill bar RectTransform")]
    public RectTransform staminaFillBarLeftRect;

    [Tooltip("The RIGHT fill bar RectTransform")]
    public RectTransform staminaFillBarRightRect;

    [Tooltip("The LEFT fill bar Image (for color changes)")]
    public Image staminaFillBarLeftImage;

    [Tooltip("The RIGHT fill bar Image (for color changes)")]
    public Image staminaFillBarRightImage;

    [Tooltip("The dark background behind the fill")]
    public Image staminaDarkBackground;

    [Tooltip("Parent CanvasGroup for fading the entire stamina UI")]
    public CanvasGroup staminaCanvasGroup;

    // ===== MERGE SECTION: STAMINA SIZE SETTINGS =====
    [Header("=== STAMINA SIZE SETTINGS ===")]
    [Tooltip("Maximum width of each fill bar (when stamina is full)")]
    public float staminaMaxFillWidth = 190f;

    [Tooltip("Height of each fill bar")]
    public float staminaFillHeight = 10f;

    // ===== MERGE SECTION: STAMINA VISIBILITY SETTINGS =====
    [Header("=== STAMINA VISIBILITY SETTINGS ===")]
    [Tooltip("Stamina threshold to show the bar (default 18)")]
    public float staminaShowThreshold = 18f;

    [Tooltip("How fast the stamina bar fades in/out")]
    public float staminaFadeSpeed = 3f;

    // ===== MERGE SECTION: STAMINA COLOR SETTINGS =====
    [Header("=== STAMINA COLOR SETTINGS ===")]
    [Tooltip("Enable color gradient (changes color as stamina depletes)")]
    public bool useStaminaColorGradient = true;

    [Tooltip("Color when stamina is FULL")]
    public Color staminaFullColor = new Color(0.1f, 0.3f, 0.6f, 1f); // Dark blue

    [Tooltip("Color when stamina is EMPTY")]
    public Color staminaEmptyColor = new Color(0.05f, 0.1f, 0.25f, 1f); // Very dark blue

    [Tooltip("Tint color for the stamina stone frame")]
    public Color staminaStoneFrameTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Tooltip("Color for the stamina dark background")]
    public Color staminaBackgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);

    // ===== MERGE SECTION: STAMINA RECHARGE EFFECT =====
    [Header("=== STAMINA RECHARGE EFFECT ===")]
    [Tooltip("Enable pulsing effect when recharging (canSprint = false)")]
    public bool useStaminaRechargeEffect = true;

    [Tooltip("How fast the recharge pulse effect is")]
    public float staminaPulseSpeed = 3f;

    [Tooltip("Color when pulsing (recharging) - brighter")]
    public Color staminaRechargeColorBright = new Color(0.2f, 0.4f, 0.7f, 1f);

    [Tooltip("Color when pulsing (recharging) - darker")]
    public Color staminaRechargeColorDark = new Color(0.05f, 0.1f, 0.25f, 1f);

    // ????????????????????????????????????????????????????????????????????????????
    // ?                       GAME OVER UI ELEMENTS                               ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: GAME OVER REFERENCES =====
    [Header("=== GAME OVER UI ===")]
    [Tooltip("The main panel container (disable this to hide screen)")]
    public GameObject gameOverMainPanel;

    [Tooltip("The big stone frame background image")]
    public Image gameOverStoneFrameImage;

    [Tooltip("Full screen background image behind everything")]
    public Image gameOverBackgroundOverlay;

    [Header("=== GAME OVER BACKGROUND COLORS ===")]
    [Tooltip("Background color for WIN screen")]
    public Color winBackgroundColor = Color.black;

    [Tooltip("Background color for LOSS screen")]
    public Color lossBackgroundColor = Color.black;

    [Header("=== GAME OVER TEXT ELEMENTS ===")]
    [Tooltip("Title text (YOU LOST or YOU SURVIVED)")]
    public TextMeshProUGUI gameOverTitleText;

    [Tooltip("Night status text (Night 2 Failed or Night 2 Complete!)")]
    public TextMeshProUGUI gameOverNightStatusText;

    [Tooltip("Reason text (why player lost/won)")]
    public TextMeshProUGUI gameOverReasonText;

    [Tooltip("Time text (Time: 3:47 AM) - hidden for time ran out")]
    public TextMeshProUGUI gameOverTimeText;

    [Header("=== GAME OVER BUTTONS ===")]
    [Tooltip("Restart/Start New Day button")]
    public Button gameOverActionButton;

    [Tooltip("Text on the action button")]
    public TextMeshProUGUI gameOverActionButtonText;

    [Tooltip("Menu button")]
    public Button gameOverMenuButton;

    [Tooltip("Text on the menu button")]
    public TextMeshProUGUI gameOverMenuButtonText;

    // ===== MERGE SECTION: SCENE NAMES =====
    [Header("=== SCENE NAMES (Change in Inspector) ===")]
    [Tooltip("Scene to load for Day/Restart")]
    public string daySceneName = "DayScene";

    [Tooltip("Scene to load for Main Menu")]
    public string menuSceneName = "MainMenu";

    // ===== MERGE SECTION: GAME OVER TEXT SETTINGS =====
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

    // ===== MERGE SECTION: GAME OVER COLORS =====
    [Header("=== GAME OVER COLORS ===")]
    public Color winTitleColor = new Color(0.2f, 0.8f, 0.2f);
    public Color lossTitleColor = new Color(0.8f, 0.2f, 0.2f);
    public Color gameOverNormalTextColor = Color.white;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                           DEBUG SETTINGS                                  ?
    // ????????????????????????????????????????????????????????????????????????????

    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         PRIVATE VARIABLES                                 ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== FLOWER COUNTER PRIVATE VARIABLES =====
    private bool isFlowerPulsing = false;
    private float flowerPulseTimer = 0f;
    private Vector3 originalFlowerTextScale;
    private int lastFlowerCount;

    // ===== TIMER PRIVATE VARIABLES =====
    private float currentTime;
    private float initialTime;
    private bool hasTriggeredTimerLoss = false;

    // Post-processing effect references
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    // Store original post-processing values
    private float originalPostExposure;
    private Color originalColorFilter;
    private float originalSaturation;
    private float originalContrast;
    private float originalVignetteIntensity;
    private float originalVignetteSmoothness;

    // ===== STAMINA PRIVATE VARIABLES =====
    private float staminaTargetAlpha = 0f;
    private float staminaCurrentAlpha = 0f;
    private bool isStaminaRecharging = false;

    // ===== GAME OVER PRIVATE VARIABLES =====
    private bool isWin = false;
    private bool gameOverTriggered = false;

    // ===== TIMER EVENT =====
    public delegate void TimerEndedHandler();
    public event TimerEndedHandler OnTimerEnded;

    // ????????????????????????????????????????????????????????????????????????????
    // ?                          UNITY METHODS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: AWAKE METHOD =====
    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("UIManager: Multiple instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    // ===== MERGE SECTION: START METHOD =====
    void Start()
    {
        // ----- Initialize Flower Counter -----
        InitializeFlowerCounter();

        // ----- Initialize Timer -----
        InitializeTimer();

        // ----- Initialize Night Indicator -----
        InitializeNightIndicator();

        // ----- Initialize Stamina -----
        InitializeStamina();

        // ----- Initialize Game Over -----
        InitializeGameOver();

        if (showDebugLogs)
        {
            Debug.Log("UIManager: All UI systems initialized!");
        }
    }

    // ===== MERGE SECTION: UPDATE METHOD =====
    void Update()
    {
        // ----- Update Flower Counter -----
        UpdateFlowerCounter();

        // ----- Update Timer -----
        UpdateTimer();

        // ----- Update Stamina -----
        UpdateStamina();
    }

    // ===== MERGE SECTION: ON DESTROY METHOD =====
    void OnDestroy()
    {
        // Restore original post-processing values when destroyed (important for editor)
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = originalPostExposure;
            colorAdjustments.colorFilter.value = originalColorFilter;
            colorAdjustments.saturation.value = originalSaturation;
            colorAdjustments.contrast.value = originalContrast;
        }

        if (vignette != null)
        {
            vignette.intensity.value = originalVignetteIntensity;
            vignette.smoothness.value = originalVignetteSmoothness;
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                    FLOWER COUNTER METHODS                                 ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: FLOWER COUNTER INITIALIZATION =====
    private void InitializeFlowerCounter()
    {
        // Store original scale
        if (flowerCountText != null)
        {
            originalFlowerTextScale = flowerCountText.transform.localScale;

            // Apply text color
            flowerCountText.color = flowerTextColor;

            // Apply outline if enabled
            if (useFlowerOutline)
            {
                flowerCountText.outlineColor = flowerOutlineColor;
                flowerCountText.outlineWidth = flowerOutlineThickness;
            }
        }

        // Initialize
        lastFlowerCount = flowersRemaining;
        UpdateFlowerDisplay();

        if (showDebugLogs)
        {
            Debug.Log($"UIManager: Flower Counter initialized with {flowersRemaining} flowers.");
        }
    }

    // ===== MERGE SECTION: FLOWER COUNTER UPDATE =====
    private void UpdateFlowerCounter()
    {
        // Check if flower count changed
        if (flowersRemaining != lastFlowerCount)
        {
            lastFlowerCount = flowersRemaining;
            UpdateFlowerDisplay();

            // Trigger pulse effect
            if (useFlowerPulseEffect)
            {
                StartFlowerPulse();
            }
        }

        // Handle pulse animation
        if (isFlowerPulsing)
        {
            UpdateFlowerPulse();
        }
    }

    private void UpdateFlowerDisplay()
    {
        if (flowerCountText != null)
        {
            flowerCountText.text = flowersRemaining.ToString();
        }
    }

    private void StartFlowerPulse()
    {
        isFlowerPulsing = true;
        flowerPulseTimer = 0f;
    }

    private void UpdateFlowerPulse()
    {
        flowerPulseTimer += Time.deltaTime * flowerPulseSpeed;

        if (flowerPulseTimer >= Mathf.PI) // One complete pulse cycle
        {
            isFlowerPulsing = false;
            if (flowerCountText != null)
            {
                flowerCountText.transform.localScale = originalFlowerTextScale;
            }
            return;
        }

        // Sine wave for smooth pulse
        float scale = 1f + (Mathf.Sin(flowerPulseTimer) * (flowerPulseScale - 1f));

        if (flowerCountText != null)
        {
            flowerCountText.transform.localScale = originalFlowerTextScale * scale;
        }
    }

    // ===== MERGE SECTION: FLOWER COUNTER PUBLIC METHODS =====
    /// <summary>
    /// Set the total flowers for this night
    /// </summary>
    public void SetTotalFlowers(int total)
    {
        totalFlowers = total;
        flowersRemaining = total;
        UpdateFlowerDisplay();
    }

    /// <summary>
    /// Called when player collects a flower
    /// </summary>
    public void CollectFlower()
    {
        if (flowersRemaining > 0)
        {
            flowersRemaining--;
            if (showDebugLogs)
            {
                Debug.Log($"UIManager: Flower collected! {flowersRemaining} remaining.");
            }
        }
    }

    /// <summary>
    /// Set flowers remaining directly
    /// </summary>
    public void SetFlowersRemaining(int count)
    {
        flowersRemaining = Mathf.Max(0, count);
    }

    /// <summary>
    /// Get current flowers remaining
    /// </summary>
    public int GetFlowersRemaining()
    {
        return flowersRemaining;
    }

    /// <summary>
    /// Check if all flowers are collected
    /// </summary>
    public bool AllFlowersCollected()
    {
        return flowersRemaining <= 0;
    }

    /// <summary>
    /// Reset flower counter for new night
    /// </summary>
    public void ResetFlowersForNewNight(int flowerCount)
    {
        totalFlowers = flowerCount;
        flowersRemaining = flowerCount;
        UpdateFlowerDisplay();
        if (showDebugLogs)
        {
            Debug.Log($"UIManager: Flowers reset for new night with {flowerCount} flowers.");
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         TIMER METHODS                                     ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: TIMER INITIALIZATION =====
    private void InitializeTimer()
    {
        // Initialize timer
        currentTime = totalGameTimeSeconds;
        initialTime = totalGameTimeSeconds;
        hasTriggeredTimerLoss = false;

        // Apply frame tint
        if (timerFrameImage != null)
        {
            timerFrameImage.color = timerFrameTint;
        }

        // Apply text color
        if (timerText != null)
        {
            timerText.color = timerTextColor;
        }

        // Get post-processing effects from Volume
        SetupPostProcessing();

        // Auto start if enabled
        if (autoStartTimer)
        {
            StartTimer();
        }

        UpdateTimerDisplay();

        if (showDebugLogs)
        {
            float dawnStartHour = dawnStartPercent * 6f;
            Debug.Log($"UIManager: Timer initialized. Dawn starts at {dawnStartHour:F1} hours ({dawnStartPercent:P0})");
        }
    }

    // ===== MERGE SECTION: TIMER UPDATE =====
    private void UpdateTimer()
    {
        if (!isTimerRunning) return;

        // Count down
        currentTime -= Time.deltaTime;

        // Update display
        UpdateTimerDisplay();

        // Update effects
        UpdateDawnEffects();

        // Check for timer end
        if (currentTime <= 0f && !hasTriggeredTimerLoss)
        {
            currentTime = 0f;
            TimerEnded();
        }
    }

    private void SetupPostProcessing()
    {
        if (postProcessVolume == null)
        {
            // Try to find Global Volume in scene
            postProcessVolume = FindFirstObjectByType<Volume>();

            if (postProcessVolume == null)
            {
                Debug.LogWarning("UIManager: No Post-Processing Volume found! Dawn effects will not work.");
                return;
            }
        }

        // Get Color Adjustments effect
        if (postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            // Store original values
            originalPostExposure = colorAdjustments.postExposure.value;
            originalColorFilter = colorAdjustments.colorFilter.value;
            originalSaturation = colorAdjustments.saturation.value;
            originalContrast = colorAdjustments.contrast.value;

            if (showDebugLogs)
            {
                Debug.Log("UIManager: Found Color Adjustments.");
            }
        }
        else
        {
            Debug.LogWarning("UIManager: Color Adjustments not found in Volume! Add it for dawn effects.");
        }

        // Get Vignette effect
        if (postProcessVolume.profile.TryGet(out vignette))
        {
            // Store original values
            originalVignetteIntensity = vignette.intensity.value;
            originalVignetteSmoothness = vignette.smoothness.value;

            if (showDebugLogs)
            {
                Debug.Log("UIManager: Found Vignette.");
            }
        }
        else
        {
            Debug.LogWarning("UIManager: Vignette not found in Volume! Add it for vignette fade effect.");
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        timerText.text = FormatAsClockTime(currentTime);
    }

    /// <summary>
    /// Converts remaining time to clock format (12:00 AM to 6:00 AM)
    /// </summary>
    private string FormatAsClockTime(float remainingTime)
    {
        // Calculate how much time has passed (0 to 1)
        float timePassed = 1f - (remainingTime / initialTime);

        // Convert to hours (0 = 12:00 AM, 1 = 6:00 AM)
        float totalHours = timePassed * 6f; // 0 to 6 hours

        int hours = Mathf.FloorToInt(totalHours);
        int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);

        // Convert to 12-hour format
        int displayHour;
        string ampm = "AM";

        if (hours == 0)
        {
            displayHour = 12; // 12:00 AM (midnight)
        }
        else
        {
            displayHour = hours; // 1 AM to 6 AM
        }

        return string.Format("{0}:{1:00} {2}", displayHour, minutes, ampm);
    }

    // ===== MERGE SECTION: DAWN EFFECTS =====
    private void UpdateDawnEffects()
    {
        // Calculate how much time has passed (0 to 1)
        float timePassed = 1f - (currentTime / initialTime); // 0 = start, 1 = end

        // ===== DAWN EFFECT (After dawn start percent) =====
        if (timePassed >= dawnStartPercent)
        {
            // Calculate dawn progress (0 at dawn start, 1 at 6:00 AM)
            float dawnProgress = (timePassed - dawnStartPercent) / (1f - dawnStartPercent);
            dawnProgress = Mathf.Clamp01(dawnProgress);

            ApplyDawnEffects(dawnProgress);

            if (showDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"UIManager: Dawn {dawnProgress:P0} - Exposure: {colorAdjustments?.postExposure.value:F2}");
            }
        }
        // ===== NIGHT (Before dawn starts) =====
        else
        {
            ApplyNightEffects();
        }
    }

    private void ApplyNightEffects()
    {
        if (colorAdjustments != null)
        {
            if (useBrightnessEffect)
            {
                colorAdjustments.postExposure.value = nightPostExposure;
            }

            if (useColorFilter)
            {
                colorAdjustments.colorFilter.value = nightColorFilter;
            }

            if (useSaturation)
            {
                colorAdjustments.saturation.value = nightSaturation;
            }

            if (useContrast)
            {
                colorAdjustments.contrast.value = nightContrast;
            }
        }

        if (vignette != null && useVignetteFade)
        {
            vignette.intensity.value = nightVignetteIntensity;
            vignette.smoothness.value = nightVignetteSmoothness;
        }
    }

    private void ApplyDawnEffects(float progress)
    {
        if (colorAdjustments != null)
        {
            if (useBrightnessEffect)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(nightPostExposure, dawnPostExposure, progress);
            }

            if (useColorFilter)
            {
                colorAdjustments.colorFilter.value = Color.Lerp(nightColorFilter, dawnColorFilter, progress);
            }

            if (useSaturation)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(nightSaturation, dawnSaturation, progress);
            }

            if (useContrast)
            {
                colorAdjustments.contrast.value = Mathf.Lerp(nightContrast, dawnContrast, progress);
            }
        }

        if (vignette != null && useVignetteFade)
        {
            vignette.intensity.value = Mathf.Lerp(nightVignetteIntensity, dawnVignetteIntensity, progress);
            vignette.smoothness.value = Mathf.Lerp(nightVignetteSmoothness, dawnVignetteSmoothness, progress);
        }
    }

    private void TimerEnded()
    {
        hasTriggeredTimerLoss = true;
        isTimerRunning = false;

        if (showDebugLogs)
        {
            Debug.Log("UIManager: 6:00 AM reached! Timer ended.");
        }

        // Apply full dawn effects
        ApplyDawnEffects(1f);

        // Trigger event (GameManager listens to this and will show loss screen)
        OnTimerEnded?.Invoke();

        // NOTE: GameManager.OnTimeUp() will call ShowLoss_TimeRanOut()
        // We don't call it here to avoid double-triggering
    }

    // ===== MERGE SECTION: TIMER PUBLIC METHODS =====
    /// <summary>
    /// Start the timer countdown
    /// </summary>
    public void StartTimer()
    {
        isTimerRunning = true;
        hasTriggeredTimerLoss = false;

        if (showDebugLogs)
        {
            Debug.Log("UIManager: Timer started! Time is 12:00 AM");
        }
    }

    /// <summary>
    /// Pause the timer
    /// </summary>
    public void PauseTimer()
    {
        isTimerRunning = false;

        if (showDebugLogs)
        {
            Debug.Log("UIManager: Timer paused.");
        }
    }

    /// <summary>
    /// Resume the timer
    /// </summary>
    public void ResumeTimer()
    {
        if (currentTime > 0)
        {
            isTimerRunning = true;

            if (showDebugLogs)
            {
                Debug.Log("UIManager: Timer resumed.");
            }
        }
    }

    /// <summary>
    /// Reset timer to 12:00 AM
    /// </summary>
    public void ResetTimer()
    {
        currentTime = totalGameTimeSeconds;
        initialTime = totalGameTimeSeconds;
        hasTriggeredTimerLoss = false;
        isTimerRunning = false;

        // Reset to night values
        ApplyNightEffects();

        UpdateTimerDisplay();

        if (showDebugLogs)
        {
            Debug.Log("UIManager: Timer reset to 12:00 AM");
        }
    }

    /// <summary>
    /// Set the total game time (real seconds for the 6 in-game hours)
    /// </summary>
    public void SetTotalGameTime(float seconds)
    {
        totalGameTimeSeconds = seconds;
        ResetTimer();
    }

    /// <summary>
    /// Set total game time using minutes
    /// </summary>
    public void SetTotalGameTimeMinutes(float minutes)
    {
        totalGameTimeSeconds = minutes * 60f;
        ResetTimer();
    }

    /// <summary>
    /// Set when dawn starts (0.0 to 1.0)
    /// </summary>
    public void SetDawnStartPercent(float percent)
    {
        dawnStartPercent = Mathf.Clamp01(percent);

        if (showDebugLogs)
        {
            float dawnStartHour = dawnStartPercent * 6f;
            Debug.Log($"UIManager: Dawn now starts at {dawnStartHour:F1} hours ({dawnStartPercent:P0})");
        }
    }

    /// <summary>
    /// Set dawn start time using game hour (0-6)
    /// </summary>
    public void SetDawnStartHour(float hour)
    {
        dawnStartPercent = Mathf.Clamp01(hour / 6f);

        if (showDebugLogs)
        {
            Debug.Log($"UIManager: Dawn now starts at {hour:F1}:00 AM ({dawnStartPercent:P0})");
        }
    }

    /// <summary>
    /// Get current time remaining in seconds
    /// </summary>
    public float GetTimeRemaining()
    {
        return currentTime;
    }

    /// <summary>
    /// Get current clock time as string
    /// </summary>
    public string GetClockTime()
    {
        return FormatAsClockTime(currentTime);
    }

    /// <summary>
    /// Check if time is up (6:00 AM reached)
    /// </summary>
    public bool IsTimeUp()
    {
        return currentTime <= 0f;
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                     NIGHT INDICATOR METHODS                               ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: NIGHT INDICATOR INITIALIZATION =====
    private void InitializeNightIndicator()
    {
        // Apply colors
        if (nightText != null)
        {
            nightText.color = nightTextColor;
        }

        if (moonIcon != null)
        {
            moonIcon.color = moonTint;
        }

        // Update display
        UpdateNightDisplay();

        if (showDebugLogs)
        {
            Debug.Log($"UIManager: Night Indicator initialized to Night {currentNight}.");
        }
    }

    private void UpdateNightDisplay()
    {
        if (nightText != null)
        {
            nightText.text = string.Format(nightTextFormat, currentNight);
        }
    }

    // ===== MERGE SECTION: NIGHT INDICATOR PUBLIC METHODS =====
    /// <summary>
    /// Set the current night number (call this from GameManager)
    /// </summary>
    public void SetNight(int night)
    {
        currentNight = Mathf.Clamp(night, 1, 99); // Allow up to 99 nights
        UpdateNightDisplay();
        if (showDebugLogs)
        {
            Debug.Log($"UIManager: Set to Night {currentNight}");
        }
    }

    /// <summary>
    /// Get the current night number
    /// </summary>
    public int GetNight()
    {
        return currentNight;
    }

    /// <summary>
    /// Go to next night
    /// </summary>
    public void NextNight()
    {
        currentNight++;
        UpdateNightDisplay();
        if (showDebugLogs)
        {
            Debug.Log($"UIManager: Advanced to Night {currentNight}");
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                        STAMINA METHODS                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: STAMINA INITIALIZATION =====
    private void InitializeStamina()
    {
        // Initialize UI as invisible
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 0f;
            staminaCurrentAlpha = 0f;
        }

        // Apply stone frame tint to make it darker
        if (staminaStoneFrame != null)
        {
            staminaStoneFrame.color = staminaStoneFrameTint;
        }

        // Apply background color
        if (staminaDarkBackground != null)
        {
            staminaDarkBackground.color = staminaBackgroundColor;
        }

        // Apply initial colors to fill bars
        if (staminaFillBarLeftImage != null)
        {
            staminaFillBarLeftImage.color = staminaFullColor;
        }
        if (staminaFillBarRightImage != null)
        {
            staminaFillBarRightImage.color = staminaFullColor;
        }

        // Auto-find player if not assigned
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("UIManager: No PlayerMovement found! Please assign it in the Inspector.");
            }
        }

        if (showDebugLogs)
        {
            Debug.Log("UIManager: Stamina UI initialized.");
        }
    }

    // ===== MERGE SECTION: STAMINA UPDATE =====
    private void UpdateStamina()
    {
        if (playerMovement == null) return;

        // Get current stamina values from player
        float currentStamina = playerMovement.currentStamina;
        float maxStamina = playerMovement.maxStamina;

        // Update all stamina UI systems
        UpdateStaminaVisibility(currentStamina, maxStamina);
        UpdateStaminaFillBars(currentStamina, maxStamina);
        UpdateStaminaColors(currentStamina, maxStamina);
    }

    private void UpdateStaminaVisibility(float currentStamina, float maxStamina)
    {
        // Show bar when stamina drops below threshold
        if (currentStamina < staminaShowThreshold)
        {
            staminaTargetAlpha = 1f;
        }
        // Hide bar when stamina is full
        else if (currentStamina >= maxStamina)
        {
            staminaTargetAlpha = 0f;
        }

        // Smooth fade transition
        staminaCurrentAlpha = Mathf.MoveTowards(staminaCurrentAlpha, staminaTargetAlpha, staminaFadeSpeed * Time.deltaTime);

        // Apply alpha to canvas group
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = staminaCurrentAlpha;
        }
    }

    private void UpdateStaminaFillBars(float currentStamina, float maxStamina)
    {
        // Calculate fill percentage (0 to 1)
        float fillPercent = Mathf.Clamp01(currentStamina / maxStamina);

        // Calculate the width based on stamina
        float currentWidth = staminaMaxFillWidth * fillPercent;

        // Apply width to LEFT fill bar
        if (staminaFillBarLeftRect != null)
        {
            staminaFillBarLeftRect.sizeDelta = new Vector2(currentWidth, staminaFillHeight);
        }

        // Apply width to RIGHT fill bar
        if (staminaFillBarRightRect != null)
        {
            staminaFillBarRightRect.sizeDelta = new Vector2(currentWidth, staminaFillHeight);
        }
    }

    private void UpdateStaminaColors(float currentStamina, float maxStamina)
    {
        Color targetColor;

        // Check if in recharge mode (pulsing effect)
        if (isStaminaRecharging && useStaminaRechargeEffect)
        {
            // Pulsing effect: oscillate between bright and dark
            float pulse = (Mathf.Sin(Time.time * staminaPulseSpeed * Mathf.PI) + 1f) / 2f; // 0 to 1
            targetColor = Color.Lerp(staminaRechargeColorDark, staminaRechargeColorBright, pulse);
        }
        // Normal gradient based on stamina level
        else if (useStaminaColorGradient)
        {
            float staminaPercent = currentStamina / maxStamina;
            targetColor = Color.Lerp(staminaEmptyColor, staminaFullColor, staminaPercent);
        }
        // Single color mode
        else
        {
            targetColor = staminaFullColor;
        }

        // Apply color to BOTH fill bars
        if (staminaFillBarLeftImage != null)
        {
            staminaFillBarLeftImage.color = targetColor;
        }

        if (staminaFillBarRightImage != null)
        {
            staminaFillBarRightImage.color = targetColor;
        }
    }

    // ===== MERGE SECTION: STAMINA PUBLIC METHODS =====
    /// <summary>
    /// Called by PlayerMovement when canSprint changes to false (stamina depleted)
    /// </summary>
    public void SetStaminaRecharging(bool recharging)
    {
        isStaminaRecharging = recharging;

        if (showDebugLogs)
        {
            if (recharging)
            {
                Debug.Log("UIManager: Stamina recharge mode ON - pulsing effect active");
            }
            else
            {
                Debug.Log("UIManager: Stamina recharge mode OFF");
            }
        }
    }

    /// <summary>
    /// Check if stamina is currently recharging
    /// </summary>
    public bool IsStaminaRecharging()
    {
        return isStaminaRecharging;
    }

    /// <summary>
    /// Force show the stamina bar (useful for testing)
    /// </summary>
    public void ForceShowStamina()
    {
        staminaTargetAlpha = 1f;
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 1f;
            staminaCurrentAlpha = 1f;
        }
        if (showDebugLogs)
        {
            Debug.Log("UIManager: Force Show Stamina triggered");
        }
    }

    /// <summary>
    /// Force hide the stamina bar
    /// </summary>
    public void ForceHideStamina()
    {
        staminaTargetAlpha = 0f;
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 0f;
            staminaCurrentAlpha = 0f;
        }
        if (showDebugLogs)
        {
            Debug.Log("UIManager: Force Hide Stamina triggered");
        }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                       GAME OVER METHODS                                   ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: GAME OVER INITIALIZATION =====
    private void InitializeGameOver()
    {
        HideGameOverScreen();

        if (gameOverActionButton != null)
        {
            gameOverActionButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (gameOverMenuButton != null)
        {
            gameOverMenuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        if (gameOverMenuButtonText != null)
        {
            gameOverMenuButtonText.text = menuButtonLabel;
        }

        if (showDebugLogs)
        {
            Debug.Log("UIManager: Game Over UI initialized.");
        }
    }

    // ===== MERGE SECTION: GAME OVER CHECK =====
    /// <summary>
    /// Check if game over has already been triggered
    /// </summary>
    public bool IsGameOver()
    {
        return gameOverTriggered;
    }

    // ===== MERGE SECTION: SHOW WIN SCREEN =====
    /// <summary>
    /// Show win screen with specified night and time
    /// </summary>
    public void ShowWin(int nightNumber, string timeWhenWon)
    {
        // Prevent double game over
        if (gameOverTriggered)
        {
            if (showDebugLogs)
            {
                Debug.Log("UIManager: Game over already triggered, ignoring ShowWin");
            }
            return;
        }
        gameOverTriggered = true;

        isWin = true;

        if (gameOverMainPanel != null)
        {
            gameOverMainPanel.SetActive(true);
        }

        // Set background color for WIN
        if (gameOverBackgroundOverlay != null)
        {
            gameOverBackgroundOverlay.color = winBackgroundColor;
            gameOverBackgroundOverlay.gameObject.SetActive(true);
        }

        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = winTitle;
            gameOverTitleText.color = winTitleColor;
        }

        if (gameOverNightStatusText != null)
        {
            gameOverNightStatusText.text = string.Format(winNightStatus, nightNumber);
            gameOverNightStatusText.color = gameOverNormalTextColor;
        }

        if (gameOverReasonText != null)
        {
            gameOverReasonText.text = winReason;
            gameOverReasonText.color = gameOverNormalTextColor;
        }

        if (gameOverTimeText != null)
        {
            gameOverTimeText.text = string.Format(timeFormat, timeWhenWon);
            gameOverTimeText.gameObject.SetActive(true);
        }

        if (gameOverActionButtonText != null)
        {
            gameOverActionButtonText.text = winButtonText;
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (showDebugLogs)
        {
            Debug.Log("UIManager: WIN - Night " + nightNumber + " at " + timeWhenWon);
        }
    }

    /// <summary>
    /// Show win screen (auto-gets current time from timer)
    /// </summary>
    public void ShowWin(int nightNumber)
    {
        string currentTime = GetClockTime();
        ShowWin(nightNumber, currentTime);
    }

    // ===== MERGE SECTION: SHOW LOSS SCREEN - MONSTER =====
    /// <summary>
    /// Show loss screen when caught by monster
    /// </summary>
    public void ShowLoss_CaughtByMonster(int nightNumber, string timeWhenCaught)
    {
        ShowLossScreen(nightNumber, lossReason_Monster, timeWhenCaught, true);
    }

    /// <summary>
    /// Show loss screen when caught by monster (auto-gets current time)
    /// </summary>
    public void ShowLoss_CaughtByMonster(int nightNumber)
    {
        string currentTimeStr = GetClockTime();
        ShowLoss_CaughtByMonster(nightNumber, currentTimeStr);
    }

    // ===== MERGE SECTION: SHOW LOSS SCREEN - TIME RAN OUT =====
    /// <summary>
    /// Show loss screen when time ran out
    /// </summary>
    public void ShowLoss_TimeRanOut(int nightNumber, bool hadAllFlowers)
    {
        string reason = hadAllFlowers ? lossReason_TimeOut_HadFlowers : lossReason_TimeOut_NoFlowers;
        ShowLossScreen(nightNumber, reason, "", false);
    }

    /// <summary>
    /// Show loss screen when time ran out (auto-checks if had all flowers)
    /// </summary>
    public void ShowLoss_TimeRanOut(int nightNumber)
    {
        bool hadAllFlowers = AllFlowersCollected();
        ShowLoss_TimeRanOut(nightNumber, hadAllFlowers);
    }

    private void ShowLossScreen(int nightNumber, string reason, string time, bool showTime)
    {
        // Prevent double game over
        if (gameOverTriggered)
        {
            if (showDebugLogs)
            {
                Debug.Log("UIManager: Game over already triggered, ignoring ShowLoss");
            }
            return;
        }
        gameOverTriggered = true;

        isWin = false;

        if (gameOverMainPanel != null)
        {
            gameOverMainPanel.SetActive(true);
        }

        // Set background color for LOSS
        if (gameOverBackgroundOverlay != null)
        {
            gameOverBackgroundOverlay.color = lossBackgroundColor;
            gameOverBackgroundOverlay.gameObject.SetActive(true);
        }

        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = lossTitle;
            gameOverTitleText.color = lossTitleColor;
        }

        if (gameOverNightStatusText != null)
        {
            gameOverNightStatusText.text = string.Format(lossNightStatus, nightNumber);
            gameOverNightStatusText.color = gameOverNormalTextColor;
        }

        if (gameOverReasonText != null)
        {
            gameOverReasonText.text = reason;
            gameOverReasonText.color = gameOverNormalTextColor;
        }

        if (gameOverTimeText != null)
        {
            if (showTime && !string.IsNullOrEmpty(time))
            {
                gameOverTimeText.text = string.Format(timeFormat, time);
                gameOverTimeText.gameObject.SetActive(true);
            }
            else
            {
                gameOverTimeText.gameObject.SetActive(false);
            }
        }

        if (gameOverActionButtonText != null)
        {
            gameOverActionButtonText.text = lossButtonText;
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (showDebugLogs)
        {
            Debug.Log("UIManager: LOSS - Night " + nightNumber + ", Reason: " + reason);
        }
    }

    // ===== MERGE SECTION: HIDE GAME OVER SCREEN =====
    /// <summary>
    /// Hide the game over screen
    /// </summary>
    public void HideGameOverScreen()
    {
        if (gameOverMainPanel != null)
        {
            gameOverMainPanel.SetActive(false);
        }

        if (gameOverBackgroundOverlay != null)
        {
            gameOverBackgroundOverlay.gameObject.SetActive(false);
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

    // ===== MERGE SECTION: GAME OVER BUTTON CALLBACKS =====
    public void OnActionButtonClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(daySceneName))
        {
            if (showDebugLogs)
            {
                Debug.Log("UIManager: Loading " + daySceneName);
            }
            SceneManager.LoadScene(daySceneName);
        }
        else
        {
            Debug.LogWarning("UIManager: Day scene name not set!");
        }
    }

    public void OnMenuButtonClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            if (showDebugLogs)
            {
                Debug.Log("UIManager: Loading " + menuSceneName);
            }
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning("UIManager: Menu scene name not set!");
        }
    }

    // ===== MERGE SECTION: GAME OVER PUBLIC HELPER METHODS =====
    /// <summary>
    /// Set the night number for game over display
    /// </summary>
    public void SetGameOverNightNumber(int night)
    {
        currentNight = night;
    }

    /// <summary>
    /// Check if game over screen is currently showing
    /// </summary>
    public bool IsGameOverShowing()
    {
        return gameOverMainPanel != null && gameOverMainPanel.activeSelf;
    }

    /// <summary>
    /// Check if player won
    /// </summary>
    public bool DidPlayerWin()
    {
        return isWin;
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                    PLACEHOLDER METHODS FOR FRIEND                         ?
    // ?         (These are called by RayInteractor - friend will implement)       ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== MERGE SECTION: CROSSHAIR (Friend will implement) =====
    /// <summary>
    /// Change crosshair state (0 = normal, 1 = interaction)
    /// TODO: Friend will implement this
    /// </summary>
    public void ChangeCrossHair(int state)
    {
        // Placeholder - Friend will implement crosshair system
        // if (showDebugLogs)
        // {
        //     Debug.Log($"UIManager: ChangeCrossHair called with state {state} - NOT IMPLEMENTED");
        // }
    }

    // ===== MERGE SECTION: PROMPT TEXT (Friend will implement) =====
    /// <summary>
    /// Set interaction prompt text
    /// TODO: Friend will implement this
    /// </summary>
    public void setPromptText(string text, Color color)
    {
        // Placeholder - Friend will implement prompt system
        // if (showDebugLogs)
        // {
        //     Debug.Log($"UIManager: setPromptText called with '{text}' - NOT IMPLEMENTED");
        // }
    }

    // ????????????????????????????????????????????????????????????????????????????
    // ?                         EDITOR TESTING                                    ?
    // ????????????????????????????????????????????????????????????????????????????

    // ===== FLOWER COUNTER TESTS =====
    [ContextMenu("Test: Collect Flower")]
    public void TestCollectFlower()
    {
        CollectFlower();
    }

    [ContextMenu("Test: Reset to 4 Flowers")]
    public void TestResetFlowers4()
    {
        ResetFlowersForNewNight(4);
    }

    [ContextMenu("Test: Reset to 3 Flowers")]
    public void TestResetFlowers3()
    {
        ResetFlowersForNewNight(3);
    }

    // ===== TIMER TESTS =====
    [ContextMenu("Test: Start Timer")]
    public void TestStartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    [ContextMenu("Test: Pause Timer")]
    public void TestPauseTimer()
    {
        PauseTimer();
    }

    [ContextMenu("Test: Set to Dawn Start Time")]
    public void TestSetToDawnStart()
    {
        float remainingPercent = 1f - dawnStartPercent;
        currentTime = initialTime * remainingPercent;
        isTimerRunning = true;
        Debug.Log($"UIManager: Set to dawn start ({dawnStartPercent:P0} passed)");
    }

    [ContextMenu("Test: Set to 5:45 AM")]
    public void TestSet545AM()
    {
        currentTime = initialTime * 0.042f; // 4.2% remaining
        isTimerRunning = true;
        Debug.Log("UIManager: Set to 5:45 AM");
    }

    [ContextMenu("Test: Set Game Time 6 Minutes")]
    public void TestSet6Minutes()
    {
        SetTotalGameTimeMinutes(6f);
    }

    [ContextMenu("Test: Set Game Time 3 Minutes")]
    public void TestSet3Minutes()
    {
        SetTotalGameTimeMinutes(3f);
    }

    [ContextMenu("Test: Set Game Time 1 Minute (Fast Test)")]
    public void TestSet1Minute()
    {
        SetTotalGameTimeMinutes(1f);
    }

    [ContextMenu("Test: Preview Night Settings")]
    public void TestPreviewNight()
    {
        SetupPostProcessing();
        ApplyNightEffects();
        Debug.Log("UIManager: Preview - Night settings applied");
    }

    [ContextMenu("Test: Preview Dawn 50%")]
    public void TestPreviewDawn50()
    {
        SetupPostProcessing();
        ApplyDawnEffects(0.5f);
        Debug.Log("UIManager: Preview - Dawn 50% applied");
    }

    [ContextMenu("Test: Preview Dawn 100%")]
    public void TestPreviewDawn100()
    {
        SetupPostProcessing();
        ApplyDawnEffects(1f);
        Debug.Log("UIManager: Preview - Dawn 100% (6:00 AM) applied");
    }

    // ===== NIGHT INDICATOR TESTS =====
    [ContextMenu("Test: Set Night 1")]
    public void TestNight1()
    {
        SetNight(1);
    }

    [ContextMenu("Test: Set Night 2")]
    public void TestNight2()
    {
        SetNight(2);
    }

    [ContextMenu("Test: Set Night 3")]
    public void TestNight3()
    {
        SetNight(3);
    }

    // ===== STAMINA TESTS =====
    [ContextMenu("Test: Force Show Stamina")]
    public void TestForceShowStamina()
    {
        ForceShowStamina();
    }

    [ContextMenu("Test: Force Hide Stamina")]
    public void TestForceHideStamina()
    {
        ForceHideStamina();
    }

    [ContextMenu("Test: Stamina Fill 50%")]
    public void TestStaminaFill50()
    {
        float testWidth = staminaMaxFillWidth * 0.5f;
        if (staminaFillBarLeftRect != null) staminaFillBarLeftRect.sizeDelta = new Vector2(testWidth, staminaFillHeight);
        if (staminaFillBarRightRect != null) staminaFillBarRightRect.sizeDelta = new Vector2(testWidth, staminaFillHeight);
        ForceShowStamina();
        Debug.Log($"UIManager: Test Stamina Fill 50% - Width set to {testWidth}");
    }

    [ContextMenu("Test: Stamina Fill 25%")]
    public void TestStaminaFill25()
    {
        float testWidth = staminaMaxFillWidth * 0.25f;
        if (staminaFillBarLeftRect != null) staminaFillBarLeftRect.sizeDelta = new Vector2(testWidth, staminaFillHeight);
        if (staminaFillBarRightRect != null) staminaFillBarRightRect.sizeDelta = new Vector2(testWidth, staminaFillHeight);
        ForceShowStamina();
        Debug.Log($"UIManager: Test Stamina Fill 25% - Width set to {testWidth}");
    }

    // ===== GAME OVER TESTS =====
    [ContextMenu("Test: Show Win (Night 1)")]
    public void TestShowWin1()
    {
        gameOverTriggered = false; // Reset for testing
        ShowWin(1, "4:32 AM");
    }

    [ContextMenu("Test: Show Win (Night 2)")]
    public void TestShowWin2()
    {
        gameOverTriggered = false;
        ShowWin(2, "5:15 AM");
    }

    [ContextMenu("Test: Show Loss - Monster (Night 1)")]
    public void TestShowLossMonster1()
    {
        gameOverTriggered = false;
        ShowLoss_CaughtByMonster(1, "2:47 AM");
    }

    [ContextMenu("Test: Show Loss - Time Out Had Flowers")]
    public void TestShowLossTimeHadFlowers()
    {
        gameOverTriggered = false;
        ShowLoss_TimeRanOut(2, true);
    }

    [ContextMenu("Test: Show Loss - Time Out No Flowers")]
    public void TestShowLossTimeNoFlowers()
    {
        gameOverTriggered = false;
        ShowLoss_TimeRanOut(1, false);
    }

    [ContextMenu("Test: Hide Game Over Screen")]
    public void TestHideGameOverScreen()
    {
        HideGameOverScreen();
    }
}