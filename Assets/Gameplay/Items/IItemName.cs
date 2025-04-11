using System;
using UnityEngine;

namespace LightHouse.Interactions
{
    public interface IItemName
    {
        //MUST BE CALLED ONLY WHEN RAYCASTING
        public event Action OnNameUpdated;

        /// <summary>
        /// Automatically setted, to know when the objet is raycasted
        /// </summary>
        public bool IsItemRaycasted { get; set; }

        /// <summary>
        /// If cannot be raycasted the name won't be displayed.
        /// There still no Action when this raycasted Changed to update while raycasting an item
        /// </summary>
        public bool CanBeRaycasted { get; set; }

        /// <summary>
        /// Display the name of the item (displays nothing if string.empty)
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
        /// When we change raycasted item or stop raycast anything
        /// </summary>
        void OnRaycastEnd();
    }
}
    