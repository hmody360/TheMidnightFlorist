using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private Slider TimerSlider;
    [SerializeField] private Image WrapperTypeHolder;
    [SerializeField] private Image[] FlowerTypeList;
    [SerializeField] private Image SprayTypeHolder;
    [SerializeField] private Image CardTypeHolder;
    [SerializeField] private GameObject OrderStatsHolder;

    public void UpdateTimerSlider(float _timer, float _MaxTimer)
    {
        TimerSlider.value = _timer / _MaxTimer;
    }

    public void setOrder(Sprite WrapperIcon, Sprite[] FlowerIconList, Sprite SprayIcon, Sprite CardIcon)
    {
        WrapperTypeHolder.sprite = WrapperIcon;

        for (int i = 0; i < FlowerTypeList.Length; i++)
        {
            FlowerTypeList[i].sprite = FlowerIconList[i];
        }

        SprayTypeHolder.sprite = SprayIcon;
        CardTypeHolder.sprite = CardIcon;
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
        _animator.SetTrigger("ShowOrder");
    }

    public void HideOrderPanel()
    {
        _animator.SetTrigger("HideOrder");
    }
}
