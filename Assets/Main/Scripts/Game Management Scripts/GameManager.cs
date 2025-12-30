using Seagull.Interior_I1.SceneProps;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Day Mode Objects")]
    [SerializeField] private RotatableObject _clock;

    [Header("Night Mode")]
    //[SerializeField] private int _collectedFlowers = 0;
    //[SerializeField] private int _totalFlowers;
    //[SerializeField] private float _currentTimer;
    //[SerializeField] private float _totalTimer;

    [Header("Soundtrack")]
    [SerializeField] private AudioManager _gameSoundtrackManager;

    //Events
    public static event Action onStoreOpened;
    public static event Action onStoreClosed;

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
        setDay(1);
        initiateWorkItems();

    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += initializeDayUI;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= initializeDayUI;
    }

    // Update is called once per frame

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
        UIManager.instance.UpdateQuoutaText(_currentQuota,_quotaToReach);
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
        UIManager.instance.UpdateQuoutaText(_currentQuota, _quotaToReach);
        UIManager.instance.UpdateNectarCoinsText(_currentNectarCoins);
    }

    public void addNectarCoins()
    {
        _currentNectarCoins += _currentQuota;

        if (_currentNectarCoins > _maxNectarCoins)
        {
            _currentNectarCoins = _maxNectarCoins;
        }
        UIManager.instance.UpdateNectarCoinsText(_currentNectarCoins);
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
            UIManager.instance.UpdateCustomerCountText(_customersLeaved, _totalCustomer);
            return true;
        }
        else
        {
            return false;
        }
    }

    public int getCurrentCustomers()
    {
        return _currentCustomers;
    }

    public int getCustomersAtATime()
    {
        return _customersAtATime;
    }

    public int getCustomersLeaved()
    {
        return _customersLeaved;
    }

    public int getTotalCustomers()
    {
        return _totalCustomer;
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
        OpenWorkItemLights();
        ChangeClockTime(0.85f, 0.25f);
        ChangeWorldSun(Color.white, 2f);
        UIManager.instance.SetShopStatus(_isStoreOpen);
        UIManager.instance.SetTaskText("Make Flower Bouquets for Customers");
        UIManager.instance.ShowStatsPanel();
        _gameSoundtrackManager.ChangeGameMusic(2);
        onStoreOpened?.Invoke();
    }

    public bool CloseStore()
    {
        if (_currentQuota >= _quotaToReach || _customersLeaved == _totalCustomer)
        {
            _isStoreOpen = false;
            _isDayFinished = true;
            _isCurrentlyDaytime = false;
            initiateWorkItems();
            ChangeWorldSun(Color.darkSlateBlue, 1f);
            OpenStoreLights();
            CloseWorkItemLights();
            ChangeClockTime(0.71f, 0.5f);
            UIManager.instance.SetShopStatus(_isStoreOpen);
            UIManager.instance.SetTaskText("Go To The Backyard");
            UIManager.instance.HideStatsPanel();

            _gameSoundtrackManager.ChangeGameMusic(3);
            _gameSoundtrackManager.ChangeAmbienceSounds(4);
            onStoreClosed?.Invoke();
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

    public void ChangeClockTime(float LongValue, float ShortValue)
    {
        if (LongValue > 1 || ShortValue > 1 || _clock == null)
        {
            Debug.Log("Clock Timer is Not Valid or Clock isn't included");
            return;
        }

        _clock.rotatables[0].value.rotation = LongValue;
        _clock.rotatables[1].value.rotation = ShortValue;
    }

    // Lighting Logic
    private void ChangeWorldSun(Color sunColor, float sunIntensity)
    {
        Light sunLight = RenderSettings.sun;
        if (sunLight != null)
        {
            sunLight.color = sunColor;
            sunLight.intensity = sunIntensity;
        }
    }

    private void OpenStoreLights()
    {
        GameObject[] storeLights = GameObject.FindGameObjectsWithTag("StoreLight");

        foreach (GameObject storeLight in storeLights)
        {
            storeLight.GetComponent<LightSourceObject>().turnOnAll();
        }
    }

    private void CloseStoreLights()
    {
        GameObject[] storeLights = GameObject.FindGameObjectsWithTag("StoreLight");

        foreach (GameObject storeLight in storeLights)
        {
            storeLight.GetComponent<LightSourceObject>().turnOffAll();
        }
    }

    private void OpenWorkItemLights()
    {
        GameObject[] workItemLights = GameObject.FindGameObjectsWithTag("WorkItemLight");

        foreach (GameObject WorkItemLight in workItemLights)
        {
            WorkItemLight.GetComponent<Light>().enabled = true;
        }
    }

    private void CloseWorkItemLights()
    {
        GameObject[] workItemLights = GameObject.FindGameObjectsWithTag("WorkItemLight");

        foreach (GameObject WorkItemLight in workItemLights)
        {
            WorkItemLight.GetComponent<Light>().enabled = false;
        }
    }

    //UI Logic

    private void initializeDayUI(Scene currentScene, LoadSceneMode loadSceneMode)
    {
        if (currentScene.name == "FlowershopScene")
        {
            UIManager.instance.SetDayCounterText(_currentDay);
            UIManager.instance.SetTimeDisplayer(true);
            UIManager.instance.SetTaskText("Open The Shop (Register)");
            UIManager.instance.SetShopStatus(_isStoreOpen);
            UIManager.instance.UpdateNectarCoinsText(_currentNectarCoins);
            UIManager.instance.UpdateQuoutaText(_currentQuota, _quotaToReach);
            UIManager.instance.UpdateCustomerCountText(_customersLeaved, _totalCustomer);
        }
    }

}
