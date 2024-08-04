using UnityEngine;

public interface IItem
{
    GameObject go { get; }
    InventoryItemData ItemDatas { get; }
}

public abstract class ItemBase : MonoBehaviour, IItem
{
    public virtual string Name { get; } = "Object ?";
    public GameObject go => this.gameObject;

    [SerializeField]
    private InventoryItemData itemData;
    public InventoryItemData ItemDatas => itemData;
}