using System;
using UnityEngine;

namespace LightHouse.Interactions
{
    public interface IItemName
    {
        public bool CanBeRaycasted { get; set; }
        public bool IsItemRaycasted { get; set; }

        public event Action OnNameUpdated;
        string GetName();
        GameObject GetGameObject();
        Collider GetCollider();
    }
}
