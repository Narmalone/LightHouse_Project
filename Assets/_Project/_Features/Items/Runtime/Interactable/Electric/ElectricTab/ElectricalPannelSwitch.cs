using LightHouse.Features.Electricity;
using LightHouse.Features.Interactions;

using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Features.Items.Interactable
{
    public class ElectricalPannelSwitch : InteractableSwitchRotate, IInteractable
    {
        [SerializeField] private ElectricZoneData _electricityZone = new ElectricZoneData(ElectricityZones.None);
        public ElectricZoneData ElectricityZone => _electricityZone;
        public event Action<ElectricalPannelSwitch> OnSwitchInteracted;

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
