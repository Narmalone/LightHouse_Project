using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractCubeTest : ItemBase
{
    public override string Name { get; } = "Cube Interaction";

    public MeshRenderer _mesh;

    public override void Use()
    {
        _mesh.material.color = Random.ColorHSV();
    }
}
