using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// TimerUI - Countdown timer with Volume-based darkness effects
/// Features:
/// - Customizable start time
/// - Gradual darkness using Post-Processing Volume (after 25% time)
/// - Vignette intensifies in last 10 seconds
/// - Triggers loss screen when timer reaches 0
/// </summary>
public class TimerUI : MonoBehaviour
{
    // ==================== UI REFERENCES ====================
    [Header("=== UI ELEMENTS ===")]
    [Tooltip("The stone frame behind the timer")]
    public Image frameImage;

    [Tooltip("The timer text display")]
    public TextMeshProUGUI timerText;

    // ==================== TIMER SETTINGS ====================
    [Header("=== TIMER SETTINGS ===")]
    [Tooltip("Starting time in seconds (e.g., 120 = 2 minutes, 180 = 3 minutes)")]
    public float startTimeInSeconds = 120f;

    [Tooltip("Is the timer currently running?")]
    public bool isRunning = false;

    [Tooltip("Start timer automatically when game starts")]
    public bool autoStart = false;

    // ==================== POST-PROCESSING REFERENCES ====================
    [Header("=== POST-PROCESSING VOLUME ===")]
    [Tooltip("The Global Volume in your scene")]
    public Volume postProcessVolume;

    // ==================== DARKNESS EFFECT SETTINGS ====================
    [Header("=== GRADUAL DARKNESS (Post Exposure) ===")]
    [Tooltip("Enable gradual darkness as night progresses")]
    public bool useGradualDarkness = true;

    [Tooltip("When to start getting darker (0.25 = 25% time remaining)")]
    [Range(0f, 1f)]
    public float darknessStartPercent = 0.25f;

    [Tooltip("Starting Post Exposure value (your normal night setting)")]
    public float startPostExposure = -0.8f;

    [Tooltip("Ending Post Exposure value (very dark)")]
    public float endPostExposure = -3f;

    [Header("=== VIGNETTE EFFECT (Last 10 Seconds) ===")]
    [Tooltip("Enable vignette intensifying in last seconds")]
    public bool useVignetteEffect = true;

    [Tooltip("Seconds remaining when vignette effect starts")]
    public float vignetteEffectStartTime = 10f;

    [Tooltip("Starting vignette intensity (your normal setting)")]
    public float startVignetteIntensity = 0.4f;

    [Tooltip("Maximum vignette intensity at 0 seconds")]
    public float maxVignetteIntensity = 0.8f;

    [Tooltip("Starting vignette smoothness")]
    public float startVignetteSmoothness = 0.5f;

    [Tooltip("Ending vignette smoothness (tighter)")]
    public float endVignetteSmoothness = 0.2f;

    // ==================== STYLE SETTINGS ====================
    [Header("=== TEXT STYLE ===")]
    [Tooltip("Normal timer text color")]
    public Color normalTextColor = Color.white;

    [Tooltip("Warning text color (last 10 seconds)")]
    public Color warningTextColor = Color.red;

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
    private float originalVignetteIntensity;
    private float originalVignetteSmoothness;

    // Public event for when timer ends
    public delegate void TimerEndedHandler();
    public event TimerEndedHandler OnTimerEnded;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Initialize timer
        currentTime = startTimeInSeconds;
        initialTime = startTimeInSeconds;
        hasTriggeredLoss = false;

        // Apply frame tint
        if (frameImage != null)
        {
            frameImage.color = frameTint;
        }

        // Apply text color
        if (timerText != null)
        {
            timerText.color = normalTextColor;
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
            Debug.Log($"TimerUI: Initialized with {startTimeInSeconds} seconds ({FormatTime(startTimeInSeconds)})");
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
        UpdateGradualDarkness();
        UpdateVignetteEffect();

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
            postProcessVolume = FindObjectOfType<Volume>();

            if (postProcessVolume == null)
            {
                Debug.LogWarning("TimerUI: No Post-Processing Volume found! Darkness effects will not work.");
                return;
            }
        }

        // Get Color Adjustments effect
        if (postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            // Store original value
            originalPostExposure = colorAdjustments.postExposure.value;

            if (showDebugLogs)
            {
                Debug.Log($"TimerUI: Found Color Adjustments. Original Post Exposure: {originalPostExposure}");
            }
        }
        else
        {
            Debug.LogWarning("TimerUI: Color Adjustments not found in Volume! Add it for darkness effect.");
        }

