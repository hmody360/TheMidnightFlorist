using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Day : MonoBehaviour
{


    public Coroutine CrosshairCoroutine;


    [Header("HUD Main")]
    [SerializeField] private GameObject _HUDPanel;
    [SerializeField] private Image _crosshair;
    [SerializeField] private Sprite[] _crosshairSprites;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private bool isDenyPromptShowing;

    [Header("Time Related")]
    [SerializeField] private TextMeshProUGUI _dayCounterText;
    [SerializeField] private GameObject _timeDisplayerObj;

    [Header("Status Related")]
    [SerializeField] private TextMeshProUGUI _currentTaskText;
    [SerializeField] private TextMeshProUGUI _shopStatusText;

    [Header("Stats Related")]
    [SerializeField] private GameObject _statsPanel;
    [SerializeField] private TextMeshProUGUI _currentNectarCoinsText;
    [SerializeField] private TextMeshProUGUI _quotaText;
    [SerializeField] private TextMeshProUGUI _customerCounterText;

    [Header("GameOver Related")]
    [SerializeField] private GameObject _gameOverPanel;

    public static UIManager_Day instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        ChangeCrossHair(0);
        _promptText.text = "";
    }

    // Crosshair and Prompt
    public void ChangeCrossHair(int spriteIndex)
    {
        _crosshair.sprite = _crosshairSprites[spriteIndex];

        if (spriteIndex == 0)
        {
            _crosshair.rectTransform.sizeDelta = new Vector2(5f, 5f);
        }
        else
        {
            _crosshair.rectTransform.sizeDelta = new Vector2(20f, 25f);
        }
    }

    public void setPromptText(string promptText, Color color, bool isTimedPrompt = false)
    {
        if (isDenyPromptShowing)
        {
            return;
        }
        if (isTimedPrompt)
        {
            StartCoroutine(DelayTimedPrompt());
        }
        _promptText.color = color;
        _promptText.text = promptText;
    }

    IEnumerator DelayTimedPrompt()
    {
        isDenyPromptShowing = true;
        yield return new WaitForSeconds(2f);
        isDenyPromptShowing = false;
    }

    // HUD

    public void ShowGameHUD()
    {
        if (_HUDPanel != null)
        {
            _HUDPanel.SetActive(true);
        }
    }

    public void HideGameHUD()
    {
        if (_HUDPanel != null)
        {
            _HUDPanel.SetActive(false);
        }
    }

    public void SetDayCounterText(int currentDay)
    {
        if (_dayCounterText != null)
        {
            _dayCounterText.text = "Day: " + currentDay;
        }
    }

    public void SetTimeDisplayer(bool isCurrentlyDay)
    {
        if (_timeDisplayerObj != null)
        {
            if (isCurrentlyDay)
            {
                _timeDisplayerObj.GetComponent<Animator>().SetTrigger("ChangeToDay");
            }
            else
            {
                _timeDisplayerObj.GetComponent<Animator>().SetTrigger("ChangeToNight");
            }
        }
    }

    public void SetTaskText(string taskText)
    {
        if (_currentTaskText != null)
        {
            _currentTaskText.text = taskText;
        }
    }

    public void SetShopStatus(bool isShopOpen)
    {
        if (_shopStatusText != null)
        {
            if (isShopOpen)
            {
                _shopStatusText.color = Color.green;
                _shopStatusText.text = "Open";
            }
            else
            {
                _shopStatusText.color = Color.red;
                _shopStatusText.text = "Closed";
            }
        }
    }

    public void ShowStatsPanel()
    {
        if (_statsPanel != null)
        {
            _statsPanel.SetActive(true);
        }
    }

    public void HideStatsPanel()
    {
        if (_statsPanel != null)
        {
            _statsPanel.SetActive(true);
        }
    }

    public void UpdateNectarCoinsText(float nectarCoins)
    {
        if (_currentNectarCoinsText != null)
        {
            _currentNectarCoinsText.text = nectarCoins.ToString();
        }
    }

    public void UpdateQuoutaText(float currentQuota, float totalQuota)
    {
        if (_quotaText != null)
        {
            _quotaText.text = currentQuota + " / " + totalQuota;
        }
    }

    public void UpdateCustomerCountText(int customersLeaved, int totalCustomers)
    {
        if (_customerCounterText != null)
        {
            _customerCounterText.text = customersLeaved + " / " + totalCustomers;
        }
    }

    public void ShowGameOverPanel()
    {
        if(_gameOverPanel  != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

}
