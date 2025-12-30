using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class TimerUI : MonoBehaviour
{
    // ==================== UI REFERENCES ====================
    [Header("=== UI ELEMENTS ===")]
    [Tooltip("The stone frame behind the timer")]
    public Image frameImage;

    [Tooltip("The timer text display")]
    public TextMeshProUGUI timerText;

    [Tooltip("Reference to call the loss screen")]
    public GameOverUI gameOverUI;

    [Tooltip("Reference to know what current night is")]
    public NightIndicatorUI NightIndicatorUI;

    // ==================== TIMER SETTINGS ====================
    [Header("=== TIMER SETTINGS ===")]
    [Tooltip("Total game time in seconds (how long from 12AM to 6AM in real time)")]
    public float totalGameTimeSeconds = 360f; // 6 minutes = 6 hours in game

    [Tooltip("Is the timer currently running?")]
    public bool isRunning = false;

    [Tooltip("Start timer automatically when game starts")]
    public bool autoStart = false;

    // ==================== POST-PROCESSING REFERENCES ====================
    [Header("=== POST-PROCESSING VOLUME ===")]
    [Tooltip("The Global Volume in your scene")]
    public Volume postProcessVolume;

    // ==================== DAWN TIMING ====================
    [Header("=== DAWN TIMING ===")]
    [Tooltip("When dawn effects start (0.916 = 91.6% = 5:30 AM, 0.75 = 75% = 4:30 AM)")]
    [Range(0f, 1f)]
    public float dawnStartPercent = 0.916f; // 5:30 AM = 5.5/6 = 91.6%

    // ==================== POST EXPOSURE (BRIGHTNESS) ====================
    [Header("=== BRIGHTNESS (Post Exposure) ===")]
    [Tooltip("Enable brightness changes")]
    public bool useBrightnessEffect = true;

    [Tooltip("Post Exposure at night (dark)")]
    public float nightPostExposure = -0.8f;

    [Tooltip("Post Exposure at 6:00 AM (dawn complete)")]
    public float dawnPostExposure = 0.5f;

    // ==================== COLOR FILTER ====================
    [Header("=== COLOR FILTER (Tint) ===")]
    [Tooltip("Enable color filter changes")]
    public bool useColorFilter = true;

    [Tooltip("Color tint at night (dark blue #1A2A4A)")]
    public Color nightColorFilter = new Color(0.102f, 0.165f, 0.290f, 1f); // #1A2A4A

    [Tooltip("Color tint at 6:00 AM (warm peach #FFE4C4)")]
    public Color dawnColorFilter = new Color(1f, 0.894f, 0.769f, 1f); // #FFE4C4

    // ==================== SATURATION ====================
    [Header("=== SATURATION (Color Intensity) ===")]
    [Tooltip("Enable saturation changes")]
    public bool useSaturation = true;

    [Tooltip("Saturation at night (less colorful)")]
    public float nightSaturation = -25f;

    [Tooltip("Saturation at 6:00 AM")]
    public float dawnSaturation = 10f;

    // ==================== CONTRAST ====================
    [Header("=== CONTRAST ===")]
    [Tooltip("Enable contrast changes")]
    public bool useContrast = true;

    [Tooltip("Contrast at night")]
    public float nightContrast = 15f;

    [Tooltip("Contrast at 6:00 AM")]
    public float dawnContrast = 20f;

    // ==================== VIGNETTE ====================
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

    // ==================== TEXT STYLE ====================
    [Header("=== TEXT STYLE ===")]
    [Tooltip("Timer text color")]
    public Color textColor = Color.white;

    [Tooltip("Frame tint color")]
    public Color frameTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    // ==================== DEBUG ====================
    [Header("=== DEBUG ===")]
    [Tooltip("Enable debug logs")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private float currentTime;
    private float initialTime;
    private bool hasTriggeredLoss = false;

    // Post-processing effect references
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    // Store original values to restore later
    private float originalPostExposure;
    private Color originalColorFilter;
    private float originalSaturation;
    private float originalContrast;
    private float originalVignetteIntensity;
    private float originalVignetteSmoothness;

    // Public event for when timer ends
    public delegate void TimerEndedHandler();
    public event TimerEndedHandler OnTimerEnded;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Initialize timer
        currentTime = totalGameTimeSeconds;
        initialTime = totalGameTimeSeconds;
        hasTriggeredLoss = false;

        // Apply frame tint
        if (frameImage != null)
        {
            frameImage.color = frameTint;
        }

        // Apply text color
        if (timerText != null)
        {
            timerText.color = textColor;
        }

        // Get post-processing effects from Volume
        SetupPostProcessing();

        // Auto start if enabled
        if (autoStart)
        {
            StartTimer();
        }

        UpdateDisplay();

        if (showDebugLogs)
        {
            float dawnStartHour = dawnStartPercent * 6f; // Convert to hours
            Debug.Log($"TimerUI: Initialized. Dawn starts at {dawnStartHour:F1} hours ({dawnStartPercent:P0})");
        }
    }

    void Update()
    {
        if (!isRunning) return;

        // Count down
        currentTime -= Time.deltaTime;

        // Update display
        UpdateDisplay();

        // Update effects
        UpdateDawnEffects();

        // Check for timer end
        if (currentTime <= 0f && !hasTriggeredLoss)
        {
            currentTime = 0f;
            TimerEnded();
        }
    }

    // ==================== POST-PROCESSING SETUP ====================
    private void SetupPostProcessing()
    {
        if (postProcessVolume == null)
        {
            // Try to find Global Volume in scene
            postProcessVolume = FindFirstObjectByType<Volume>();

            if (postProcessVolume == null)
            {
                Debug.LogWarning("TimerUI: No Post-Processing Volume found! Dawn effects will not work.");
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
                Debug.Log($"TimerUI: Found Color Adjustments.");
            }
        }
        else
        {
            Debug.LogWarning("TimerUI: Color Adjustments not found in Volume! Add it for dawn effects.");
        }

        // Get Vignette effect
        if (postProcessVolume.profile.TryGet(out vignette))
        {
            // Store original values
            originalVignetteIntensity = vignette.intensity.value;
            originalVignetteSmoothness = vignette.smoothness.value;

            if (showDebugLogs)
            {
                Debug.Log($"TimerUI: Found Vignette.");
            }
        }
        else
        {
            Debug.LogWarning("TimerUI: Vignette not found in Volume! Add it for vignette fade effect.");
        }
    }

    // ==================== DISPLAY (12:00 AM to 6:00 AM) ====================
    private void UpdateDisplay()
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

    // ==================== DAWN EFFECTS ====================
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
                Debug.Log($"TimerUI: Dawn {dawnProgress:P0} - Exposure: {colorAdjustments?.postExposure.value:F2}");
            }
        }
        // ===== NIGHT (Before dawn starts) =====
        else
        {
            ApplyNightEffects();
        }
    }

    // ==================== APPLY NIGHT EFFECTS ====================
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

    // ==================== APPLY DAWN EFFECTS ====================
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

    // ==================== TIMER END ====================
    private void TimerEnded()
    {
        hasTriggeredLoss = true;
        isRunning = false;

        if (showDebugLogs)
        {
            Debug.Log("TimerUI: 6:00 AM reached! LOSS triggered!");
        }

        // Apply full dawn effects
        ApplyDawnEffects(1f);

        // Trigger event (other scripts can listen to this)
        OnTimerEnded?.Invoke();

        // Find GameOverUI and show loss
        if (gameOverUI != null)
        {
            gameOverUI.ShowLoss_TimeRanOut(NightIndicatorUI.GetNight());
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Start the timer countdown
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        hasTriggeredLoss = false;

        if (showDebugLogs)
        {
            Debug.Log("TimerUI: Timer started! Time is 12:00 AM");
        }
    }

    /// <summary>
    /// Pause the timer
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;

        if (showDebugLogs)
        {
            Debug.Log("TimerUI: Timer paused.");
        }
    }

    /// <summary>
    /// Resume the timer
    /// </summary>
    public void ResumeTimer()
    {
        if (currentTime > 0)
        {
            isRunning = true;

            if (showDebugLogs)
            {
                Debug.Log("TimerUI: Timer resumed.");
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
        hasTriggeredLoss = false;
        isRunning = false;

        // Reset to night values
        ApplyNightEffects();

        UpdateDisplay();

        if (showDebugLogs)
        {
            Debug.Log("TimerUI: Timer reset to 12:00 AM");
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
    /// Example: 0.916 = 5:30 AM, 0.75 = 4:30 AM
    /// </summary>
    public void SetDawnStartPercent(float percent)
    {
        dawnStartPercent = Mathf.Clamp01(percent);

        if (showDebugLogs)
        {
            float dawnStartHour = dawnStartPercent * 6f;
            Debug.Log($"TimerUI: Dawn now starts at {dawnStartHour:F1} hours ({dawnStartPercent:P0})");
        }
    }

    /// <summary>
    /// Set dawn start time using game hour (0-6)
    /// Example: 5.5 = 5:30 AM, 4.5 = 4:30 AM
    /// </summary> 
    public void SetDawnStartHour(float hour)
    {
        dawnStartPercent = Mathf.Clamp01(hour / 6f);

        if (showDebugLogs)
        {
            Debug.Log($"TimerUI: Dawn now starts at {hour:F1}:00 AM ({dawnStartPercent:P0})");
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

    // ==================== CLEANUP ====================
    private void OnDestroy()
    {
        // Restore original values when destroyed (important for editor)
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

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Start Timer")]
    public void TestStartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    [ContextMenu("Pause Timer")]
    public void TestPauseTimer()
    {
        PauseTimer();
    }

    [ContextMenu("Set to Dawn Start Time")]
    public void TestSetToDawnStart()
    {
        float remainingPercent = 1f - dawnStartPercent;
        currentTime = initialTime * remainingPercent;
        isRunning = true;
        Debug.Log($"TimerUI: Set to dawn start ({dawnStartPercent:P0} passed)");
    }

    [ContextMenu("Set to 5:45 AM")]
    public void TestSet545AM()
    {
        // 5:45 AM = 5.75/6 = 95.8%
        currentTime = initialTime * 0.042f; // 4.2% remaining
        isRunning = true;
        Debug.Log("TimerUI: Set to 5:45 AM");
    }

    [ContextMenu("Set Game Time: 6 Minutes")]
    public void TestSet6Minutes()
    {
        SetTotalGameTimeMinutes(6f);
    }

    [ContextMenu("Set Game Time: 3 Minutes")]
    public void TestSet3Minutes()
    {
        SetTotalGameTimeMinutes(3f);
    }

    [ContextMenu("Set Game Time: 1 Minute (Fast Test)")]
    public void TestSet1Minute()
    {
        SetTotalGameTimeMinutes(1f);
    }

    [ContextMenu("Preview: Night Settings")]
    public void PreviewNight()
    {
        SetupPostProcessing();
        ApplyNightEffects();
        Debug.Log("TimerUI: Preview - Night settings applied");
    }

    [ContextMenu("Preview: Dawn 50%")]
    public void PreviewDawn50()
    {
        SetupPostProcessing();
        ApplyDawnEffects(0.5f);
        Debug.Log("TimerUI: Preview - Dawn 50% applied");
    }

    [ContextMenu("Preview: Dawn 100% (6:00 AM)")]
    public void PreviewDawn100()
    {
        SetupPostProcessing();
        ApplyDawnEffects(1f);
        Debug.Log("TimerUI: Preview - Dawn 100% (6:00 AM) applied");
    }
}