        // Get Vignette effect
        if (postProcessVolume.profile.TryGet(out vignette))
        {
            // Store original values
            originalVignetteIntensity = vignette.intensity.value;
            originalVignetteSmoothness = vignette.smoothness.value;

            if (showDebugLogs)
            {
                Debug.Log($"TimerUI: Found Vignette. Original Intensity: {originalVignetteIntensity}");
            }
        }
        else
        {
            Debug.LogWarning("TimerUI: Vignette not found in Volume! Add it for edge darkness effect.");
        }
    }

    // ==================== DISPLAY ====================
    private void UpdateDisplay()
    {
        if (timerText == null) return;

        timerText.text = FormatTime(currentTime);

        // Change color in last 10 seconds
        if (currentTime <= vignetteEffectStartTime && currentTime > 0)
        {
            // Pulse between normal and warning color
            float pulse = (Mathf.Sin(Time.time * 5f) + 1f) / 2f;
            timerText.color = Color.Lerp(normalTextColor, warningTextColor, pulse);
        }
        else
        {
            timerText.color = normalTextColor;
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0}:{1:00}", minutes, seconds);
    }

    // ==================== GRADUAL DARKNESS (Post Exposure) ====================
    private void UpdateGradualDarkness()
    {
        if (!useGradualDarkness || colorAdjustments == null) return;

        float timePercent = currentTime / initialTime; // 1.0 = full time, 0.0 = no time

        // Only start darkening after reaching the threshold (e.g., 25% remaining)
        if (timePercent <= darknessStartPercent)
        {
            // Map from darknessStartPercent->0 to startPostExposure->endPostExposure
            float darknessProgress = 1f - (timePercent / darknessStartPercent); // 0 to 1
            float targetExposure = Mathf.Lerp(startPostExposure, endPostExposure, darknessProgress);

            colorAdjustments.postExposure.value = targetExposure;

            if (showDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"TimerUI: Darkness Progress: {darknessProgress:F2}, Post Exposure: {targetExposure:F2}");
            }
        }
        else
        {
            // Before darkness threshold, keep normal
            colorAdjustments.postExposure.value = startPostExposure;
        }
    }

    // ==================== VIGNETTE EFFECT (Last 10 Seconds) ====================
    private void UpdateVignetteEffect()
    {
        if (!useVignetteEffect || vignette == null) return;

        if (currentTime <= vignetteEffectStartTime && currentTime > 0)
        {
            // Calculate progress (0 at 10 seconds, 1 at 0 seconds)
            float progress = 1f - (currentTime / vignetteEffectStartTime);

            // Increase vignette intensity
            float targetIntensity = Mathf.Lerp(startVignetteIntensity, maxVignetteIntensity, progress);
            vignette.intensity.value = targetIntensity;

            // Decrease smoothness (tighter vignette)
            float targetSmoothness = Mathf.Lerp(startVignetteSmoothness, endVignetteSmoothness, progress);
            vignette.smoothness.value = targetSmoothness;
        }
        else if (currentTime > vignetteEffectStartTime)
        {
            // Not in last 10 seconds, keep normal
            vignette.intensity.value = startVignetteIntensity;
            vignette.smoothness.value = startVignetteSmoothness;
        }
    }

    // ==================== TIMER END ====================
    private void TimerEnded()
    {
        hasTriggeredLoss = true;
        isRunning = false;

        if (showDebugLogs)
        {
            Debug.Log("TimerUI: TIME'S UP! Triggering loss...");
        }

        // Maximum darkness
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = endPostExposure;
        }

        // Maximum vignette
        if (vignette != null)
        {
            vignette.intensity.value = maxVignetteIntensity;
            vignette.smoothness.value = endVignetteSmoothness;
        }

        // Trigger event (other scripts can listen to this)
        OnTimerEnded?.Invoke();

        // TODO: Call your GameManager to show loss screen
        // GameManager.Instance.ShowLossScreen();
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
            Debug.Log("TimerUI: Timer started!");
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
    /// Reset timer to starting time
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTimeInSeconds;
        initialTime = startTimeInSeconds;
        hasTriggeredLoss = false;
        isRunning = false;

        // Reset post-processing to original values
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = startPostExposure;
        }

        if (vignette != null)
        {
            vignette.intensity.value = startVignetteIntensity;
            vignette.smoothness.value = startVignetteSmoothness;
        }

        UpdateDisplay();

        if (showDebugLogs)
        {
            Debug.Log($"TimerUI: Timer reset to {FormatTime(startTimeInSeconds)}");
        }
    }

    /// <summary>
    /// Set a new starting time (in seconds) and reset
    /// </summary>
    public void SetStartTime(float seconds)
    {
        startTimeInSeconds = seconds;
        ResetTimer();
    }

    /// <summary>
    /// Set starting time using minutes and seconds
    /// </summary>
    public void SetStartTime(int minutes, int seconds)
    {
        startTimeInSeconds = (minutes * 60f) + seconds;
        ResetTimer();
    }

    /// <summary>
    /// Add time (can be negative to remove time)
    /// </summary>
    public void AddTime(float seconds)
    {
        currentTime = Mathf.Max(0f, currentTime + seconds);
        UpdateDisplay();

        if (showDebugLogs)
        {
            Debug.Log($"TimerUI: Added {seconds} seconds. Current: {FormatTime(currentTime)}");
        }
    }

    /// <summary>
    /// Get current time remaining
    /// </summary>
    public float GetTimeRemaining()
    {
        return currentTime;
    }

    /// <summary>
    /// Get time remaining as formatted string
    /// </summary>
    public string GetTimeRemainingFormatted()
    {
        return FormatTime(currentTime);
    }

    /// <summary>
    /// Check if time is up
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

    [ContextMenu("Set to 15 Seconds (Test Warning)")]
    public void TestSet15Seconds()
    {
        currentTime = 15f;
        initialTime = 60f; // So 25% threshold works
        isRunning = true;
    }

    [ContextMenu("Set to 30 Seconds (Test 25% Darkness)")]
    public void TestSet30Seconds()
    {
        currentTime = 30f;
        initialTime = 120f; // 30 is 25% of 120
        isRunning = true;
    }

    [ContextMenu("Set to 3 Minutes")]
    public void TestSet3Minutes()
    {
        SetStartTime(3, 0);
    }

    [ContextMenu("Set to 2 Minutes")]
    public void TestSet2Minutes()
    {
        SetStartTime(2, 0);
    }
}