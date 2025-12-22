using UnityEngine;

public class SprayBottle : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;
    private GameObject _itemSlot;

    [SerializeField] private Spray _containedSpray;
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
        ActionName = "Spray " + _containedSpray.Name + " " +  _containedSpray.GetSprayType() + " Spray";
        _itemSlot = GameObject.FindGameObjectWithTag("ItemSlot");
    }

    public void Interact()
    {
        GameObject currentBouquet = GameObject.FindGameObjectWithTag("CurrentBouquet");

        if (currentBouquet == null || currentBouquet.GetComponent<Bouquet>().GetWrapper() == null || currentBouquet.GetComponent<Bouquet>().GetFlowerList().Count == 0)
        {
            _audioSource.PlayOneShot(_audioClips[1]);
            return;
        }

        bool isSprayAdded = GameObject.FindGameObjectWithTag("CurrentBouquet").GetComponent<Bouquet>().AddSpray(_containedSpray);

        if (isSprayAdded)
        {
            _animator.SetTrigger("SprayTrigger");
            _audioSource.PlayOneShot(_audioClips[0]);
        }
        else
        {
            _audioSource.PlayOneShot(_audioClips[1]);
        }
    }
}
