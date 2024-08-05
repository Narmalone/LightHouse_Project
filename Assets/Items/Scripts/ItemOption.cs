using UnityEngine;

public interface IItem
{
    GameObject go { get; }
    ItemDatas ItemDatas { get; }
}

public abstract class ItemBase : MonoBehaviour, IItem
{
    public virtual string Name { get; } = "Object ?";
    public GameObject go => this.gameObject;

    [SerializeField]
    private ItemDatas itemData;
    public ItemDatas ItemDatas => itemData;

    /// <summary>
    /// Use the object
    /// </summary>
    public virtual void Use()
    {

    }

    public virtual void SetStateObject(ItemBase item)
    {

    }
}