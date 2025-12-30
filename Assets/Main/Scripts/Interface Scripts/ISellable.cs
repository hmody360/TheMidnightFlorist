using UnityEngine;

public interface ISellable
{
    public GameObject[] Prefab { get; set; }
    public string Name { get; set; }
    public Sprite Icon { get; }
    public int Price { get; set; }
}
