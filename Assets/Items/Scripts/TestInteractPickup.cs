using UnityEngine;

public class TestInteractPickup : ItemBase, IInteractInInventory
{
    public override string Name { get; } = "Cube Interaction et Pickup";

    public MeshRenderer _mesh;

    public override void Use()
    {
        _mesh.material.color = Random.ColorHSV();
    }

    public void UseInInventory()
    {
        Use();
    }

    public override void SetStateObject(ItemBase item)
    {
        base.SetStateObject(item);
        _mesh.material.color = ((TestInteractPickup)item)._mesh.material.color;
    }
}