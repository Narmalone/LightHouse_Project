using UnityEngine;
using LightHouse.Items;
using System.Collections.Generic;
using System;
using LightHouse.Items.Interactable;

namespace LightHouse.Electricity
{
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
        public float PowerUsedWhenEnabled;
        public bool IsEnabled;
        public List<IElectricItem> Items;

        public ElectricZoneData(ElectricityZones zone)
        {
            Zone = zone;
            PowerUsedWhenEnabled = 0f;
            IsEnabled = false;
            Items = new List<IElectricItem>();
        }

        public float RecalculatePower()
        {
            PowerUsedWhenEnabled = 0f;
            foreach (IElectricItem item in Items)
                PowerUsedWhenEnabled += item.ElectricityCost;
            return PowerUsedWhenEnabled;
        }
    }

    [DefaultExecutionOrder(-1)]
    public class ElectricityManager : MonoBehaviour
    {
        [SerializeField] private Generator _generator;
        [SerializeField] private ElectricalPannel _electricalPannel;
        [SerializeField]
        private ElectricityZoneSettings _zoneSettings;

        private Dictionary<ElectricityZones, ElectricZoneData> _electricZonesData = new();

        [SerializeField] private float _maxTotalPower = 600.0f;
        [SerializeField] private float _currentTotalPower = 0.0f;

        #region MONO
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

            float maxPower = 0f;
            foreach(var zone in _zoneSettings.zonePowers)
            {
                maxPower += GetMaxPowerForZone(zone.Zone);
            }
            _maxTotalPower = maxPower;
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
        /// <summary>
        /// TO DO CHANGER l'INTEGRALITE DE LA FONCTION
        /// </summary>
        private void ElectricalPannel_OnSwitchElectricityChanged(bool state, ElectricityZones zoneKey, ElectricZoneData datas)
        {
            if (!_electricZonesData.TryGetValue(zoneKey, out var zone))
                return;

            zone.IsEnabled = state;

            foreach (IElectricItem item in zone.Items)
            {
                item.IsElectricityOn = state;

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

        private void Obj_AddElectricityCostToManager(float power)
        {
            // Implémenter si nécessaire
        }

        private void Obj_RemoveElectricityCostToManager(float power)
        {
            // Implémenter si nécessaire
        }
        #endregion

        #region Zone Management
        private void AddItemToZone(ElectricityZones zone, IElectricItem item)
        {
            if (!_electricZonesData.TryGetValue(zone, out var data)) return;

            if (!data.Items.Contains(item))
            {
                data.Items.Add(item);
                data.RecalculatePower();
            }
        }

        private void RemoveItemFromZone(ElectricityZones zone, IElectricItem item)
        {
            if (!_electricZonesData.TryGetValue(zone, out var data)) return;

            if (data.Items.Remove(item))
            {
                data.RecalculatePower();
            }
        }

        public List<IElectricItem> GetAllItemsInZone(ElectricityZones zone)
        {
            return _electricZonesData.TryGetValue(zone, out var data)
                ? data.Items
                : new List<IElectricItem>();
        }

        public float GetTotalPowerInZone(ElectricityZones zone)
        {
            return _electricZonesData.TryGetValue(zone, out var data)
                ? data.PowerUsedWhenEnabled
                : 0f;
        }

        public float GetCurrentTotalPower()
        {
            float total = 0f;
            foreach (var zone in _electricZonesData.Values)
                total += zone.PowerUsedWhenEnabled;
            return total;
        }

        public float GetMaxPowerForZone(ElectricityZones zone)
        {
            return _zoneSettings != null ? _zoneSettings.GetMaxPower(zone) : 0f;
        }

        #endregion
    }
}
