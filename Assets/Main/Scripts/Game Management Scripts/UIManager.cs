using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{


    public Coroutine CrosshairCoroutine;


    [Header("HUD Main")]
    [SerializeField] private GameObject _HUDPanel;
    [SerializeField] private Image _crosshair;
    [SerializeField] private Sprite[] _crosshairSprites;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private bool isDenyPromptShowing;

    [Header("Time Related")]
    [SerializeField] private Text _dayCounterText;
    [SerializeField] private GameObject _sunMoonImageObj;

    [Header("Status Related")]
    [SerializeField] private Text _currentTaskText;
    [SerializeField] private Text _shopStatusText;

    [Header("Stats Related")]
    [SerializeField] private GameObject _statsPanel;
    [SerializeField] private Text _currentNectarCoinsText;
    [SerializeField] private Text _quotaText;
    [SerializeField] private Text _customerCounterText;

    public static UIManager instance;

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
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        ChangeCrossHair(0);
        _promptText.text = "";
    }

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

}
