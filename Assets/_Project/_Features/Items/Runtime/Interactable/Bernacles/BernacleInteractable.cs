using System;
using LightHouse.Features.Items.Detection;

namespace LightHouse.Features.Items.Interactable
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
