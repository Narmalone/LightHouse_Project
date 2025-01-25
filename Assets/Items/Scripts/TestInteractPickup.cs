using UnityEngine;

public class TestInteractPickup : ItemBase, IInteractInInventory
{
    public override string Name { get; set; } = "Cube Interaction et Pickup";

    public MeshRenderer _mesh;

    public override bool Use()
    {
        _mesh.material.color = Random.ColorHSV();
        return false;
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