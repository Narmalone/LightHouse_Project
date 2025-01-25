using UnityEngine;

public class ItemNameDisplay : ItemBase
{
    [SerializeField] private string ObjName;

    public override string Name { get => ObjName; set => ObjName = value; }
}
