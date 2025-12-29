using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class Customer : MonoBehaviour, Iinteractable
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private string _actionName;
    private Transform _player;
    private bool hasOrdered = false;
    private bool isBored;
    private bool isLeaving = false;


    [SerializeField] private AudioSource[] _audioSourceList;
    [SerializeField] private AudioClip[] _audioClipList;

    //Bouquet Requesting
    [SerializeField] private Bouquet _requestedBouquet;
    [SerializeField] private Wrapper[] _wrappersToChoose;
    [SerializeField] private FlowerObj[] _flowersToChoose;
    [SerializeField] private Spray[] _scentsToChoose;
    [SerializeField] private Card[] _cardsToChoose;

    [SerializeField] private float _timer = 0;
    [SerializeField] private float _waitingTime = 10;

    public List<Transform> goLocations;

    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float lookRotationSpeed = 5f;
    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActionName = "Give Bouquet";
        _agent.stoppingDistance = stoppingDistance;
        _audioSourceList[0].clip = _audioClipList[0];
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        GoToCounter();

        _requestedBouquet = GenerateRandomBouquet();
        GameManager.instance.customerEnter();
    }

    // Update is called once per frame
    void Update()
    {
        checkArrival();
        checkLeaving();
        startTimer();
        LookAtPlayer();

    }

    public void Interact()
    {
        if (hasOrdered)
        {
            int CustomerReaction = CompareBouquets();

            switch (CustomerReaction)
            {
                case 1:
                    Debug.Log("YAY");
                    _audioSourceList[1].PlayOneShot(_audioClipList[2]);
                    Leave();
                    break;
                case 2:
                    Debug.Log("Nooo");
                    _audioSourceList[1].PlayOneShot(_audioClipList[3]);
                    Leave();
                    break;
                case 3:
                    _audioSourceList[1].PlayOneShot(_audioClipList[4]);
                    UIManager.instance.setPromptText("UHH...is it invisible or..?", Color.red, true);
                    break;
                default:
                    break;
            }


        }
        else
        {
            UIManager.instance.setPromptText("Customer hasn't Ordered Yet", Color.red, true);
        }
    }

    private Bouquet GenerateRandomBouquet()
    {
        if (_wrappersToChoose.Length == 0 || _flowersToChoose.Length == 0 || _scentsToChoose.Length == 0 || _cardsToChoose.Length == 0)
        {
            Debug.Log("Some Lists Are Empty");
            return null;
        }

        int randomWrapperIndex = Random.Range(0, _wrappersToChoose.Length);

        int randomFlower1Index = Random.Range(1, _flowersToChoose.Length);
        int randomFlower2Index = Random.Range(0, _flowersToChoose.Length);
        int randomFlower3Index = (randomFlower2Index == 0) ? 0 : Random.Range(1, _flowersToChoose.Length); //Option To Request Only One Flower Set by setting the second and third to be null

        int randomScentIndex = Random.Range(0, _scentsToChoose.Length); //Scent May Be Null

        int randomCardIndex = Random.Range(0, _scentsToChoose.Length); // Card May Be Null

        List<FlowerObj> generatedFlowerList = new List<FlowerObj>() { _flowersToChoose[randomFlower1Index] };
        if (randomFlower2Index != 0)
            generatedFlowerList.Add(_flowersToChoose[randomFlower2Index]);

        if (randomFlower3Index != 0)
            generatedFlowerList.Add(_flowersToChoose[randomFlower3Index]);

        return new Bouquet(_wrappersToChoose[randomWrapperIndex], generatedFlowerList, _scentsToChoose[randomScentIndex], _cardsToChoose[randomCardIndex]);
    }

    private void GoToCounter()
    {

        _agent.SetDestination(goLocations[0].position);
        _animator.SetBool("isWalking", true);
        _audioSourceList[0].Play();
    }

    private void checkArrival()
    {
        if (hasOrdered || isLeaving)
        {
            return;
        }

        if (!_agent.pathPending && _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            Debug.Log(
                "I have Ordered As Follows:\n"
                + _requestedBouquet._wrapper.Name + "\n"
                + _requestedBouquet._flowerList[0].Name + "\n"
                + ((_requestedBouquet._flowerList.Count > 1) ? _requestedBouquet._flowerList[1].Name : "No Flower 2") + "\n"
                + ((_requestedBouquet._flowerList.Count > 2) ? _requestedBouquet._flowerList[2].Name : "No Flower 3") + "\n"
                + ((_requestedBouquet._spray != null) ? _requestedBouquet._spray.Name : "No Spray") + "\n"
                + ((_requestedBouquet._card != null) ? _requestedBouquet._card.Name : "No Card")
                );
            _animator.SetBool("isWalking", false);
            _audioSourceList[1].PlayOneShot(_audioClipList[1]);
            _audioSourceList[0].Stop();
            hasOrdered = true;
            _agent.updateRotation = false; // Stop Agent Rotation to look at player;
            gameObject.layer = 6;
        }
    }

    private void checkLeaving()
    {
        if (!isLeaving)
        {
            return;
        }

        if (!_agent.pathPending && _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            GameManager.instance.customerLeave();
            Destroy(gameObject);
        }
    }

    private void Leave()
    {
        isLeaving = true;
        gameObject.layer = 0;
        _agent.SetDestination(goLocations[1].position);
        _agent.updateRotation = true; // Start Agent Rotation to stop looking at player;
        _animator.SetBool("isWalking", true);
        _audioSourceList[0].Play();
    }

    private void startTimer()
    {
        if (isLeaving)
        {
            return;
        }

        if (hasOrdered)
        {
            _timer += Time.deltaTime;
            if (_timer / _waitingTime > 0.5f && isBored == false)
            {
                isBored = true;
                _animator.SetBool("isBored", isBored);
            }
        }

        if (_timer >= _waitingTime)
        {
            hasOrdered = false;
            _audioSourceList[1].PlayOneShot(_audioClipList[5]);
            Leave();
        }
    }

    private int CompareBouquets()
    {
        GameObject givenBouquetObj = GameObject.FindGameObjectWithTag("CurrentBouquet");

        if (givenBouquetObj != null)
        {
            BouquetHolder givenBouquet = givenBouquetObj.GetComponent<BouquetHolder>();

            if (
                givenBouquet.GetWrapper() == _requestedBouquet._wrapper &&
                givenBouquet.GetFlowerList().SequenceEqual(_requestedBouquet._flowerList) && // Comparing using == dosen't work because it compares memory references
                givenBouquet.GetSpray() == _requestedBouquet._spray &&
                givenBouquet.GetCard() == _requestedBouquet._card
                )
            {
                GameManager.instance.addQuota(givenBouquet.currentPrice);
                Destroy(givenBouquet.gameObject);
                return 1;

            }
            else
            {
                GameManager.instance.addQuota(givenBouquet.currentPrice / 2);
                Destroy(givenBouquet.gameObject);
                return 2;
            }

        }
        else
        {
            Debug.Log("You Dont't have a bouquet!");
            return 3;
        }
    }

    private void LookAtPlayer()
    {
        if (!hasOrdered || isLeaving)
        {
            return;
        }

        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookRotationSpeed * Time.deltaTime);
    }
}
