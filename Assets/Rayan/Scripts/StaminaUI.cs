using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StaminaUI - Works with Unity 6 without requiring sprites!
/// Uses RectTransform scaling for the fill effect instead of Image.fillAmount
/// This approach works even when Source Image is empty (None)
/// </summary>
public class StaminaUI : MonoBehaviour
{
    // ==================== REFERENCES ====================
    [Header("=== REQUIRED REFERENCES ===")]
    [Tooltip("Reference to your PlayerMovement script")]
    public PlayerMovement playerMovement;

    [Header("=== UI ELEMENTS ===")]
    [Tooltip("The stone frame Image (your asset)")]
    public Image stoneFrame;

    [Tooltip("The LEFT fill bar RectTransform")]
    public RectTransform fillBarLeftRect;

    [Tooltip("The RIGHT fill bar RectTransform")]
    public RectTransform fillBarRightRect;

    [Tooltip("The LEFT fill bar Image (for color changes)")]
    public Image fillBarLeftImage;

    [Tooltip("The RIGHT fill bar Image (for color changes)")]
    public Image fillBarRightImage;

    [Tooltip("The dark background behind the fill")]
    public Image darkBackground;

    [Tooltip("Parent CanvasGroup for fading the entire UI")]
    public CanvasGroup staminaCanvasGroup;

    // ==================== SIZE SETTINGS ====================
    [Header("=== SIZE SETTINGS ===")]
    [Tooltip("Maximum width of each fill bar (when stamina is full)")]
    public float maxFillWidth = 190f;

    [Tooltip("Height of each fill bar")]
    public float fillHeight = 10f;

    // ==================== VISIBILITY SETTINGS ====================
    [Header("=== VISIBILITY SETTINGS ===")]
    [Tooltip("Stamina threshold to show the bar (default 18)")]
    public float showThreshold = 18f;

    [Tooltip("How fast the bar fades in/out")]
    public float fadeSpeed = 3f;

    // ==================== COLOR SETTINGS ====================
    [Header("=== COLOR SETTINGS ===")]
    [Tooltip("Enable color gradient (changes color as stamina depletes)")]
    public bool useColorGradient = true;

    [Tooltip("Color when stamina is FULL")]
    public Color fullStaminaColor = new Color(0.1f, 0.3f, 0.6f, 1f); // Dark blue

    [Tooltip("Color when stamina is EMPTY")]
    public Color emptyStaminaColor = new Color(0.05f, 0.1f, 0.25f, 1f); // Very dark blue

    [Tooltip("Tint color for the stone frame (make it darker)")]
    public Color stoneFrameTint = new Color(0.4f, 0.4f, 0.4f, 1f); // Darker gray tint

