using LightHouse.Features.Electricity;

using System;
using UnityEngine;

namespace LightHouse.Features.Items.Interactable
{
    public class ElectricalSwitch : InteractableSwitch, IElectricItem
    {
        #region VARIABLES
        #region EVENTS
        public event Action<ElectricityZones, float> AddElectricityCostToManager;
        public event Action<ElectricityZones, float> RemoveElectricityCostToManager;
        #endregion

        #region SERIALIZED
        [Header(" --- ELECTRICITY --- ")]
        [field: SerializeField] public ElectricityZones ItemZone { get; set; }
        [field: SerializeField] public float ElectricityCost { get; set; } = -1;
        [field: SerializeField] public bool HasElectricity { get; set; }

        [Header(" --- OTHER --- ")]
        [SerializeField] private MonoBehaviour[] _electricItemBehaviours;
        [SerializeField] private SingleRaycastedItemName _noElectricity;
        #endregion

        #region PRIVATE
        private IElectricItem[] _items;
        #endregion

        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            _items = new IElectricItem[_electricItemBehaviours.Length];
            for (int i = 0; i < _electricItemBehaviours.Length; i++)
            {
                _items[i] = _electricItemBehaviours[i] as IElectricItem;
            }
            this._detectionCollider.enabled = false;
        }

        private void Start()
        {
            ElectricItemRegistry.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ElectricItemRegistry.Unregister(this);
        }

        #endregion

        #region INTERACTABLE
        public override void Interact()
        {
            base.Interact();

            if (_isSwitchOn)
            {
                foreach(var item in _items)
                {
                    item.UserTurnOn();
                }
            }
            else
            {
                foreach(var item in _items)
                {
                    item.UserTurnOff();
                }
            }
        }
        #endregion

        #region ELECTRICTIY
        public void OnElectricityZoneEnabled()
        {
            //Update localization / interactibility
            _noElectricity.gameObject.SetActive(false);
            this._detectionCollider.enabled = true;
            AddElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        public void OnElectricityZoneDisabled()
        {
            //Update localization / interactibility
            _noElectricity.gameObject.SetActive(true);
            this._detectionCollider.enabled = false;
            RemoveElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        public void UserTurnOn() { }
        public void UserTurnOff() { }
        #endregion
    }

}
