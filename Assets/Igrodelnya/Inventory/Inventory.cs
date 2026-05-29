using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<Item> _items;
    void Awake()
    {
        if (G.Inventory == null)
        {
            G.Inventory = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void Add(Item item)
    {
        _items.Add(item);
    }

    public void Remove(Item item)
    {
        _items.Remove(item);
    }
}