    [Tooltip("Color for the dark background")]
    public Color backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.95f); // Almost black

    // ==================== RECHARGE EFFECT SETTINGS ====================
    [Header("=== RECHARGE EFFECT ===")]
    [Tooltip("Enable pulsing effect when recharging (canSprint = false)")]
    public bool useRechargeEffect = true;

    [Tooltip("How fast the recharge pulse effect is")]
    public float pulseSpeed = 3f;

    [Tooltip("Color when pulsing (recharging) - brighter for visibility")]
    public Color rechargeColorBright = new Color(0.2f, 0.4f, 0.7f, 1f); // Brighter blue

    [Tooltip("Color when pulsing (recharging) - darker")]
    public Color rechargeColorDark = new Color(0.05f, 0.1f, 0.25f, 1f); // Darker blue

    // ==================== DEBUG ====================
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private bool isRecharging = false;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Initialize UI as invisible
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 0f;
            currentAlpha = 0f;
        }

        // Apply stone frame tint to make it darker
        if (stoneFrame != null)
        {
            stoneFrame.color = stoneFrameTint;
        }

        // Apply background color
        if (darkBackground != null)
        {
            darkBackground.color = backgroundColor;
        }

        // Apply initial colors to fill bars
        if (fillBarLeftImage != null)
        {
            fillBarLeftImage.color = fullStaminaColor;
        }
        if (fillBarRightImage != null)
        {
            fillBarRightImage.color = fullStaminaColor;
        }

        // Auto-find player if not assigned
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("StaminaUI: No PlayerMovement found! Please assign it in the Inspector.");
            }
        }

        if (showDebugLogs)
        {
            Debug.Log("StaminaUI: Initialized! Using RectTransform scaling method.");
        }
    }

    void Update()
    {
        if (playerMovement == null) return;

        // Get current stamina values from player
        float currentStamina = playerMovement.currentStamina;
        float maxStamina = playerMovement.maxStamina;

        // Update all UI systems
        UpdateVisibility(currentStamina, maxStamina);
        UpdateFillBars(currentStamina, maxStamina);
        UpdateColors(currentStamina, maxStamina);
    }

    // ==================== VISIBILITY SYSTEM ====================
    private void UpdateVisibility(float currentStamina, float maxStamina)
    {
        // Show bar when stamina drops below threshold
        if (currentStamina < showThreshold)
        {
            targetAlpha = 1f;
        }
        // Hide bar when stamina is full
        else if (currentStamina >= maxStamina)
        {
            targetAlpha = 0f;
        }

        // Smooth fade transition
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Apply alpha to canvas group
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = currentAlpha;
        }
    }

    // ==================== FILL BAR SYSTEM (Using RectTransform Width) ====================
    private void UpdateFillBars(float currentStamina, float maxStamina)
    {
        // Calculate fill percentage (0 to 1)
        float fillPercent = Mathf.Clamp01(currentStamina / maxStamina);

        // Calculate the width based on stamina
        float currentWidth = maxFillWidth * fillPercent;

        // Apply width to LEFT fill bar
        if (fillBarLeftRect != null)
        {
            fillBarLeftRect.sizeDelta = new Vector2(currentWidth, fillHeight);
        }

        // Apply width to RIGHT fill bar
        if (fillBarRightRect != null)
        {
            fillBarRightRect.sizeDelta = new Vector2(currentWidth, fillHeight);
        }

        // Debug log every second
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"StaminaUI: Stamina={currentStamina:F1}/{maxStamina}, Width={currentWidth:F1}/{maxFillWidth}");
        }
    }

    // ==================== COLOR SYSTEM ====================
    private void UpdateColors(float currentStamina, float maxStamina)
    {
        Color targetColor;

        // Check if in recharge mode (pulsing effect)
        if (isRecharging && useRechargeEffect)
        {
            // Pulsing effect: oscillate between bright and dark
            float pulse = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) / 2f; // 0 to 1
            targetColor = Color.Lerp(rechargeColorDark, rechargeColorBright, pulse);
        }
        // Normal gradient based on stamina level
        else if (useColorGradient)
        {
            float staminaPercent = currentStamina / maxStamina;
            targetColor = Color.Lerp(emptyStaminaColor, fullStaminaColor, staminaPercent);
        }
        // Single color mode
        else
        {
            targetColor = fullStaminaColor;
        }

        // Apply color to BOTH fill bars
        if (fillBarLeftImage != null)
        {
            fillBarLeftImage.color = targetColor;
        }

        if (fillBarRightImage != null)
        {
            fillBarRightImage.color = targetColor;
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Called by PlayerMovement when canSprint changes to false (stamina depleted)
    /// </summary>
    public void SetRecharging(bool recharging)
    {
        isRecharging = recharging;

        if (showDebugLogs)
        {
            if (recharging)
            {
                Debug.Log("StaminaUI: Recharge mode ON - pulsing effect active");
            }
            else
            {
                Debug.Log("StaminaUI: Recharge mode OFF");
            }
        }
    }

    /// <summary>
    /// Check if currently in recharge mode
    /// </summary>
    public bool IsRecharging()
    {
        return isRecharging;
    }

    /// <summary>
    /// Force show the stamina bar (useful for testing)
    /// </summary>
    [ContextMenu("Force Show Bar")]
    public void ForceShow()
    {
        targetAlpha = 1f;
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 1f;
            currentAlpha = 1f;
        }
        Debug.Log("StaminaUI: Force Show triggered");
    }

    /// <summary>
    /// Force hide the stamina bar
    /// </summary>
    [ContextMenu("Force Hide Bar")]
    public void ForceHide()
    {
        targetAlpha = 0f;
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 0f;
            currentAlpha = 0f;
        }
        Debug.Log("StaminaUI: Force Hide triggered");
    }

    /// <summary>
    /// Test the fill bars at 50%
    /// </summary>
    [ContextMenu("Test Fill 50%")]
    public void TestFill50()
    {
        float testWidth = maxFillWidth * 0.5f;
        if (fillBarLeftRect != null) fillBarLeftRect.sizeDelta = new Vector2(testWidth, fillHeight);
        if (fillBarRightRect != null) fillBarRightRect.sizeDelta = new Vector2(testWidth, fillHeight);
        ForceShow();
        Debug.Log($"StaminaUI: Test Fill 50% - Width set to {testWidth}");
    }

    /// <summary>
    /// Test the fill bars at 25%
    /// </summary>
    [ContextMenu("Test Fill 25%")]
    public void TestFill25()
    {
        float testWidth = maxFillWidth * 0.25f;
        if (fillBarLeftRect != null) fillBarLeftRect.sizeDelta = new Vector2(testWidth, fillHeight);
        if (fillBarRightRect != null) fillBarRightRect.sizeDelta = new Vector2(testWidth, fillHeight);
        ForceShow();
        Debug.Log($"StaminaUI: Test Fill 25% - Width set to {testWidth}");
    }
}