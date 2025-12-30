using UnityEngine;
using static GameEnums;

public class FlowerObj : MonoBehaviour, ISellable
{
    [SerializeField] private GameObject[] _flowerPrefab;
    [SerializeField] private FlowerType _type;
    [SerializeField] private string _color;
    [SerializeField] private int _price;



    public GameObject[] Prefab
    {
        get { return _flowerPrefab; }
        set { _flowerPrefab = value; }
    }

    public string Name
    {
        get { return _color; }
        set { _color = value; }
    }

    public int Price
    {
        get { return _price; }
        set { _price = value; }
    }

    public FlowerType getFlowerType()
    {
        return _type;
    }
    public void SetFlowerType(FlowerType type)
    {
        _type = type;
    }

}
