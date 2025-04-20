using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;

namespace LightHouse.Items
{
    public class InteractableSwitch : InteractableItemBase, IInteractable
    {
        protected bool _isSwitchOn = false;
        public bool IsSwitchOn => _isSwitchOn;

        public override string GetInteractionName()
        {
            return _isSwitchOn ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to switch Off"
                           : $"Press {InputManager.GetBindingName(InputManager.Interact)} to switch On";
        }

        public override void Interact()
        {
            _isSwitchOn = !_isSwitchOn;
            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
            InvokeObjectInteracted();
        }
    }

}
