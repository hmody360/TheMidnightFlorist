using UnityEngine;

public class FlowerBox : MonoBehaviour, Iinteractable
{

    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;
    private GameObject _itemSlot;

    [SerializeField] private GameObject _bouquetPrefab;
    [SerializeField] private FlowerObj _containedFlower;
    [SerializeField] private AudioClip[] _audioClips;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _actionName = "Add " + _containedFlower.Name + " " + _containedFlower.getFlowerType();
        _itemSlot = GameObject.FindGameObjectWithTag("ItemSlot");
    }

    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }

    public void Interact()
    {
        if (GameObject.FindGameObjectWithTag("CurrentBouquet") == null)
        {
            Instantiate(_bouquetPrefab, _itemSlot.transform);
        }

        bool isFlowerAdded = GameObject.FindGameObjectWithTag("CurrentBouquet").GetComponent<BouquetHolder>().AddFlower(_containedFlower);
        if (isFlowerAdded)
        {
            _animator.SetTrigger("ShakeTrigger");
            _audioSource.PlayOneShot(_audioClips[0]);
        }
        else
        {
            UIManager.instance.setPromptText("Bouquet is Full!", Color.red, true);
            _audioSource.PlayOneShot(_audioClips[1]);
        }

    }
}
