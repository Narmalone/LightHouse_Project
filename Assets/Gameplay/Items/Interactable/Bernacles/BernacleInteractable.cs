using System;
using System.Collections;
using System.Collections.Generic;
using LightHouse.Handlers;
using LightHouse.Interactions;
using LightHouse.Inventory;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class BernacleInteractable : IDUseItemTracker, IDestroyable
    {
        public event Action OnDestroyed;
        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyed?.Invoke();
        }
    }

}
