using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;
using LightHouse.Electricity;

namespace LightHouse.Items
{
    public class ElectricalSwitch : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _itemName;
        [SerializeField] private Collider _col;
        [SerializeField] private ElectricZoneData _electricityZone = new ElectricZoneData();

        public ElectricZoneData ElectricityZone => _electricityZone;
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;
        private bool _isSwitchOn = false;
        public bool IsSwitchOn => _isSwitchOn;

        public bool CanBeRaycasted { get; set; } = true;

        public event Action<ElectricalSwitch> OnSwitchInteracted;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public virtual string GetInteractionName()
        {
            return _isSwitchOn ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to set off"
                           : $"Press {InputManager.GetBindingName(InputManager.Interact)} to set on";
        }

        public string GetName()
        {
            return $"{_itemName} {_electricityZone.Zone.ToString()}";
        }

        public void Interact()
        {
            //interact with the switch
            _isSwitchOn = !_isSwitchOn;
            OnInteractionNameChanged?.Invoke();
            OnSwitchInteracted?.Invoke(this);
        }
    }

}
