using UnityEngine;

public class CustomerRespawner : MonoBehaviour
{

    
    private float _MaxRespawnTime = 10f;
    private AudioSource _audioSource;
    private GameManager _gameManager;

    [SerializeField] private bool _isRespawnerEnabled = false;
    [SerializeField] private float _respawnTimer;
    [SerializeField] private GameObject[] _customerPrefabs;
    [SerializeField] private Transform[] _customerPoints;
    [SerializeField] private GameObject[] _isCustomerAtPoint;




    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _respawnTimer = _MaxRespawnTime;
        _isCustomerAtPoint = new GameObject[_customerPoints.Length];
        _gameManager = GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isRespawnerEnabled)
        {
            startTimer();
        }
    }

    private void OnEnable()
    {
        GameManager.onStoreOpened += EnableRespawner;
        GameManager.onStoreClosed += DisableRespawner;

        Customer.OnCustomerLeave += ClearCounterSlot;
    }

    private void OnDisable()
    {
        GameManager.onStoreOpened -= EnableRespawner;
        GameManager.onStoreClosed -= DisableRespawner;

        Customer.OnCustomerLeave -= ClearCounterSlot;
    }

    public void EnableRespawner()
    {
        _isRespawnerEnabled = true;
    }

    public void DisableRespawner()
    {
        _isRespawnerEnabled = false;
    }

    private void SpawnCustomer()
    {
        if (_gameManager.getCurrentCustomers() >= _gameManager.getCustomersAtATime())
        {
            return;
        }

        if (_gameManager.getCustomersLeaved() >= _gameManager.getTotalCustomers())
        {
            return;
        }
        _respawnTimer = _MaxRespawnTime;

        int rndNum = Random.Range(0, _customerPrefabs.Length);

        GameObject customerInstance = Instantiate(_customerPrefabs[rndNum], transform.position, transform.rotation);
        Customer customer = customerInstance.GetComponentInChildren<Customer>();

        if (customer == null)
        {
            Debug.LogError("Customer is missing the Customer Script");
            return;
        }

        for (int i = 0; i < _isCustomerAtPoint.Length; i++)
        {
            if (_isCustomerAtPoint[i] == null)
            {

                _isCustomerAtPoint[i] = customerInstance;

                if (_customerPoints[i] == null)
                {
                    Debug.LogError($"Customer point {i} is NULL!");
                    return;
                }
                customer.Initialize(_customerPoints[i], transform, i);
                break;
            }
        }
    }

    private void ClearCounterSlot(int index)
    {
        if (index < 0 || index >= _isCustomerAtPoint.Length)
        {
            Debug.LogError("Counter Index Error");
            return;
        }

        _isCustomerAtPoint[index] = null;
    }

    private void startTimer()
    {
        if (_respawnTimer <= 0)
        {
            SpawnCustomer();
        }
        _respawnTimer -= Time.deltaTime;
    }
}
