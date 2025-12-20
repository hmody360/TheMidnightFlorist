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
        if(_flowerList.Count < 3)
        {
            _flowerList.Add(flower);
            currentPrice += flower.Price;

            switch (_flowerList.Count)
            {
                case 1:
                    Instantiate(flower.Prefab, _flowerPoints[0]);
                    break;
                case 2:
                    Instantiate(flower.Prefab, _flowerPoints[1]);
                    break;
                case 3:
                    Instantiate(flower.Prefab, _flowerPoints[2]);
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
}
