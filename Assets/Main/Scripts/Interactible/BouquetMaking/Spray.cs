using UnityEngine;
using static GameEnums;

public class Spray : MonoBehaviour, ISellable
{
    [SerializeField] private GameObject[] _sprayPrefab;
    [SerializeField] private SprayType _type;
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _price;

    public GameObject[] Prefab
    {
        get { return _sprayPrefab; }
        set { _sprayPrefab = value; }
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public Sprite Icon
    {
        get { return _icon; }
        set { _icon = value; }
    }

    public int Price
    {
        get { return _price; }
        set { _price = value; }
    }

    public SprayType GetSprayType()
    {
        return _type;
    }

    public void SetSprayType(SprayType type)
    {
        _type = type;
    }
}
