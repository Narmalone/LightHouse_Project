using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Signals : NightWatchReportWindow
{
    [SerializeField] private SignalInfoElement SignalPrefab;
    [SerializeField] private SignalHistoryElement HistorySignalPrefab;
    [SerializeField] private RectTransform SignalParent;
    [SerializeField] private RectTransform HistoryParent;
    [SerializeField] private Sprite BoatIcon;
    [SerializeField] private Sprite BuoyIcon;
    [SerializeField] private Sprite ValidIcon;
    [SerializeField] private Sprite InvalidIcon;

    [SerializeField] private BoatAnomaliesDatabase BoatDB;
    [SerializeField] private BuyoncyAnomalyDatabase BuoyDB;

    private readonly Dictionary<string, SignalInfoElement> _active = new();

    private void Awake()
    {
        BoatDB.OnAnomalyAdded += OnAnomalyAdded;
        BoatDB.OnAnomalyRemoved += OnAnomalyRemoved;
        BuoyDB.OnAnomalyAdded += OnAnomalyAdded;
        BuoyDB.OnAnomalyRemoved += OnAnomalyRemoved;
    }

    private void OnDestroy()
    {
        BoatDB.OnAnomalyAdded -= OnAnomalyAdded;
        BoatDB.OnAnomalyRemoved -= OnAnomalyRemoved;
        BuoyDB.OnAnomalyAdded -= OnAnomalyAdded;
        BuoyDB.OnAnomalyRemoved -= OnAnomalyRemoved;
    }

    private void OnAnomalyAdded(ISignal model)
    {
        if (_active.ContainsKey(model.Key)) return;

        var icon = model is BoatAnomalyDatas ? BoatIcon
                 : model is BuyoncyAnomalyDatas ? BuoyIcon
                 : null;

        var ui = Instantiate(SignalPrefab, SignalParent);
        ui.Initialize(model, icon);

        ui.OnTimerEnded += HandleExpired;
        Debug.Log("cc");
        _active[model.Key] = ui;
    }

    private void HandleExpired(ISignal model)
    {
        // Génère l’historique “Invalid”
        var history = Instantiate(HistorySignalPrefab, HistoryParent);
        history.SetInfos(
            icon: model is BoatAnomalyDatas ? BoatIcon : BuoyIcon,
            arrivalDate: model.DisplayText,
            completionValidation: InvalidIcon
        );

        // Supprime en base selon le type concret
        /*if (model is BoatAnomalyDatas boat)
            BoatDB.RemoveAnomaly(boat.BoatName);
        else if (model is BuyoncyAnomalyDatas buoy)
            BuoyDB.RemoveAnomaly(buoy.ID);*/
        RemoveUI(model);
       /* if(model is BoatAnomalyDatas datas)
        {
            BoatDB.RemoveAnomaly(boat.BoatName);
        }*/
        Debug.Log("Active");
    }

    private void OnAnomalyRemoved(ISignal model)
    {
        var history = Instantiate(HistorySignalPrefab, HistoryParent);
        history.SetInfos(
            icon: model is BoatAnomalyDatas ? BoatIcon : BuoyIcon,
            arrivalDate: model.DisplayText,
            completionValidation: ValidIcon
        );
        RemoveUI(model);
    }

    private void RemoveUI(ISignal model)
    {
        if (_active.TryGetValue(model.Key, out var ui))
        {
            ui.OnTimerEnded -= HandleExpired;

            Destroy(ui.gameObject);
            _active.Remove(model.Key);
        }
    }

}
