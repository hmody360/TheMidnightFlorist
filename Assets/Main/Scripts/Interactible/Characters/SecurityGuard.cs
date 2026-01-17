using UnityEngine;
using UnityEngine.AI;

public class SecurityGuard : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _isWorking = false;
    private bool _isWalking = false;
    private bool _hasTalked = false;

    [SerializeField] private float _stoppingDistance = 0.5f;
    [SerializeField] private AudioSource[] _audioSourceList;

    [SerializeField] private Transform[] _goToList;

    [SerializeField] Dialogue[] securityDialogues;

    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }

    private void OnEnable()
    {
        GameManager.onStoreOpened += StandToWork;
        GameManager.onStoreClosed += Leave;
        DialogueManager.onDialougeEnd += EndConversation;
        DialogueManager.onSentenceEnd += PauseTalking;
        DialogueManager.onSentenceStart += ResumeTalking;
    }

    private void OnDisable()
    {
        GameManager.onStoreOpened -= StandToWork;
        GameManager.onStoreClosed -= Leave;
        DialogueManager.onDialougeEnd -= EndConversation;
        DialogueManager.onSentenceEnd -= PauseTalking;
        DialogueManager.onSentenceStart -= ResumeTalking;
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ActionName = "Talk To Guard";
        _agent.stoppingDistance = _stoppingDistance;
    }

    private void Update()
    {
        checkReachWorkSpot();
        checkLeave();

        if (_isWalking)
        {
            _audioSourceList[0].Play();
            _isWalking = false;
        }
    }

    public void Interact()
    {
        if (_hasTalked)
        {
            UIManager.instance.setPromptText("I'm busy right now...", Color.red, true);
            return;
        }

        switch (GameManager.instance.getDay())
        {
            case 1:
                DialogueManager.instance.StartDialogue(securityDialogues[0]);
                break;
            case 2:
                DialogueManager.instance.StartDialogue(securityDialogues[1]);
                break;
            case 3:
                DialogueManager.instance.StartDialogue(securityDialogues[2]);
                break;
            default:
                break;
        }

        
        _audioSourceList[1].Play();
        _animator.SetBool("isTalking", true);
    }

    private void StandToWork()
    {
        gameObject.layer = 0;

        _agent.SetDestination(_goToList[0].position);
        _animator.SetTrigger("StandUpTrigger");
        _animator.SetBool("isWalking", true);
        _isWorking = true;
        _isWalking = true;

    }

    private void checkReachWorkSpot()
    {
        if (_agent.pathPending)
        {
            return;
        }

        if (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance && _isWorking)
        {
            _agent.isStopped = true;
            _audioSourceList[0].Stop();
            _animator.SetBool("isWalking", false);
        }
    }

    private void Leave()
    {
        _agent.SetDestination(_goToList[1].position);
        _animator.SetBool("isWalking", true);
        _agent.isStopped = false;
        _isWorking = false;
        _isWalking = true;
    }

    private void checkLeave()
    {
        if (_agent.pathPending && _isWorking)
        {
            return;
        }

        if (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance && !_isWorking)
        {
            _audioSourceList[0].Stop();
            Destroy(gameObject);
        }
    }

    private void EndConversation()
    {
        _audioSourceList[1].Stop();
        _animator.SetBool("isTalking", false);
        _hasTalked = true;
    }

    private void PauseTalking()
    {
        if (_audioSourceList[1].isPlaying)
        {
            _audioSourceList[1].Pause();
            _animator.SetBool("isTalking", false);
        }
        
    }

    private void ResumeTalking()
    {
        if (!_audioSourceList[1].isPlaying && _hasTalked == false)
        {
            _audioSourceList[1].Play();
            _animator.SetBool("isTalking", true);
        }
    }

}
