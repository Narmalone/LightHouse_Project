using UnityEngine;
using LightHouse.Items;
using System.Collections.Generic;
using System;

namespace LightHouse.Electricity
{
    public enum ElectricityZones
    {
        None, 
        RDC,
        OutsideLocal,
    }

    [Serializable]
    public struct ElectricZoneData
    {
        public ElectricityZones Zone;
        public float PowerUsed;
        public float MaxPower;
        public bool IsElectricityOn;
        public List<IElectricItem> Items;

        public ElectricZoneData(ElectricityZones zone)
        {
            Zone = zone;
            PowerUsed = 0f;
            MaxPower = 100.0f;
            IsElectricityOn = false;
            Items = new List<IElectricItem>();
        }

        public float RecalculatePower()
        {
            PowerUsed = 0f;
            foreach (IElectricItem item in Items)
            {
                PowerUsed += item.ElectricityCost;
            }
            return PowerUsed;
        }
    }


    [DefaultExecutionOrder(-1)]
    public class ElectricityManager : MonoBehaviour
    {
        [SerializeField] private Generator _generator;
        [SerializeField] private ElectricalPannel _electricalPannel;

        private Dictionary<ElectricityZones, ElectricZoneData> _electricZonesData = new();
        [SerializeField] private float _maxTotalPower = 600.0f;
        [SerializeField] private float _currentTotalPower = 0.0f;

        #region MONO'S Callback
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

        #region Registering & Unregistering Callbacks

        public void RegisterCallbacks()
        {
            _generator.OnGeneratorSwitchChanged += Generator_OnGeneratorSwitchChanged;
            _electricalPannel.OnSwitchElectricityChanged += ElectricalPannel_OnSwitchElectricityChanged;
            ElectricItemRegistry.OnElectricItemRegister += ElectricItemRegistry_OnElectricItemRegister;
            ElectricItemRegistry.OnElectricItemUnregister += ElectricItemRegistry_OnElectricItemUnregister;
        }

        public void UnregisterCallbacks()
        {
            _generator.OnGeneratorSwitchChanged -= Generator_OnGeneratorSwitchChanged;
            _electricalPannel.OnSwitchElectricityChanged -= ElectricalPannel_OnSwitchElectricityChanged;
            ElectricItemRegistry.OnElectricItemRegister -= ElectricItemRegistry_OnElectricItemRegister;
            ElectricItemRegistry.OnElectricItemUnregister -= ElectricItemRegistry_OnElectricItemUnregister;
        }

        #endregion

        #region INIT
        private void Initialize()
        {
            foreach (ElectricityZones zone in Enum.GetValues(typeof(ElectricityZones)))
            {
                if (!_electricZonesData.ContainsKey(zone))
                {
                    _electricZonesData[zone] = new ElectricZoneData(zone);
                }
            }
        }

        #endregion

        #region  -- ITEMS CALLBACKS --

        #region Generator Callbacks

        private void Generator_OnGeneratorSwitchChanged(bool obj)
        {
            if (obj)
            {
                _electricalPannel.OnEnablePannelInteractibility();
            }
            else
            {
                _electricalPannel.OnDisablePannelInteractibility();
            }
        }

        #endregion

        #region Electrical Callbacks
        private void ElectricalPannel_OnSwitchElectricityChanged(bool arg1, ElectricityZones zoneKey, ElectricZoneData datas)
        {
            if (!_electricZonesData.ContainsKey(zoneKey)) return;

            ElectricZoneData zone = _electricZonesData[zoneKey];
            zone.IsElectricityOn = arg1;
            _electricZonesData[zoneKey] = zone;

            foreach (IElectricItem target in zone.Items)
            {
                target.IsElectricityOn = arg1;

                if (arg1)
                    target.OnElectricityZoneEnabled();
                else
                    target.OnElectricityZoneDisabled();
            }
        }

        #endregion

        #region ElectricItemRegistry Callbacks
        private void ElectricItemRegistry_OnElectricItemUnregister(IElectricItem item)
        {
            RemoveItemFromDictionnary(item.ItemZone, item);
            item.AddElectricityCostToManager -= Obj_AddElectricityCostToManager;
            item.RemoveElectricityCostToManager -= Obj_RemoveElectricityCostToManager;
        }

        private void ElectricItemRegistry_OnElectricItemRegister(IElectricItem obj)
        {
            AddItemToDictionnary(obj.ItemZone, obj);
            obj.AddElectricityCostToManager += Obj_AddElectricityCostToManager;
            obj.RemoveElectricityCostToManager += Obj_RemoveElectricityCostToManager;
        }

        private void Obj_RemoveElectricityCostToManager(float obj)
        {
            
        }

        private void Obj_AddElectricityCostToManager(float obj)
        {
            
        }

        #endregion

        #endregion

        #region Dictionnary
        private void AddItemToDictionnary(ElectricityZones zone, IElectricItem item)
        {
            if (!_electricZonesData.ContainsKey(zone))
                return;

            ElectricZoneData data = _electricZonesData[zone];

            if (!data.Items.Contains(item))
            {
                data.Items.Add(item);
                data.RecalculatePower();
                _electricZonesData[zone] = data; 
            }
        }

        private void RemoveItemFromDictionnary(ElectricityZones zone, IElectricItem item)
        {
            if (!_electricZonesData.ContainsKey(zone))
                return;

            ElectricZoneData data = _electricZonesData[zone];

            if (data.Items.Contains(item))
            {
                data.Items.Remove(item);
                data.RecalculatePower();
                _electricZonesData[zone] = data; 
            }
        }
        private List<IElectricItem> GetAllItemsInZonze(ElectricityZones zone)
        {
            return _electricZonesData.ContainsKey(zone) ? _electricZonesData[zone].Items : new List<IElectricItem>();
        }

        public float GetTotalPowerInZone(ElectricityZones zone)
        {
            return _electricZonesData.ContainsKey(zone) ? _electricZonesData[zone].PowerUsed : 0f;
        }

        public float GetCurrentTotalPower() 
        {
            float totalPower = 0f;
            foreach (KeyValuePair<ElectricityZones, ElectricZoneData> kvp in _electricZonesData) 
            {
                totalPower += GetTotalPowerInZone(kvp.Key);
            }
            return totalPower;
        }


        #endregion
    }

}
