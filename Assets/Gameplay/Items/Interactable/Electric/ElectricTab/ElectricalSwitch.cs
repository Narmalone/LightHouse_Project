using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;
using LightHouse.Electricity;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Items.Interactable
{
    public class ElectricalSwitch : InteractableSwitchRotate, IInteractable
    {
        [SerializeField] private ElectricZoneData _electricityZone = new ElectricZoneData();
        public ElectricZoneData ElectricityZone => _electricityZone;
        public event Action<ElectricalSwitch> OnSwitchInteracted;

        [Header(" -- Localization Electric --")]
        [SerializeField] protected LocalizedString _electricZone;
        protected string _electricZoneName;

        protected override void Awake()
        {
            base.Awake();
            SetUpElectricityText();
        }

        protected async void SetUpElectricityText()
        {
            AsyncOperationHandle<string> actionTextOp = _electricZone.GetLocalizedStringAsync();
            await actionTextOp.Task;
            _electricZoneName = actionTextOp.Result;
        }

        protected override void OnLocaleChanged(Locale locale)
        {
            base.OnLocaleChanged(locale);
            SetUpElectricityText();
        }

        public override string GetName()
        {
            return $"{_name} {_electricZoneName}";
        }

        public override void Interact()
        {
            base.Interact();
            OnSwitchInteracted?.Invoke(this);
        }
    }

}
