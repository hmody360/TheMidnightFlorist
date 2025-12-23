using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{


    public Coroutine CrosshairCoroutine;


    [Header("HUD")]
    [SerializeField] private Image _crosshair;
    [SerializeField] private Sprite[] _crosshairSprites;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private bool isDenyPromptShowing;

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

    public void setPromptText(string promptText, Color color, bool isDenyPrompt = false)
    {
        if (isDenyPromptShowing)
        {
            return;
        }
        if (isDenyPrompt)
        {
            StartCoroutine(DelayDenyPrompt());
        }
        _promptText.color = color;
        _promptText.text = promptText;
    }

    IEnumerator DelayDenyPrompt()
    {
            isDenyPromptShowing = true;
            yield return new WaitForSeconds(2f);
            isDenyPromptShowing = false;
    }

}
