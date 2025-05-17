using System;
using LightHouse.Electricity;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class ElectricalSwitch : InteractableSwitch, IElectricItem
    {
        [SerializeField] private MonoBehaviour[] _electricItemBehaviours;
        [SerializeField] private SingleRaycastedItemName _noElectricity;
        private IElectricItem[] _items;
        [SerializeField] private ElectricityZones _zoneToEnableOrDisable = ElectricityZones.None;

        [field: SerializeField] public float ElectricityCost { get; set; } = -1;
        public bool IsElectricityOn { get; set; }
        [field: SerializeField] public ElectricityZones ItemZone { get; set; }

        public event Action<ElectricityZones, float> AddElectricityCostToManager;
        public event Action<ElectricityZones, float> RemoveElectricityCostToManager;

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

        //TO DOO Quand il n'y a pas d'électricité dans la zone on affiche un autre


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

        public void OnElectricityZoneEnabled()
        {
            //Update localization / interactibility
            _noElectricity.gameObject.SetActive(false);
            this._detectionCollider.enabled = true;
        }

        public void OnElectricityZoneDisabled()
        {
            //Update localization / interactibility
            _noElectricity.gameObject.SetActive(true);
            this._detectionCollider.enabled = false;
        }

        public void UserTurnOn() { }
        public void UserTurnOff() { }
    }

}
