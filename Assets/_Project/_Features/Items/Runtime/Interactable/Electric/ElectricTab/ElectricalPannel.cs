using System;
using UnityEngine;
using LightHouse.Features.Electricity;

namespace LightHouse.Features.Items.Interactable
{
    public class ElectricalPannel : MonoBehaviour
    {
        #region VARIABLES

        #region EVENTS
        public event Action<bool, ElectricityZones, ElectricZoneData> OnSwitchElectricityChanged;
        #endregion

        #region SERIALIZED
        [SerializeField] private InteractableSwitchRotate _electricDoor;
        [SerializeField] private SingleRaycastedItemName _enableElectricityFirst;
        [SerializeField] private ElectricalPannelSwitch[] _switchesOnPannel;
        #endregion

        #region PROPERTIES
        public InteractableSwitchRotate ElectricDoor => _electricDoor;
        public SingleRaycastedItemName EnableElectricityFirstCollider => _enableElectricityFirst;
        #endregion

        #endregion

        #region UNITY'S LIFECYCLE

        private void Awake() => RegisterEvents();

        private void OnDestroy() => UnregisterEvents();

        #endregion

        #region Callbacks Events
        public void RegisterEvents()
        {
            foreach (ElectricalPannelSwitch sw in _switchesOnPannel) 
            {
                sw.OnSwitchInteracted += Sw_OnSwitchInteracted;
            }
        }
        public void UnregisterEvents()
        {
            foreach (ElectricalPannelSwitch sw in _switchesOnPannel)
            {
                sw.OnSwitchInteracted -= Sw_OnSwitchInteracted;
            }
        }

        #endregion

        #region PANNEL INTERACTABILITY

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

        #endregion

        #region SWITCHES
        public void DownAllSwitches()
        {
            foreach (ElectricalPannelSwitch sw in _switchesOnPannel)
            {
                sw.Off();
                sw.UpdateInteractionText();
            }
        }

        private void Sw_OnSwitchInteracted(ElectricalPannelSwitch obj)
        {
            OnSwitchElectricityChanged?.Invoke(obj.IsSwitchOn, obj.ElectricityZone.Zone, obj.ElectricityZone);
        }
        #endregion

    }
}
