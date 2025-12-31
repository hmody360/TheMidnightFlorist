using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToBackyard : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private Animator _animator;
    private GameManager _gameManager;
    private AudioSource _audioSource;

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
        _gameManager = GameManager.instance;
        ActionName = "Go To Backyard?";
    }

    public void Interact()
    {
        if (_gameManager != null)
        {
            if (_gameManager.getFinishStatus())
            {
                gameObject.layer = 0;
                _audioSource.PlayOneShot(_audioClips[1]);
                _gameManager.resetGameStats(false);
                _animator.SetTrigger("DoorOpening");
                StartCoroutine(WaitBeforeExit());
            }
            else
            {
                _animator.SetTrigger("DoorLocked");
                _audioSource.PlayOneShot(_audioClips[0]);
                UIManager.instance.setPromptText("Finish Your Shift First!", Color.red, true);
            }
        }
    }

    private IEnumerator WaitBeforeExit()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("CopyM");
    }

}
