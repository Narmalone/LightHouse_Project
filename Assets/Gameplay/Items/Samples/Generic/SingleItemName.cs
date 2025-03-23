using LightHouse.Interactions;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class SingleItemName : ItemBase, IItemName
    {
        [SerializeField] private string _itemName;
        [SerializeField] private Collider _col;

        public string Name
        {
            get => _itemName;
            set => _itemName = value;
        }

        public Collider col => _col;

        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        public event Action OnNameUpdated;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public string GetName()
        {
            return _itemName;
        }
    }

}
