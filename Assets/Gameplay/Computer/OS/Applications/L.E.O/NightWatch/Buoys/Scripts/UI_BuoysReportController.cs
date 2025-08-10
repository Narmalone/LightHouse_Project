using LightHouse.Game.DayNightSystem;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuoysReportController : NightWatchReportWindow
{
    [SerializeField] private UI_Buoy[] _buoys;
    [SerializeField] private BuyoncyAnomalyDatabase _anomalyDatabase;
    [SerializeField] private NightWatchSendDatas _sendDatasPrefab;
    [SerializeField] private Button _sendReportButton;
    [SerializeField] private Button _resetAllButton;
    private UI_Buoy _lastSelectedBuoy;
    private int _attemptCount = 0;
    public bool IsReportCompleted = false;

    private void Awake()
    {
        for(int i = 0; i < _buoys.Length; i++)
        {
            _buoys[i].ID = i + 1;
            _buoys[i].OnBuoyCliqued += UI_BuoysReportController_OnBuoyCliqued; 
        }

        _resetAllButton.onClick.AddListener(OnResetCliqued);
        _sendReportButton.onClick.AddListener(OnSendReportCliqued);
        TimeHandlerData.OnTimeSegmentChanged += OnTimeSegmentChanged;
    }

    private void OnTimeSegmentChanged(TimeOfDaySegment segment)
    {
        if (segment != TimeOfDaySegment.Morning) return;
        foreach(var buoy in _buoys)
        {
            buoy.SwitchTo(UI_BuoyState.Unchecked);
            buoy.HasBeenReportedToday = false;
        }
        IsReportCompleted = false;
    }

    private void OnSendReportCliqued()
    {
        CompareValidations();
    }

    public bool ReportCompleted()
    {
        int reportCount = 0;
        foreach(var buoy in _buoys)
        {
            if(buoy.HasBeenReportedToday) 
                reportCount++;
        }
        return reportCount >= _buoys.Length;
    }

    private void CompareValidations()
    {
        var anomalyIds = _anomalyDatabase
            .GetAnomalies()
            .Select(a => a.ID)
            .ToHashSet();

        int correctCount = 0;
        int errorCount = 0;

        // On collecte les IDs d'anomalies à retirer après la boucle
        var idsToRemove = new System.Collections.Generic.List<int>();

        foreach (var buoy in _buoys)
        {
            bool shouldBeInvalid = anomalyIds.Contains(buoy.ID);
            var state = buoy.CurrentState;

            // Ignore les unchecked / déjà reportées si c'est ton souhait
            if (state == UI_BuoyState.Unchecked || buoy.HasBeenReportedToday)
                continue;

            bool isMarkedInvalid = state == UI_BuoyState.Invalid;
            bool isMarkedValid = state == UI_BuoyState.Valid;
            bool isCorrect = false;

            if (shouldBeInvalid && isMarkedInvalid)
            {
                correctCount++;
                isCorrect = true;
                idsToRemove.Add(buoy.ID);              // ✅ on marque pour suppression plus tard
            }
            else if (!shouldBeInvalid && isMarkedValid)
            {
                correctCount++;
                isCorrect = true;
            }
            else
            {
                errorCount++;
                buoy.SwitchTo(UI_BuoyState.Failed);
            }

            if (isCorrect)
            {
                buoy.HasBeenReportedToday = true;
                buoy.SwitchTo(UI_BuoyState.Reported);
            }
        }

        // ⬇️ suppression APRÈS la boucle (évite les effets de bord et reentrancy)
        if (idsToRemove.Count > 0)
            _anomalyDatabase.RemoveAnomalies(idsToRemove);

        Debug.Log($"Validation terminée : {correctCount} correct(s), {errorCount} erreur(s).");

        var resultWindow = Instantiate(_sendDatasPrefab, transform as RectTransform);
        resultWindow.IsSuccessfull = (errorCount == 0);

        IsReportCompleted = ReportCompleted();
        _attemptCount = (errorCount > 0) ? _attemptCount + 1 : 0;
    }

    private void OnResetCliqued()
    {
        SetAllBuoysState(UI_BuoyState.Unchecked);
        _lastSelectedBuoy = null;
    }

    public void SetAllBuoysState(UI_BuoyState toState)
    {
        foreach(var buoy in _buoys)
        {
            buoy.SwitchTo(toState);
        }
    }

    private void UI_BuoysReportController_OnBuoyCliqued(UI_Buoy obj)
    {
        _lastSelectedBuoy = obj;
    }

    private void OnDestroy()
    {
        foreach(var buoy in _buoys)
        {
            buoy.OnBuoyCliqued -= UI_BuoysReportController_OnBuoyCliqued;
        }
        _resetAllButton.onClick.RemoveListener(OnResetCliqued);
        _sendReportButton.onClick.RemoveListener(OnSendReportCliqued);
        TimeHandlerData.OnTimeSegmentChanged -= OnTimeSegmentChanged;
    }

    private void OnValidate()
    {
        _buoys = GetComponentsInChildren<UI_Buoy>();
    }
}
