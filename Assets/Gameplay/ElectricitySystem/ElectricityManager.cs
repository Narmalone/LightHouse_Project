using UnityEngine;
using System.Collections.Generic;
using System;
using LightHouse.Items.Interactable;

namespace LightHouse.Electricity
{
    #region ENUMS & UTILITIES
    public enum ElectricityZones
    {
        None,
        Maintenance,
        Kitchen,
        BedroomAndBathroom,
        Office,
        Lens,
        Shed,
    }

    [Serializable]
    public class ElectricZoneData
    {
        public ElectricityZones Zone;
        public float CurrentPowerUsed;
        public bool ElectricityOn;
        public List<IElectricItem> Items;

        public ElectricZoneData(ElectricityZones zone)
        {
            Zone = zone;
            CurrentPowerUsed = 0f;
            ElectricityOn = false;
            Items = new List<IElectricItem>();
        }

        public float RecalculatePower()
        {
            CurrentPowerUsed = 0f;
            foreach (IElectricItem item in Items)
            {
                CurrentPowerUsed += item.ElectricityCost;
                Debug.Log(item.ToString() + " "+ item.ElectricityCost + " " + CurrentPowerUsed);
            }
            return CurrentPowerUsed;
        }

        public float RemovePower(float removeSomePower)
        {
            CurrentPowerUsed -= removeSomePower;
            return CurrentPowerUsed;
        }

        public float AddPower(float addSomePower) 
        {
            CurrentPowerUsed += addSomePower;
            return CurrentPowerUsed;
        }
    }
    #endregion

    [DefaultExecutionOrder(-1)]
    public class ElectricityManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private Generator _generator;
        [SerializeField] private ElectricalPannel _electricalPannel;
        [SerializeField] private ElectricityZoneSettings _zoneSettings;

        private Dictionary<ElectricityZones, ElectricZoneData> _electricZonesData = new();
        [SerializeField] private float _maxTotalPower = 450.0f;
        #endregion

        #region Unity's Lifecycle
        private void Awake()
        {
            Initialize();
            RegisterCallbacks();
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }
        #endregion

        #region Init
        private void Initialize()
        {
            foreach (ElectricityZones zone in Enum.GetValues(typeof(ElectricityZones)))
            {
                if (!_electricZonesData.ContainsKey(zone))
                    _electricZonesData[zone] = new ElectricZoneData(zone);
            }
        }
        #endregion

        #region Callbacks
        private void RegisterCallbacks()
        {
            _generator.OnGeneratorSwitchChanged += Generator_OnGeneratorSwitchChanged;
            _electricalPannel.OnSwitchElectricityChanged += ElectricalPannel_OnSwitchElectricityChanged;
            ElectricItemRegistry.OnElectricItemRegister += ElectricItemRegistry_OnElectricItemRegister;
            ElectricItemRegistry.OnElectricItemUnregister += ElectricItemRegistry_OnElectricItemUnregister;
        }

        private void UnregisterCallbacks()
        {
            _generator.OnGeneratorSwitchChanged -= Generator_OnGeneratorSwitchChanged;
            _electricalPannel.OnSwitchElectricityChanged -= ElectricalPannel_OnSwitchElectricityChanged;
            ElectricItemRegistry.OnElectricItemRegister -= ElectricItemRegistry_OnElectricItemRegister;
            ElectricItemRegistry.OnElectricItemUnregister -= ElectricItemRegistry_OnElectricItemUnregister;
        }
        #endregion

        #region Generator Events
        private void Generator_OnGeneratorSwitchChanged(bool isOn)
        {
            if (isOn)
                _electricalPannel.OnEnablePannelInteractibility();
            else
                _electricalPannel.OnDisablePannelInteractibility();
        }
        #endregion

        #region Panel Events
        private void ElectricalPannel_OnSwitchElectricityChanged(bool state, ElectricityZones zoneKey, ElectricZoneData datas)
        {
            if (!_electricZonesData.TryGetValue(zoneKey, out var zone))
                return;

            zone.ElectricityOn = state;

            foreach (IElectricItem item in zone.Items)
            {
                item.HasElectricity = state;

                if (state) item.OnElectricityZoneEnabled();
                else item.OnElectricityZoneDisabled();
            }
        }
        #endregion

        #region Item Registry Events
        private void ElectricItemRegistry_OnElectricItemRegister(IElectricItem item)
        {
            AddItemToZone(item.ItemZone, item);
            item.AddElectricityCostToManager += Obj_AddElectricityCostToManager;
            item.RemoveElectricityCostToManager += Obj_RemoveElectricityCostToManager;
        }

        private void ElectricItemRegistry_OnElectricItemUnregister(IElectricItem item)
        {
            RemoveItemFromZone(item.ItemZone, item);
            item.AddElectricityCostToManager -= Obj_AddElectricityCostToManager;
            item.RemoveElectricityCostToManager -= Obj_RemoveElectricityCostToManager;
        }

        private void Obj_AddElectricityCostToManager(ElectricityZones zone, float power)
        {
            if (!_electricZonesData.TryGetValue(zone, out var data)) return;
            if (!data.ElectricityOn) return;
            data.AddPower(power);

            if (data.CurrentPowerUsed >= GetMaxPowerForZone(zone))
                ShutdownElectricalPannel();
            else if (GetCurrentTotalPower() > _maxTotalPower)
                ShutdownElectricalPannel();
        }

        private void Obj_RemoveElectricityCostToManager(ElectricityZones zone, float power)
        {
            if (!_electricZonesData.TryGetValue(zone, out var data)) return;
            data.RemovePower(power);
        }
        #endregion

        #region Zone Management
        private void AddItemToZone(ElectricityZones zone, IElectricItem item)
        {
            if (!_electricZonesData.TryGetValue(zone, out var data)) return;
            if (!data.Items.Contains(item))
                data.Items.Add(item);
        }

        private void RemoveItemFromZone(ElectricityZones zone, IElectricItem item)
        {
            if (!_electricZonesData.TryGetValue(zone, out var data)) return;
            if (data.Items.Contains(item))
                data.Items.Remove(item);
        }

        public List<IElectricItem> GetAllItemsInZone(ElectricityZones zone)
        {
            return _electricZonesData.TryGetValue(zone, out var data)
                ? data.Items
                : new List<IElectricItem>();
        }

        public float GetCurrentTotalPower()
        {
            float total = 0f;
            foreach (var zone in _electricZonesData.Values)
                total += zone.CurrentPowerUsed;
            return total;
        }

        public float GetMaxPowerForZone(ElectricityZones zone)
        {
            return _zoneSettings != null ? _zoneSettings.GetMaxPower(zone) : 0f;
        }

        #endregion

        #region OTHER FUNCS
        public void ShutdownElectricalPannel()
        {
            foreach(ElectricZoneData zoneDatas in _electricZonesData.Values)
            {
                zoneDatas.ElectricityOn = false;
                foreach(var item in zoneDatas.Items)
                {
                    item.HasElectricity = false;
                    item.OnElectricityZoneDisabled();
                }
                if(zoneDatas.CurrentPowerUsed < 0f) zoneDatas.CurrentPowerUsed = 0f;
            }
            _electricalPannel.DownAllSwitches();
        }
        #endregion
    }
}
