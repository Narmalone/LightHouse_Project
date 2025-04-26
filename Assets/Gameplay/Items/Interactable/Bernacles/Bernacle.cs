using System;
using System.Collections;
using System.Collections.Generic;
using LightHouse.Interactions;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class Bernacle : IDUseItemTracker, IDestroyable
    {
        public event Action OnDestroyed;
        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyed?.Invoke();
        }
    }

}
