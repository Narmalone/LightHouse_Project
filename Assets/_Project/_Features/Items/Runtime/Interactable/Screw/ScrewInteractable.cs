using LightHouse.Features.Items.Detection;
using System;

namespace LightHouse.Features.Items.Interactable
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
