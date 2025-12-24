using UnityEngine;

public class DoorController : MonoBehaviour, Iinteractable
{
    private Animator _doorAnimController;
    private AudioSource _doorAudioSource;
    private string _actionName;

    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }


    [SerializeField] private AudioClip[] _doorClipList;
    [SerializeField] public bool isOpen = false;
    [SerializeField] private bool canOpen = true;

    private void Awake()
    {
        _doorAnimController = GetComponent<Animator>();
        _doorAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ActionName = "Open Door";
    }

    public void Interact()
    {
        if (!canOpen && GameManager.instance.getStoreStatus())
        {
            

            UIManager.instance.setPromptText("I'm Can't Leave Yet!", Color.red, true);
            _doorAudioSource.PlayOneShot(_doorClipList[2]);
        }
        else if (isOpen && !canOpen)
        {
            UIManager.instance.setPromptText("I'm in the way!", Color.red, true);
            _doorAudioSource.PlayOneShot(_doorClipList[2]);
        }
        else if (!isOpen)
        {
            _doorAudioSource.PlayOneShot(_doorClipList[0]);
            isOpen = true;
            ActionName = "Close Door";
            _doorAnimController.SetBool("isOpen", isOpen);
        }
        else
        {
            _doorAudioSource.PlayOneShot(_doorClipList[1]);
            isOpen = false;
            ActionName = "Open Door";
            _doorAnimController.SetBool("isOpen", isOpen);
        }
    }

    public bool getDoorStatus()
    {
        return isOpen;
    }

    public void setDoorActivatible(bool ability)
    {
        canOpen = ability;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = true;
        }
    }
}
