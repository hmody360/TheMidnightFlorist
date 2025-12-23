using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Bouquet : MonoBehaviour
{
    public int currentPrice = 0;

    //PrefabPoints
    [SerializeField] private Transform _WrapperPoint;
    [SerializeField] private Transform[] _flowerPoints;
    [SerializeField] private Transform _sprayPoint;
    [SerializeField] private Transform _cardPoint;

    //Bouquet Contents
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
                    Instantiate(flower, _flowerPoints[0]);
                    break;
                case 2:
                    Instantiate(flower, _flowerPoints[1]);
                    break;
                case 3:
                    Instantiate(flower, _flowerPoints[2]);
                    break;
                default:
                    break;
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
}
