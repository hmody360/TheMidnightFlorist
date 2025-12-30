using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using static GameEnums;

public class Wrapper : MonoBehaviour, ISellable
{
    [SerializeField] private GameObject[] _wrapperPrefab;
    [SerializeField] private WrapperType _type;
    [SerializeField] private string _name;
    [SerializeField] private int _price;

    public GameObject[] Prefab
    {
        get { return _wrapperPrefab; }
        set { _wrapperPrefab = value; }
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public int Price
    {
        get { return _price; }
        set { _price = value; }
    }

    public WrapperType GetWrapperType()
    {
        return _type;
    }

    public void SetWrapperType(WrapperType type)
    {
        _type = type;
    }

}
