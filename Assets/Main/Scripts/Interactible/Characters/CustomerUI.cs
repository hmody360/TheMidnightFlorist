using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomerUI : MonoBehaviour
{
    private Animator _animator;
    private Camera _mainCamera;

    [SerializeField] private Slider TimerSlider;
    [SerializeField] private Image WrapperTypeHolder;
    [SerializeField] private Image[] FlowerTypeList;
    [SerializeField] private Image SprayTypeHolder;
    [SerializeField] private Image CardTypeHolder;
    [SerializeField] private GameObject OrderStatsHolder;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        transform.rotation = _mainCamera.transform.rotation;
    }
    public void UpdateTimerSlider(float _timer, float _MaxTimer)
    {
        TimerSlider.value = _timer / _MaxTimer;
    }

    public void setOrder(Wrapper wrapper, List<FlowerObj> flowerIconList, Spray spray, Card card)
    {
        WrapperTypeHolder.sprite = wrapper.Icon;

        for (int i = 0; i < flowerIconList.Count; i++)
        {
            FlowerTypeList[i].sprite = flowerIconList[i].Icon;
        }
        if(spray != null)
        {
            SprayTypeHolder.sprite = spray.Icon;
        }
        
        if(card != null)
        {
            CardTypeHolder.sprite = card.Icon;
        }
        
    }

    public void ChangeOrderStaus(bool isSatisfied)
    {
        Animator statsHolderAnimator = OrderStatsHolder.GetComponent<Animator>();
        if (isSatisfied)
        {
            statsHolderAnimator.SetTrigger("CorrectOrder");
        }
        else
        {
            statsHolderAnimator.SetTrigger("WrongOrder");
        }
    }

    public void ShowOrderPanel()
    {
        _animator.SetTrigger("ShowTrigger");
        TimerSlider.gameObject.SetActive(true);
    }

    public void HideOrderPanel()
    {
        _animator.SetTrigger("HideTrigger");
        TimerSlider.gameObject.SetActive(false);
    }
}
