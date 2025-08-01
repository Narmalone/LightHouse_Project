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
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _buyoncies.Length; i++)
        {
            var controller = _buyoncies[i];
            controller.OnBroken -= BuyoncyManager_OnBroken;
        }
        _anomalyDatabase.ResetAnomalies();
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
