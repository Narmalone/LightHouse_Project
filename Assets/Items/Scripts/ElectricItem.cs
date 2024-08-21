using UnityEngine;

public class ElectricItem : ItemBase
{
    [Header("ELECTRIC ITEM")]
    public bool HasElectricity = false;

    public virtual void OnElecEnabled() { }

    public virtual void OnElecDisabled() { }
}