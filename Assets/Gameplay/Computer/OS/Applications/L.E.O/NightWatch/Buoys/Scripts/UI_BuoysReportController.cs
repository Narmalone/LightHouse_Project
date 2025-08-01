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

    private void Awake()
    {
        for(int i = 0; i < _buoys.Length; i++)
        {
            _buoys[i].ID = i + 1;
            _buoys[i].OnBuoyCliqued += UI_BuoysReportController_OnBuoyCliqued; 
        }

        _resetAllButton.onClick.AddListener(OnResetCliqued);
        _sendReportButton.onClick.AddListener(OnSendReportCliqued);
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

        // 2. Pour chaque bouée UI, on compare et on met à jour l'état
        foreach (var buoy in _buoys)
        {
            bool shouldBeInvalid = anomalyIds.Contains(buoy.ID);
            bool isMarkedInvalid = buoy.CurrentState == UI_BuoyState.Invalid;
            bool isCorrect = false;

            if (shouldBeInvalid)
            {
                // Devrait être invalid → on met en Invalid
                buoy.SwitchTo(UI_BuoyState.Invalid);

                if (isMarkedInvalid)
                {
                    correctCount++;
                    isCorrect = true;
                }
                else
                    errorCount++;
            }
            else
            {
                // Devrait être valid → on met en Valid
                buoy.SwitchTo(UI_BuoyState.Valid);

                if (!isMarkedInvalid)
                {
                    correctCount++;
                    isCorrect = true;
                }
                else
                    errorCount++;
            }

            if (isCorrect)
            {
                _anomalyDatabase.RemoveAnomaly(buoy.ID);
            }
        }

        Debug.Log($"Validation terminée : {correctCount} correct(s), {errorCount} erreur(s).");

        // 3. Affiche la fenêtre de résultat
        var resultWindow = Instantiate(_sendDatasPrefab, transform as RectTransform);
        resultWindow.IsSuccessfull = (errorCount == 0);
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
    }

    private void OnValidate()
    {
        _buoys = GetComponentsInChildren<UI_Buoy>();
    }
}
