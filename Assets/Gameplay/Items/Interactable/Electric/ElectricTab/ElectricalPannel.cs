using System;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Electricity;

namespace LightHouse.Items.Interactable
{
    public class ElectricalPannel : MonoBehaviour
    {
        [SerializeField] private InteractableSwitchRotate _electricDoor;
        [SerializeField] private SingleRaycastedItemName _enableElectricityFirst;
        [SerializeField] private ElectricalSwitch[] _switchesOnPannel;

        public InteractableSwitchRotate ElectricDoor => _electricDoor;
        public SingleRaycastedItemName EnableElectricityFirstCollider => _enableElectricityFirst;
        public event Action<bool, ElectricityZones, ElectricZoneData> OnSwitchElectricityChanged;

        #region MONO'S CALLBACK

        private void Awake()
        {
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region Callbacks Events
        public void RegisterEvents()
        {
            foreach (ElectricalSwitch sw in _switchesOnPannel) 
            {
                sw.OnSwitchInteracted += Sw_OnSwitchInteracted;
            }
        }
        public void UnregisterEvents()
        {
            foreach (ElectricalSwitch sw in _switchesOnPannel)
            {
                sw.OnSwitchInteracted -= Sw_OnSwitchInteracted;
            }
        }

        #endregion

        public void OnEnablePannelInteractibility()
        {
            _electricDoor.CanBeInteracted = true;
            _electricDoor.CanBeRaycasted = true;
            _enableElectricityFirst.gameObject.SetActive(false);
        }

        public void OnDisablePannelInteractibility()
        {
            _electricDoor.CanBeInteracted = false;
            _electricDoor.CanBeRaycasted = false;
            _enableElectricityFirst.gameObject.SetActive(true);
        }

        public void DownAllSwitches()
        {

        }

        private void Sw_OnSwitchInteracted(ElectricalSwitch obj)
        {
            OnSwitchElectricityChanged?.Invoke(obj.IsSwitchOn, obj.ElectricityZone.Zone, obj.ElectricityZone);
        }

        
    }
}
