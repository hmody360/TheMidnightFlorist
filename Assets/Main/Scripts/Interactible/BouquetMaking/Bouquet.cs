using System.Collections.Generic;
using UnityEngine;

public class Bouquet
{
    public Wrapper _wrapper;
    public List<FlowerObj> _flowerList;
    public Spray _spray;
    public Card _card;

    public Bouquet(Wrapper wrapper, List<FlowerObj> flowerList, Spray spray, Card card)
    {
        _wrapper = wrapper;
        _flowerList = flowerList;
        _spray = spray;
        _card = card;
    }
}
