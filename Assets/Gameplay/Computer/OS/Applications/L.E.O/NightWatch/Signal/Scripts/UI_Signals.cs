using UnityEngine;

public class UI_Signals : NightWatchReportWindow
{
    [SerializeField] private SignalInfoController SignalPrefab;
    [SerializeField] private RectTransform _signalLayoutParent;
    [SerializeField] private BoatAnomaliesDatabase _boatAnomaliesDatabase;

    private void Awake()
    {
        _boatAnomaliesDatabase.OnAnomalyAdded += BoatAnomaliesDatabase_OnAnomalyAdded;
    }

    private void OnDestroy()
    {
        _boatAnomaliesDatabase.OnAnomalyAdded += BoatAnomaliesDatabase_OnAnomalyAdded;
    }

    private void BoatAnomaliesDatabase_OnAnomalyAdded()
    {
        var instance = Instantiate(SignalPrefab, _signalLayoutParent);
        //Instantiate the prefab
        //Get the arrival date
        //set up the timer (careful about timers, if we leave application it's still running) !
        //subscribe to the signal info and update it's states
    }
}
