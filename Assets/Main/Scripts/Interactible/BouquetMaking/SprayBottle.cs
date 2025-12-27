using UnityEngine;

public class SprayBottle : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;
    private ParticleSystem _sprayParticleSystem;

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
        ActionName = "Spray " + _containedSpray.Name + " " + _containedSpray.GetSprayType() + " Spray";
        _sprayParticleSystem = GetComponent<ParticleSystem>();
    }

    public void Interact()
    {
        GameObject currentBouquet = GameObject.FindGameObjectWithTag("CurrentBouquet");

        if (currentBouquet == null || currentBouquet.GetComponent<BouquetHolder>().GetWrapper() == null || currentBouquet.GetComponent<BouquetHolder>().GetFlowerList().Count == 0)
        {
            UIManager_Day.instance.setPromptText("Add Wrapper and Flower First!", Color.red, true);
            _animator.SetTrigger("NoSprayTrigger");
            _audioSource.PlayOneShot(_audioClips[1]);
            return;
        }

        bool isSprayAdded = GameObject.FindGameObjectWithTag("CurrentBouquet").GetComponent<BouquetHolder>().AddSpray(_containedSpray);

        if (isSprayAdded)
        {
            _animator.SetTrigger("SprayTrigger");
            _sprayParticleSystem.Play();
            _audioSource.PlayOneShot(_audioClips[0]);
        }
        else
        {
            _animator.SetTrigger("NoSprayTrigger");
            UIManager_Day.instance.setPromptText("Spray Already Added", Color.red, true);
            _audioSource.PlayOneShot(_audioClips[1]);
        }
    }
}
