using UnityEngine;

public interface IItem
{
    GameObject go { get; }
    public bool IsInventoryItem { get; }
    ItemDatas ItemDatas { get; }
}

public abstract class ItemBase : MonoBehaviour, IItem
{
    public virtual string Name { get; } = "Object ?";
    public GameObject go => this.gameObject;

    [SerializeField]
    private ItemDatas itemData;
    public ItemDatas ItemDatas => itemData;

    public bool IsInventoryItem { get => false; }

    /// <summary>
    /// Use the object
    /// </summary>
    public virtual void Use()
    {

    }
}

public abstract class ItemBaseInventory : MonoBehaviour, IItem
{

    public virtual string Name { get; } = "Object ?";
    public GameObject go => this.gameObject;

    [SerializeField]
    private ItemDatas itemData;
    public ItemDatas ItemDatas => itemData;

    public bool IsInventoryItem { get => true; }

    /// <summary>
    /// Put the object in the inventory
    /// </summary>
    public virtual void TakeObject()
    {
        PlayerInventory.TakeItemAction.Invoke(this);
    }
}