using UnityEngine;
using LightHouse.Features.Signals;

namespace LightHouse.Features.Buyoncies
{
    public class BuyoncyManager : PersistentSingleton<BuyoncyManager>
    {
        public BuyoncyController[] Buyoncies => _buyoncies;
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

