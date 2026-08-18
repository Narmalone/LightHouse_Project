using LightHouse.Core.Inventory;
using LightHouse.Features.Items.Inventory;
using System;
using UnityEngine;

public class FuzeItem : InventoryItemBase, IInventoryItemUsable
{
    [SerializeField] private float _maxDurability = 60f;
    [SerializeField] private ItemIDEnum _itemIdEnum = ItemIDEnum.Fuze;

    public float MaxDurabilityItem { get => _maxDurability; }
    public float Durability { get; set; } = 60f;

    [SerializeField] private bool _canBeUsedFromInventory = true;
    [SerializeField] private float _useHoldTime = 0f;
    public bool CanBeUsedFromInventory { get => _canBeUsedFromInventory; set => _canBeUsedFromInventory = value; }
    public float UseHoldTime { get => _useHoldTime; set => _useHoldTime = value; }

    public event Action OnItemUsed;
    public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
    public event Action<string> UseTextSlotChanged;

    protected override void Start()
    {
        base.Start();
        Durability = MaxDurabilityItem;
    }

    public void InvokeOnCanBeUsedFromInventoryChanged()
    {
        CanBeUsedFromInventoryChanged?.Invoke(GlobalItemID, ItemSpecificID);
    }

    public void UseFromInventory()
    {
        OnItemUsed?.Invoke();
    }

    public string UseTextSlot()
    {
        return "Use Fuze - TEMP TEXT";
    }
}
