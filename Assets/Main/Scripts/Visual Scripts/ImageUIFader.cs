using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageUIFader : MonoBehaviour
{
    private Image[] _imgList;
    private Coroutine _fadeCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        _imgList = GetComponentsInChildren<Image>();
    }

    private void OnEnable()
    {
        if (_imgList != null)
            Fade(true);
    }

    public void Hide()
    {
        if (_imgList != null)
            Fade(false);
    }

    private void Fade(bool isFadeIn)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeUIAnim(isFadeIn));
    }


    private IEnumerator FadeUIAnim(bool isFadeIn)
    {
        if (isFadeIn)
        {
            for (float i = 0; i <= 1; i += Time.deltaTime)
            {
                setAlpha(i);
                yield return null;
            }
            setAlpha(1);
        }
        else
        {
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                setAlpha(i);
                yield return null;
            }
            setAlpha(0);
                gameObject.SetActive(false);
            
        }
    }

    private void setAlpha(float alpha)
    {
        foreach(Image img in _imgList)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

    }
}
