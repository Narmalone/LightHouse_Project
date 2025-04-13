using System;
using UnityEngine;

namespace LightHouse.Interactions
{
    public interface IItemName
    {
        /// <summary>
        /// Must be called only when raycasting the object (it force to update the UI).
        /// </summary>
        public event Action OnNameUpdated;

        /// <summary>
        /// Automatically setted by <see cref="PlayerInteractions"/>, to know when the objet is raycasted.
        /// </summary>
        public bool IsItemRaycasted { get; set; }

        /// <summary>
        /// If cannot be raycasted the name won't be displayed.
        /// There still no Action when this raycasted Changed to update while raycasting an item.
        /// </summary>
        public bool CanBeRaycasted { get; set; }

        /// <summary>
        /// Display the name of the item (displays nothing if string.empty).
        /// </summary>
        string GetName();

        /// <summary>
        /// The gameobject you want to manipulate or for inventory items to disable.
        /// </summary>
        GameObject GetGameObject();

        /// <summary>
        /// The collider you want to manipulate or for inventory items to disable.
        /// </summary>
        Collider GetCollider();
    }

    public interface IItemCallback
    {
        /// <summary>
        /// When the raycast Started on the item
        /// </summary>
        void OnRaycastStart();

        /// <summary>
        /// When we stop to raycast the item
        /// </summary>
        void OnRaycastEnd();
    }
}
    