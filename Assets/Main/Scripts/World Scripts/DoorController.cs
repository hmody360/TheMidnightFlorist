using UnityEngine;

public class DoorController : MonoBehaviour, Iinteractable
{
    private Animator _doorAnimController;
    private AudioSource _doorAudioSource;


    public string ActionName => "Open Door";

    [SerializeField] private AudioClip[] _doorClipList;
    [SerializeField] private bool isOpen = false;

    private void Awake()
    {
        _doorAnimController = GetComponent<Animator>();
        _doorAudioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (!isOpen)
        {
            _doorAudioSource.PlayOneShot(_doorClipList[0]);
            isOpen = true;
            _doorAnimController.SetBool("isOpen", isOpen);
        }
        else
        {
            _doorAudioSource.PlayOneShot(_doorClipList[1]);
            isOpen = false;
            _doorAnimController.SetBool("isOpen", isOpen);
        }
    }
}
