using LightHouse.Features.Nightwatch;
using LightHouse.Features.Signals;
using LightHouse.Features.TimeOfDay.TimeCore;
using LightHouse.Features.Weather.Ocean;
using UnityEngine;

namespace LightHouse.Features.Buyoncies
{
    public class BuyoncyManager : NotPersistentSingleton<BuyoncyManager>
    {
        public BuyoncyController[] Buyoncies => _buyoncies;
        [SerializeField] private SO_NightWatchConfiguration _nightWatchConfig;
        [SerializeField] private BuyoncyController[] _buyoncies;
        [SerializeField] private BuyoncyAnomalyDatabase _anomalyDatabase;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < _buyoncies.Length; i++)
            {
                var controller = _buyoncies[i];          // capture locale, pas 'i'
                controller.BuyoncyID = i + 1;
                controller.OnBroken += BuyoncyManager_OnBroken;
            }
            _anomalyDatabase.OnAnomalyRemoved += AnomalyDatabase_OnAnomalyRemoved;
        }

        private void Start()
        {
            foreach (BuyoncyController controller in _buyoncies)
            {
                if(OceanManager.Instance != null)
                    controller.Initialize(OceanManager.Instance.WaterSurfaceComponent);
            }
        }

        private void Update()
        {
            if (TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime,
                                          _nightWatchConfig.BuyoncysDecayStartHour,
                                          _nightWatchConfig.BuyoncysDecayEndHour))
            {
                foreach (BuyoncyController controller in _buyoncies) 
                {
                    controller.UpdateFromManager();
                }
            }
        }

        private void AnomalyDatabase_OnAnomalyRemoved(ISignal obj)
        {
            foreach (var buoy in _buyoncies)
            {
                if (obj is BuyoncyBreakdownDatas datas)
                {
                    if (datas.ID == buoy.BuyoncyID)
                    {
                        buoy.Repair();
                        buoy.HasBeenRepairedToday = true;
                        break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _buyoncies.Length; i++)
            {
                var controller = _buyoncies[i];
                controller.OnBroken -= BuyoncyManager_OnBroken;
            }
            _anomalyDatabase.ResetAnomalies();
            _anomalyDatabase.OnAnomalyRemoved -= AnomalyDatabase_OnAnomalyRemoved;
        }

        private void BuyoncyManager_OnBroken(BuyoncyController controller)
        {
            _anomalyDatabase.SetAnomaly(controller.BuyoncyID);
        }
    }
}

