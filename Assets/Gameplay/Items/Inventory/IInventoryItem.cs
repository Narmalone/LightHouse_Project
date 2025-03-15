using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    string GetName();
    GameObject GetGameObject();
    Collider GetCollider();
    Rigidbody HasRigidBody();
}
