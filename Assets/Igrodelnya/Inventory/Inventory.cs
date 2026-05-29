using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private readonly List<Item> _items = new();

    public int Count => _items.Count;

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
        if (item == null)
            return;

        _items.Add(item);
    }

    public void Remove(Item item)
    {
        if (item == null)
            return;

        _items.Remove(item);
    }
}
