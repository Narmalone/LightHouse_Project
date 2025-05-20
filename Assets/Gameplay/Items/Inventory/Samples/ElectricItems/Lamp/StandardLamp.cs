using System;
using UnityEngine;

namespace LightHouse.Electricity
{
    public class StandardLamp : MonoBehaviour, IElectricItem
    {
        #region EVENTS
        public event Action<ElectricityZones, float> AddElectricityCostToManager;
        public event Action<ElectricityZones, float> RemoveElectricityCostToManager;

        #endregion

        #region SERIALIZED / PROPERTIES
        [Header(" --- ELECTRICITY --- ")]
        [SerializeField] private Light _light;
        [field: SerializeField] public bool HasElectricity { get; set; }
        [field: SerializeField] public ElectricityZones ItemZone { get; set; }
        [field: SerializeField] public float ElectricityCost { get; set; } = 10.0f;

        #endregion

        #region UNITY LIFECYCLE
        private void Awake() => _light.gameObject.SetActive(false);
        private void Start() => ElectricItemRegistry.Register(this);
        private void OnDestroy() => ElectricItemRegistry.Unregister(this);

        #endregion

        #region ELECTRICITY
        public void OnElectricityZoneDisabled()  => UserTurnOff();
        public void OnElectricityZoneEnabled() { }

        public void UserTurnOn()
        {
            _light.gameObject.SetActive(true);
            AddElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        public void UserTurnOff()
        {
            _light.gameObject.SetActive(false);
            RemoveElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }
        #endregion
    }

}
