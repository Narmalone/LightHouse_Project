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
        }
    }

    private void OnSendReportCliqued()
    {
        CompareValidations();
    }

    private void CompareValidations()
    {
        // 1. IDs réellement en anomalie
        var anomalyIds = _anomalyDatabase
            .GetAnomalies()
            .Select(a => a.ID)
            .ToHashSet();

        int correctCount = 0;
        int errorCount = 0;

        foreach (var buoy in _buoys)
        {
            bool shouldBeInvalid = anomalyIds.Contains(buoy.ID);
            var state = buoy.CurrentState;

            // On ne compte que si le joueur a marqué la bouée (Valid ou Invalid)
            if (state == UI_BuoyState.Unchecked || buoy.HasBeenReportedToday)
                continue;

            bool isMarkedInvalid = state == UI_BuoyState.Invalid;
            bool isMarkedValid = state == UI_BuoyState.Valid;

            if (shouldBeInvalid && isMarkedInvalid)
            {
                // Anomalie + Invalid → correct
                correctCount++;
                _anomalyDatabase.RemoveAnomaly(buoy.ID);
                buoy.HasBeenReportedToday = true;
                buoy.SwitchTo(UI_BuoyState.Reported);
            }
            else if (!shouldBeInvalid && isMarkedValid)
            {
                // Pas d'anomalie + Valid → correct
                correctCount++;
            }
            else
            {
                // Tout autre combinaison ("Invalid" là où il n'y a pas d'anomalie,
                // ou "Valid" là où il y a une anomalie) → erreur
                errorCount++;
                buoy.SwitchTo(UI_BuoyState.Failed);
            }
        }

        Debug.Log($"Validation terminée : {correctCount} marquage(s) correct(s), {errorCount} erreur(s).");

        // Affiche la fenêtre de résultat
        var resultWindow = Instantiate(_sendDatasPrefab, transform as RectTransform);
        resultWindow.IsSuccessfull = (errorCount == 0);

        // Gestion du compteur de tentatives
        if (errorCount > 0) _attemptCount++;
        else _attemptCount = 0;
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
        Debug.Log("Cliqued from Buoys Report window");
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
