using UnityEngine;

public class Trashcan : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;


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
        ActionName = "Scrap Current Bouquet";
    }

    public void Interact()
    {
        GameObject _currentBouquet = GameObject.FindGameObjectWithTag("CurrentBouquet");
        if(_currentBouquet != null)
        {
            _audioSource.PlayOneShot(_audioClips[0]);
            _animator.SetTrigger("YesTrigger");
            Destroy(_currentBouquet);
        }
        else
        {
            UIManager.instance.setPromptText("No Wrapper In Hand!", Color.red, true);
            _animator.SetTrigger("NoTrigger");
            _audioSource.PlayOneShot(_audioClips[1]);
        }
    }
}
