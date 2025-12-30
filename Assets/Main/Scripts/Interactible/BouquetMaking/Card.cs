using UnityEngine;
using static GameEnums;

public class Card : MonoBehaviour, ISellable
{
    [SerializeField] private GameObject[] _cardPrefab;
    [SerializeField] private CardType _type;
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _price;

    public GameObject[] Prefab
    {
        get { return _cardPrefab; }
        set { _cardPrefab = value; }
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

    public CardType GetCardType()
    {
        return _type;
    }

    public void SetCardType(CardType type)
    {
        _type = type;
    }
}
