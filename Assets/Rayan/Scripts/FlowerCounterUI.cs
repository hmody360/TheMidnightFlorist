using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlowerCounterUI : MonoBehaviour
{
    // ==================== REFERENCES ====================
    [Header("=== UI ELEMENTS ===")]
    [Tooltip("The flower/wreath background image")]
    public Image flowerImage;

    [Tooltip("The text showing the number of flowers remaining")]
    public TextMeshProUGUI flowerCountText;

    // ==================== SETTINGS ====================
    [Header("=== SETTINGS ===")]
    [Tooltip("Total flowers to collect this night")]
    public int totalFlowers = 4;

    [Tooltip("Current flowers remaining (updated by game)")]
    public int flowersRemaining = 4;

    [Header("=== STYLE SETTINGS ===")]
    [Tooltip("Color of the count text")]
    public Color textColor = Color.white;

    [Tooltip("Enable text outline for better visibility")]
    public bool useOutline = true;

    [Tooltip("Outline color")]
    public Color outlineColor = Color.black;

    [Tooltip("Outline thickness")]
    public float outlineThickness = 0.2f;

    [Header("=== OPTIONAL EFFECTS ===")]
    [Tooltip("Pulse effect when flower count changes")]
    public bool usePulseEffect = true;

    [Tooltip("How fast the pulse effect is")]
    public float pulseSpeed = 5f;

    [Tooltip("How much the text scales during pulse")]
    public float pulseScale = 1.3f;

    // ==================== PRIVATE VARIABLES ====================
    private bool isPulsing = false;
    private float pulseTimer = 0f;
    private Vector3 originalTextScale;
    private int lastFlowerCount;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Store original scale
        if (flowerCountText != null)
        {
            originalTextScale = flowerCountText.transform.localScale;

            // Apply text color
            flowerCountText.color = textColor;

            // Apply outline if enabled
            if (useOutline)
            {
                flowerCountText.outlineColor = outlineColor;
                flowerCountText.outlineWidth = outlineThickness;
            }
        }

        // Initialize
        lastFlowerCount = flowersRemaining;
        UpdateDisplay();
    }

    void Update()
    {
        // Check if flower count changed
        if (flowersRemaining != lastFlowerCount)
        {
            lastFlowerCount = flowersRemaining;
            UpdateDisplay();

            // Trigger pulse effect
            if (usePulseEffect)
            {
                StartPulse();
            }
        }

        // Handle pulse animation
        if (isPulsing)
        {
            UpdatePulse();
        }
    }

    // ==================== DISPLAY ====================
    /// <summary>
    /// Updates the flower count display
    /// </summary>
    public void UpdateDisplay()
    {
        if (flowerCountText != null)
        {
            flowerCountText.text = flowersRemaining.ToString();
        }
    }

    // ==================== PULSE EFFECT ====================
    private void StartPulse()
    {
        isPulsing = true;
        pulseTimer = 0f;
    }

    private void UpdatePulse()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;

        if (pulseTimer >= Mathf.PI) // One complete pulse cycle
        {
            isPulsing = false;
            if (flowerCountText != null)
            {
                flowerCountText.transform.localScale = originalTextScale;
            }
            return;
        }

        // Sine wave for smooth pulse
        float scale = 1f + (Mathf.Sin(pulseTimer) * (pulseScale - 1f));

        if (flowerCountText != null)
        {
            flowerCountText.transform.localScale = originalTextScale * scale;
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Set the total flowers for this night
    /// </summary>
    public void SetTotalFlowers(int total)
    {
        totalFlowers = total;
        flowersRemaining = total;
        UpdateDisplay();
    }

    /// <summary>
    /// Called when player collects a flower
    /// </summary>
    public void CollectFlower()
    {
        if (flowersRemaining > 0)
        {
            flowersRemaining--;
            Debug.Log($"FlowerCounterUI: Flower collected! {flowersRemaining} remaining.");
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
    /// Reset for new night
    /// </summary>
    public void ResetForNewNight(int flowerCount)
    {
        totalFlowers = flowerCount;
        flowersRemaining = flowerCount;
        UpdateDisplay();
        Debug.Log($"FlowerCounterUI: Reset for new night with {flowerCount} flowers.");
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test Collect Flower")]
    public void TestCollectFlower()
    {
        CollectFlower();
    }

    [ContextMenu("Reset to 4 Flowers")]
    public void TestReset4()
    {
        ResetForNewNight(4);
    }

    [ContextMenu("Reset to 3 Flowers")]
    public void TestReset3()
    {
        ResetForNewNight(3);
    }
}