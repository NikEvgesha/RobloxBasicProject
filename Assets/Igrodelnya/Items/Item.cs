using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string _name;
    [SerializeField] protected Item _type;
    [SerializeField] protected Sprite _icon;

    public Item Type => _type;
    public Sprite Icon => _icon;
    public string Name => _name;
    public bool InQuickAccess { get; set; }
}
