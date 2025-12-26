using UnityEngine;
using System.Collections.Generic;

public class BouquetHolder : MonoBehaviour
{
    public int currentPrice = 0;

    //PrefabPoints
    [SerializeField] private Transform _WrapperPoint;
    [SerializeField] private Transform[] _flowerPoints; //Not Used Anymore [Kept Just In Case used for later]
    [SerializeField] private Transform _sprayPoint;
    [SerializeField] private Transform _cardPoint;

    [SerializeField] private Wrapper _wrapper;
    [SerializeField] private List<FlowerObj> _flowerList;
    [SerializeField] private Spray _spray;
    [SerializeField] private Card _card;

    public bool AddFlower(FlowerObj flower)
    {
        if (_flowerList.Count < 3)
        {
            _flowerList.Add(flower);
            currentPrice += flower.Price;

            switch (_flowerList.Count)
            {
                case 1:
                    Instantiate(flower.Prefab[0], _WrapperPoint);
                    break;
                case 2:
                    Instantiate(flower.Prefab[1], _WrapperPoint);
                    break;
                case 3:
                    Instantiate(flower.Prefab[2], _WrapperPoint);
                    break;
                default:
                    break;
            }
            if (_wrapper != null)
            {
                HideFlowerStems();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AddWrapper(Wrapper wrapper)
    {
        if (_wrapper != null)
        {
            return false;
        }
        else
        {
            currentPrice += wrapper.Price;
            Instantiate(wrapper, _WrapperPoint);
            _wrapper = wrapper;
            HideFlowerStems();
            return true;
        }
    }

    public bool AddSpray(Spray spray)
    {
        if (_spray != null)
        {
            return false;
        }
        else
        {
            currentPrice += spray.Price;
            Instantiate(spray, _sprayPoint);
            _spray = spray;
            return true;
        }
    }

    public bool AddCard(Card card)
    {
        if (_card != null)
        {
            return false;
        }
        else
        {
            currentPrice += card.Price;
            Instantiate(card, _cardPoint);
            _card = card;
            return true;
        }
    }

    public void HideFlowerStems()
    {
        GameObject[] currentFlowerStems = GameObject.FindGameObjectsWithTag("Stem");

        foreach (GameObject stem in currentFlowerStems)
        {
            stem.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public Wrapper GetWrapper()
    {
        return _wrapper;
    }

    public List<FlowerObj> GetFlowerList()
    {
        return _flowerList;
    }

    public Spray GetSpray()
    {
        return _spray;
    }

    public Card GetCard()
    {
        return _card;
    }

    public void SetWrapper(Wrapper wrapper)
    {
        _wrapper = wrapper;
    }

    public void SetFlowerList(List<FlowerObj> flowerList)
    {
        _flowerList = flowerList;
    }

    public void SetSpray(Spray spray)
    {
        _spray = spray;
    }

    public void SetCard(Card card)
    {
        _card = card;
    }
}
