using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{


    public Coroutine CrosshairCoroutine;

    [SerializeField] private Image _crosshair;
    [SerializeField] private Sprite[] _crosshairSprites;

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
    }

    public void ChangeCrossHair(int spriteIndex)
    {
        _crosshair.sprite = _crosshairSprites[spriteIndex];

        if(spriteIndex == 0)
        {
            _crosshair.rectTransform.sizeDelta = new Vector2(5f,5f);
        }
        else
        {
            _crosshair.rectTransform.sizeDelta = new Vector2(20f, 25f);
        }
    }

}
