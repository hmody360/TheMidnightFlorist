using UnityEngine;

public class WrapperStack : MonoBehaviour, Iinteractable
{

    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;
    private GameObject _itemSlot;

    [SerializeField] private GameObject _bouquetPrefab;
    [SerializeField] private Wrapper _containedWrapper;
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
        ActionName = "Use " + _containedWrapper.Name + " Wrapper";
        _itemSlot = GameObject.FindGameObjectWithTag("ItemSlot");
    }

    public void Interact()
    {
        if (GameObject.FindGameObjectWithTag("CurrentBouquet") == null)
        {
            Instantiate(_bouquetPrefab, _itemSlot.transform);
        }

        bool isWrapperAdded = GameObject.FindGameObjectWithTag("CurrentBouquet").GetComponent<Bouquet>().AddWrapper(_containedWrapper);

        if(isWrapperAdded)
        {
            _animator.SetTrigger("SpinTrigger");
            _audioSource.PlayOneShot(_audioClips[0]);
        }
        else
        {
            UIManager.instance.setPromptText("Wrapper Already Added!", Color.red, true);
            _audioSource.PlayOneShot(_audioClips[1]);
        }
    }


}
