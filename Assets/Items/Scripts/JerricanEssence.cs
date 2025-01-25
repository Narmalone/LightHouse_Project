using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JerricanEssence : ItemBase
{
    public override string Name => "Jerrican";
    [SerializeField, Tooltip("Percentage of giving")] public float essenceValue = 100f;
    public event Action<float> OnJericanUse;

    [SerializeField]
    private int itemPrice = 500;

    public override bool Use()
    {
        base.Use();
        OnJericanUse?.Invoke(essenceValue);
        return false;
    }

    public override int GetItemPrice()
    {
        return itemPrice;
    }

    public override void SetStateObject(ItemBase item)
    {
        itemPrice = ((JerricanEssence)item).itemPrice;
    }
}
