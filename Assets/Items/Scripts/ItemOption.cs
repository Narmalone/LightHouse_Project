using System;
using UnityEngine;

public interface IItem
{
    GameObject go { get; }
    ItemDatas ItemDatas { get; }

    public bool IsInventoryItem { get;}
    public bool IsUsable { get; }
}

public abstract class ItemBase : MonoBehaviour, IItem
{
    public virtual string Name { get; } = "Object ?";
    public GameObject go => this.gameObject;

    [SerializeField]
    private ItemDatas itemData;
    public ItemDatas ItemDatas => itemData;

    public bool IsInventoryItem => isInventoryItem;
    public bool IsUsable => isUsable;

    public bool isInventoryItem = true;
    public bool isUsable = true;

    public event Action OnUse;

    /// <summary>
    /// Use the object
    /// </summary>
    public virtual void Use()
    {
        OnUse?.Invoke();
    }

    public virtual void SetStateObject(ItemBase item)
    {

    }
}