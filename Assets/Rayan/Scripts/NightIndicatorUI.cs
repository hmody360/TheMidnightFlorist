using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class NightIndicatorUI : MonoBehaviour
{
    // ==================== REFERENCES ====================
    [Header("=== UI ELEMENTS ===")]
    [Tooltip("The moon icon image")]
    public Image moonIcon;

    [Tooltip("The text showing 'Night X'")]
    public TextMeshProUGUI nightText;

    // ==================== SETTINGS ====================
    [Header("=== SETTINGS ===")]
    [Tooltip("Current night number (1, 2, or 3)")]
    public int currentNight = 1;

    [Tooltip("Text format (use {0} for night number)")]
    public string textFormat = "Night {0}";

    [Header("=== STYLE ===")]
    [Tooltip("Text color")]
    public Color textColor = Color.white;

    [Tooltip("Moon icon tint color (white = no tint)")]
    public Color moonTint = Color.white;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Apply colors
        if (nightText != null)
        {
            nightText.color = textColor;
        }

        if (moonIcon != null)
        {
            moonIcon.color = moonTint;
        }

        // Update display
        UpdateDisplay();
    }

    // ==================== DISPLAY ====================
    /// <summary>
    /// Updates the night text display
    /// </summary>
    public void UpdateDisplay()
    {
        if (nightText != null)
        {
            nightText.text = string.Format(textFormat, currentNight);
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Set the current night number (call this from GameManager)
    /// </summary>
    /// <param name="night">Night number (1, 2, or 3)</param>
    public void SetNight(int night)
    {
        currentNight = Mathf.Clamp(night, 1, 99); // Allow up to 99 nights
        UpdateDisplay();
        Debug.Log($"NightIndicatorUI: Set to Night {currentNight}");
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
        UpdateDisplay();
        Debug.Log($"NightIndicatorUI: Advanced to Night {currentNight}");
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Set Night 1")]
    public void TestNight1()
    {
        SetNight(1);
    }

    [ContextMenu("Set Night 2")]
    public void TestNight2()
    {
        SetNight(2);
    }

    [ContextMenu("Set Night 3")]
    public void TestNight3()
    {
        SetNight(3);
    }
}