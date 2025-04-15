using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;

namespace LightHouse.Items
{
    public class InteractableSwitch : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string _itemName;
        [SerializeField] protected Collider _col;

        protected bool _isSwitchOn = false;
        public bool IsSwitchOn => _isSwitchOn;
        public bool CanBeInteracted { get; set; } = true;
        public bool IsItemRaycasted { get; set; }
        public bool CanBeRaycasted { get; set; } = true;

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public virtual string GetInteractionName()
        {
            return _isSwitchOn ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to switch Off"
                           : $"Press {InputManager.GetBindingName(InputManager.Interact)} to switch On";
        }

        public string GetName() => _itemName;

        public virtual void Interact()
        {
            _isSwitchOn = !_isSwitchOn;
            OnInteractionNameChanged?.Invoke();
            OnObjectInteracted?.Invoke();
        }
    }

}
