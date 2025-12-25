using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private int _currentDay;
    [SerializeField] private int _maxNumberOfdays;
    [SerializeField] private bool _isCurrentlyDaytime = true;

    [Header("Day Mode")]
    [SerializeField] private float _currentNectarCoins;
    [SerializeField] private float _maxNectarCoins = 9999;
    [SerializeField] private float _currentQuota;
    [SerializeField] private float _quotaToReach;

    [SerializeField] private int _currentCustomers;
    [SerializeField] private int _customersAtATime;
    [SerializeField] private int _customersLeaved;
    [SerializeField] private int _totalCustomer;


    [SerializeField] private bool _isStoreOpen = false;
    [SerializeField] private bool _isDayFinished = false;

    [Header("Night Mode")]
    [SerializeField] private int _collectedFlowers = 0;
    [SerializeField] private int _totalFlowers;


    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    void Start()
    {
        _currentDay = 1;
        _currentNectarCoins = 0;
        initiateWorkItems();
    }

    // Update is called once per frame
    void Update()
    {

    }
    // ------------------------------------------------------------------
    // Day Logic
    public void setDay(int day)
    {
        _currentDay = day;

        if (_currentDay > _maxNumberOfdays)
        {
            _currentDay = _maxNumberOfdays;
        }
    }

    public void nextDay()
    {
        _currentDay++;
        if (_currentDay > _maxNumberOfdays)
        {
            _currentDay = _maxNumberOfdays;
        }
    }
    public int getDay()
    {
        return _currentDay;
    }

    public void setMaxNoOfDays(int maxNoOfDays)
    {
        _maxNumberOfdays = maxNoOfDays;
    }

    public int getMaxNoOfDays()
    {
        return _maxNumberOfdays;
    }
    // ------------------------------------------------------------------
    // Coins Logic
    public void addQuota(float coinAmount)
    {
        _currentQuota += coinAmount;
    }

    public void removeCoins(float coinAmount)
    {
        if (_currentQuota <= 0)
        {
            _currentNectarCoins -= coinAmount;
        }
        else if (_currentQuota <= coinAmount)
        {
            float leftAmount = coinAmount - _currentQuota;
            _currentQuota = 0;
            _currentNectarCoins -= leftAmount;
        }
        else
        {
            _currentQuota -= coinAmount;
        }

        if (_currentNectarCoins < 0)
        {
            _currentNectarCoins = 0;
        }
    }

    public void addNectarCoins()
    {
        _currentNectarCoins += _currentQuota;

        if (_currentNectarCoins > _maxNectarCoins)
        {
            _currentNectarCoins = _maxNectarCoins;
        }
    }
    // ------------------------------------------------------------------
    // Customer Logic
    public bool customerEnter()
    {
        if (_customersLeaved < _totalCustomer && _currentCustomers < _customersAtATime)
        {
            _currentCustomers++;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool customerLeave()
    {
        if (_currentCustomers > 0 && _customersLeaved < _totalCustomer)
        {
            _currentCustomers--;
            _customersLeaved++;
            return true;
        }
        else
        {
            return false;
        }
    }
    // ------------------------------------------------------------------
    //Store Management Logic

    public bool getStoreStatus()
    {
        return _isStoreOpen;
    }

    public bool getFinishStatus()
    {
        return _isDayFinished;
    }

    public bool getDayTime()
    {
        return _isCurrentlyDaytime;
    }
    public void OpenStore()
    {
        _isStoreOpen = true;
        initiateWorkItems();
    }

    public bool CloseStore()
    {
        if (_currentQuota >= _quotaToReach || _customersLeaved == _totalCustomer)
        {
            _isStoreOpen = false;
            _isDayFinished = true;
            initiateWorkItems();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void initiateWorkItems()
    {
        GameObject[] WorkItems = GameObject.FindGameObjectsWithTag("WorkItem");
        GameObject CounterDoor = GameObject.FindGameObjectWithTag("CounterDoor");
        if (_isStoreOpen)
        {
            foreach (GameObject WorkItem in WorkItems)
            {
                WorkItem.layer = 6;
            }

            if (CounterDoor != null && CounterDoor.GetComponent<DoorController>().getDoorStatus())
            {
                CounterDoor.GetComponent<DoorController>().Interact();
            }
            CounterDoor.GetComponent<DoorController>().setDoorActivatible(false);
        }
        else
        {
            foreach (GameObject WorkItem in WorkItems)
            {
                WorkItem.layer = 0;
            }

            if (CounterDoor != null)
            {
                CounterDoor.GetComponent<DoorController>().setDoorActivatible(true);
            }
        }
    }
}
