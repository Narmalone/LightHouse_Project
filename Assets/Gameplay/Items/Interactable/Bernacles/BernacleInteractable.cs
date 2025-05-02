using System;
using LightHouse.Interactions;

namespace LightHouse.Items.Interactable
{
    public class BernacleInteractable : IDUseItemTracker, IDestroyable
    {
        public event Action OnDestroyed;
        protected override void OnDestroy()
        {
            OnDestroyed?.Invoke();
            base.OnDestroy();
        }
    }

}
