using UnityEngine;
using UnityEngine.AI;

public class SecurityGuard : MonoBehaviour, Iinteractable
{
    private string _actionName;
    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _isWorking = false;

    [SerializeField] private float _stoppingDistance = 0.5f;
    [SerializeField] private AudioSource[] _audioSourceList;
    [SerializeField] private AudioClip[] _audioClipList;

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
    }

    private void OnDisable()
    {
        GameManager.onStoreOpened -= StandToWork;
        GameManager.onStoreClosed -= Leave;
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
    }

    public void Interact()
    {

    }

    private void StandToWork()
    {
        gameObject.layer = 0;

        _agent.SetDestination(_goToList[0].position);
        _animator.SetTrigger("StandUpTrigger");
        _animator.SetBool("isWalking", true);
        _isWorking = true;

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
            _animator.SetBool("isWalking", false);
        }
    }

    private void Leave()
    {
        _agent.SetDestination(_goToList[1].position);
        _animator.SetBool("isWalking", true);
        _agent.isStopped = false;
        _isWorking = false;
    }

    private void checkLeave()
    {
        if (_agent.pathPending && _isWorking)
        {
            return;
        }

        if (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance && !_isWorking)
        {
            Destroy(gameObject);
        }
    }

}
