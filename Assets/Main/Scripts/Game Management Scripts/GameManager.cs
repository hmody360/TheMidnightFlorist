using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private int _currentDay;
    [SerializeField] private int _MaxNumberOfdays;

    [Header("Day Mode")]
    [SerializeField] private float _currentNectarCoins;
    [SerializeField] private float _maxNectarCoins = 9999;
    [SerializeField] private int _currentCustomers;
    [SerializeField] private int _CustomersLeaved;
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setDay(int day)
    {
        _currentDay = day;

        if (_currentDay > _MaxNumberOfdays)
        {
            _currentDay = _MaxNumberOfdays;
        }
    }

    public void nextDay()
    {
        _currentDay++;
        if (_currentDay > _MaxNumberOfdays)
        {
            _currentDay = _MaxNumberOfdays;
        }
    }
    public int getDay()
    {
        return _currentDay;
    }
}
