using UnityEngine;

public class BuyoncyManager : MonoBehaviour
{
    [SerializeField] private BuyoncyController[] _buyoncies;
    [SerializeField] private BuyoncyAnomalyDatabase _anomalyDatabase;

    private void Awake()
    {
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
        foreach(var buoy in _buyoncies)
        {
            if(obj is BuyoncyAnomalyDatas datas)
            {
                if(datas.ID == buoy.BuyoncyID)
                {
                    buoy.Repaired();
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

    private void OnValidate()
    {
        _buyoncies = GetComponentsInChildren<BuyoncyController>();
    }
}
