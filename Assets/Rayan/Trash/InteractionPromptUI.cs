using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static InteractionPromptUI Instance { get; private set; }

    // ==================== UI REFERENCES ====================
    [Header("=== UI REFERENCES ===")]
    [Tooltip("The main panel/container")]
    public GameObject promptPanel;

    [Tooltip("The button icon image (E key, mouse click, etc.)")]
    public Image iconImage;

    [Tooltip("The action text")]
    public TextMeshProUGUI promptText;

    [Tooltip("Background panel (optional)")]
    public Image backgroundImage;

    // ==================== ICON SETTINGS ====================
    [Header("=== ICON SETTINGS ===")]
    [Tooltip("The icon sprite to show")]
    public Sprite buttonIcon;

    [Tooltip("Icon size")]
    public Vector2 iconSize = new Vector2(40f, 40f);

    // ==================== TEXT SETTINGS ====================
    [Header("=== TEXT SETTINGS ===")]
    [Tooltip("Text color")]
    public Color textColor = Color.white;

    [Tooltip("Text size")]
    public float textSize = 24f;

    // ==================== BACKGROUND SETTINGS ====================
    [Header("=== BACKGROUND SETTINGS ===")]
    [Tooltip("Use background panel")]
    public bool useBackground = true;

    [Tooltip("Background color")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);

    [Tooltip("Background padding")]
    public Vector2 backgroundPadding = new Vector2(20f, 10f);

    // ==================== FADE SETTINGS ====================
    [Header("=== FADE SETTINGS ===")]
    [Tooltip("Enable fade in/out")]
    public bool useFade = true;

    [Tooltip("Fade speed")]
    public float fadeSpeed = 8f;

    // ==================== PRIVATE VARIABLES ====================
    private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;
    private bool isShowing = false;
    private string currentActionName = "";

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
            Debug.LogWarning("InteractionPromptUI: Multiple instances detected!");
        }

        // Get or add canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        // Apply settings
        ApplySettings();

        // Start hidden
        HidePromptImmediate();
    }

    void Update()
    {
        // Handle fade animation
        if (useFade)
        {
            UpdateFade();
        }
    }

    // ==================== SETTINGS ====================
    private void ApplySettings()
    {
        // Apply icon
        if (iconImage != null && buttonIcon != null)
        {
            iconImage.sprite = buttonIcon;
            iconImage.rectTransform.sizeDelta = iconSize;
        }

        // Apply text settings
        if (promptText != null)
        {
            promptText.color = textColor;
            promptText.fontSize = textSize;
        }

        // Apply background
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(useBackground);
            if (useBackground)
            {
                backgroundImage.color = backgroundColor;
            }
        }
    }

    // ==================== FADE ====================
    private void UpdateFade()
    {
        if (canvasGroup == null) return;

        // Smoothly fade to target
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Disable panel when fully hidden
        if (canvasGroup.alpha <= 0f && promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Shows the interaction prompt with the given action name
    /// </summary>
    /// <param name="actionName">Text to display (e.g., "Pick Up Flower")</param>
    public void ShowPrompt(string actionName)
    {
        if (isShowing && currentActionName == actionName) return;

        isShowing = true;
        currentActionName = actionName;

        // Enable panel
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
        }

        // Set text
        if (promptText != null)
        {
            promptText.text = actionName;
        }

        // Start fade in
        if (useFade)
        {
            targetAlpha = 1f;
        }
        else
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }

    /// <summary>
    /// Hides the interaction prompt
    /// </summary>
    public void HidePrompt()
    {
        if (!isShowing) return;

        isShowing = false;
        currentActionName = "";

        // Start fade out
        if (useFade)
        {
            targetAlpha = 0f;
        }
        else
        {
            HidePromptImmediate();
        }
    }

    /// <summary>
    /// Immediately hides the prompt without fade
    /// </summary>
    public void HidePromptImmediate()
    {
        isShowing = false;
        currentActionName = "";
        targetAlpha = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Check if prompt is currently showing
    /// </summary>
    public bool IsShowing()
    {
        return isShowing;
    }

    /// <summary>
    /// Change the button icon at runtime
    /// </summary>
    public void SetIcon(Sprite newIcon)
    {
        buttonIcon = newIcon;
        if (iconImage != null)
        {
            iconImage.sprite = newIcon;
        }
    }

    /// <summary>
    /// Change text color at runtime
    /// </summary>
    public void SetTextColor(Color color)
    {
        textColor = color;
        if (promptText != null)
        {
            promptText.color = color;
        }
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Show Prompt")]
    public void TestShowPrompt()
    {
        ShowPrompt("Pick Up Flower");
    }

    [ContextMenu("Test: Hide Prompt")]
    public void TestHidePrompt()
    {
        HidePrompt();
    }

    [ContextMenu("Test: Apply Settings")]
    public void TestApplySettings()
    {
        ApplySettings();
    }
}