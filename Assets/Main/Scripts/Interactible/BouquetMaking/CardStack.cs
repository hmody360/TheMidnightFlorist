using UnityEngine;

public class CardStack : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;

    [SerializeField] private Card _containedCard;
    [SerializeField] private AudioClip[] _audioClips;
    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ActionName = "Add " + _containedCard.Name + " " + _containedCard.GetCardType() + " Card";
    }

    public void Interact()
    {
        GameObject currentBouquet = GameObject.FindGameObjectWithTag("CurrentBouquet");

        if (currentBouquet == null || currentBouquet.GetComponent<BouquetHolder>().GetWrapper() == null || currentBouquet.GetComponent<BouquetHolder>().GetFlowerList().Count == 0)
        {
            UIManager.instance.setPromptText("Add Wrapper and Flower First!", Color.red, true);
            _audioSource.PlayOneShot(_audioClips[1]);
            return;
        }

        bool isCardAdded = GameObject.FindGameObjectWithTag("CurrentBouquet").GetComponent<BouquetHolder>().AddCard(_containedCard);

        if (isCardAdded)
        {
            _animator.SetTrigger("SpinTrigger");
            _audioSource.PlayOneShot(_audioClips[0]);
        }
        else
        {
            UIManager.instance.setPromptText("Card Already Added!", Color.red, true);
            _audioSource.PlayOneShot(_audioClips[1]);
        }
    }
}
