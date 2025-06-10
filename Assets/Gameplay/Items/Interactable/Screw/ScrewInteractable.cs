using System;
using UnityEngine;
using LightHouse.Interactions;

namespace LightHouse.Items.Interactable
{
    public class ScrewInteractable : IDUseItemTracker, IDestroyable
    {
        public event Action OnDestroyed;
        
        protected override void OnDestroy()
        {
            OnDestroyed?.Invoke();
            base.OnDestroy();
        }
    }

}
