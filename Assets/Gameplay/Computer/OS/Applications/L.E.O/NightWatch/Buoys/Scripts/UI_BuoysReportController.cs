using LightHouse.Game.DayNightSystem;
using LightHouse.Game.Computer.LEO.NightWatch.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{
    /// <summary>
    /// Contrôleur UI principal pour la gestion des rapports de bouées.
    /// Coordonne l'évaluation, l'affichage des résultats et l'état des bouées.
    /// </summary>
    public class UI_BuoysReportController : NightWatchReportWindow
    {
        #region Serialized Fields

        [Header("UI References")]
        [SerializeField] private UI_Buoy[] _buoys;
        [SerializeField] private BuyoncyAnomalyDatabase _anomalyDatabase;
        [SerializeField] private NightWatchSendDatas _sendDatasPrefab;
        [SerializeField] private Button _sendReportButton;
        [SerializeField] private Button _resetAllButton;
        [SerializeField] private UI_ReportElement _reportElementPrefab;

        [Header("Config")]
        [SerializeField] private SO_BuoyMoneyResults _moneyResultDatabase;

        /// <summary>
        /// Hook optionnel : permet à un système externe de demander une suppression forcée à <see cref="UI_Signals"/>.
        /// Param1 = key (ex: "01"), Param2 = generateHistory (true => yes, false => no), Param3 = asValid (true => valid, false => invalid).
        /// </summary>
        public event Action<string, bool, bool> OnBuoyReportFailed;

        #endregion

        #region Private Fields

        private readonly BuoyReportEvaluator _evaluator = new BuoyReportEvaluator();
        private int _attemptCount;
        public bool IsReportCompleted { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeBuoys();
            RegisterEventHandlers();

            _sendReportButton.interactable = false;
            IsReportCompleted = false;
        }

        private void OnDestroy()
        {
            UnregisterEventHandlers();
        }

        private void OnValidate()
        {
            _buoys = GetComponentsInChildren<UI_Buoy>();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise les bouées avec leur ID et événements.
        /// </summary>
        private void InitializeBuoys()
        {
            for (int i = 0; i < _buoys.Length; i++)
            {
                _buoys[i].ID = i + 1;
                _buoys[i].OnBuoyStateChanged += OnBuoyStateChanged;
            }
        }

        /// <summary>
        /// Souscrit aux événements des boutons et du système.
        /// </summary>
        private void RegisterEventHandlers()
        {
            _anomalyDatabase.OnAnomalyExpired += OnAnomalyExpired;
            _resetAllButton.onClick.AddListener(OnResetClicked);
            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            TimeHandlerData.OnTimeSegmentChanged += OnTimeSegmentChanged;
        }

        /// <summary>
        /// Désouscrit de tous les événements.
        /// </summary>
        private void UnregisterEventHandlers()
        {
            foreach (var buoy in _buoys)
                buoy.OnBuoyStateChanged -= OnBuoyStateChanged;

            _resetAllButton.onClick.RemoveListener(OnResetClicked);
            _sendReportButton.onClick.RemoveListener(OnSendReportClicked);
            TimeHandlerData.OnTimeSegmentChanged -= OnTimeSegmentChanged;
            _anomalyDatabase.OnAnomalyExpired -= OnAnomalyExpired;
        }

        #endregion

        #region Event Handlers

        private void OnAnomalyExpired(BuyoncyAnomalyDatas datas)
        {
            var buoy = _buoys.FirstOrDefault(b => b.ID == datas.ID);
            if (buoy != null)
            {
                buoy.SwitchTo(UI_BuoyState.Expired);
                buoy.HasBeenReportedToday = true;
            }
        }

        private void OnBuoyStateChanged(UI_Buoy buoy)
        {
            UpdateSendReportButton();
        }

        private void OnTimeSegmentChanged(TimeOfDaySegment segment)
        {
            if (segment != TimeOfDaySegment.Morning) return;

            ResetBuoysState();
            _sendReportButton.interactable = false;
            IsReportCompleted = false;
            _attemptCount = 0;
        }

        private void OnSendReportClicked()
        {
            EvaluateAndShowResults();
            _sendReportButton.interactable = false;
        }

        private void OnResetClicked()
        {
            ResetBuoysState();
            _sendReportButton.interactable = false;
            IsReportCompleted = false;
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Active/désactive le bouton de rapport en fonction de l'état des bouées.
        /// </summary>
        private void UpdateSendReportButton()
        {
            bool canSend = _buoys.All(b => b.CurrentState != UI_BuoyState.Unchecked);
            _sendReportButton.interactable = canSend;
        }

        /// <summary>
        /// Met toutes les bouées à un état donné et réinitialise leur rapport du jour.
        /// </summary>
        private void ResetBuoysState()
        {
            foreach (var buoy in _buoys)
            {
                if (buoy.HasBeenReportedToday) continue;
                buoy.SwitchTo(UI_BuoyState.Unchecked);
                buoy.HasBeenReportedToday = false;
            }
        }

        /// <summary>
        /// Indique si toutes les bouées ont été reportées.
        /// </summary>
        public bool ReportCompleted() =>
            _buoys.All(b => b.HasBeenReportedToday);

        #endregion

        #region Core Flow

        /// <summary>
        /// Évalue les rapports de bouées et affiche les résultats.
        /// </summary>
        private void EvaluateAndShowResults()
        {
            var anomalies = _anomalyDatabase.GetAnomalies();
            var anomalyMap = anomalies.ToDictionary(a => a.ID, a => a.RemainingTime);
            var allAnomalyIds = anomalyMap.Keys.ToList(); // ✅ tous les IDs, pas juste ceux à supprimer

            var result = _evaluator.Evaluate(_buoys, anomalyMap);

            // Passer la vérité terrain complète
            ApplyEvaluationEffects(result, allAnomalyIds);

            ShowResultsPopup(result);
            _attemptCount = (result.ErrorCount > 0) ? _attemptCount + 1 : 0;
        }


        /// <summary>
        /// Applique les changements d'état UI en fonction des résultats.
        /// </summary>
        private void ApplyEvaluationEffects(BuoyReportResult result, IReadOnlyCollection<int> allAnomalyIds)
        {
            foreach (var buoy in _buoys)
            {
                if (buoy.CurrentState == UI_BuoyState.Unchecked || buoy.HasBeenReportedToday)
                    continue;

                bool shouldBeInvalid = allAnomalyIds.Contains(buoy.ID); // ✅ vérité terrain
                bool isMarkedInvalid = buoy.CurrentState == UI_BuoyState.Invalid;
                bool isMarkedValid = buoy.CurrentState == UI_BuoyState.Valid;

                bool isCorrect = (shouldBeInvalid && isMarkedInvalid) || (!shouldBeInvalid && isMarkedValid);

                if (isCorrect)
                {
                    buoy.HasBeenReportedToday = true;
                    buoy.SwitchTo(UI_BuoyState.Reported);
                }
                else
                {
                    buoy.SwitchTo(UI_BuoyState.Failed);
                    //To doo:: faire en sorte de forcer la suppression de l'anomalie
                    OnBuoyReportFailed?.Invoke(buoy.ID.ToString(), true, true);
                }
            }

            if (result.AnomalyIdsToRemove.Count > 0)
                _anomalyDatabase.RemoveAnomalies(result.AnomalyIdsToRemove.ToList());
        }


        /// <summary>
        /// Instancie et configure le popup de résultats.
        /// </summary>
        private void ShowResultsPopup(BuoyReportResult result)
        {
            var popup = Instantiate(_sendDatasPrefab, _nightWatch.transform as RectTransform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;

            popup.OnLoadingCompleted += status =>
            {
                if (status == DataStatus.Success)
                {
                    GenerateReportElements(
                        popup,
                        popup.SuccessContent,
                        result.CorrectValidCount,
                        result.CorrectInvalidCount,
                        result.ErrorCount,
                        result.RemainingTimesForCorrectInvalid);

                    popup.RefreshLayouts();
                    IsReportCompleted = ReportCompleted();
                }
            };

            popup.StartLoading(() => DataStatus.Success);
        }

        /// <summary>
        /// Génère les éléments UI affichant les gains/pertes.
        /// </summary>
        private void GenerateReportElements(
            NightWatchSendDatas datas,
            RectTransform parent,
            int correctValidBuoys,
            int correctInvalidBuoys,
            int errorsCount,
            IReadOnlyList<float> times)
        {
            float total = 0;
            if (correctValidBuoys > 0)
            {
                int correctValidAmount = MoneyCalculator.ValidFlat(correctValidBuoys, _moneyResultDatabase);
                CreateReportElement(parent, $"Correct Valid Buoys x{correctValidBuoys}",
                    $"+ {correctValidAmount}$", Color.green);
                total += correctValidAmount;
            }

            if (correctInvalidBuoys > 0)
            {
                int correctInvalidAmount = MoneyCalculator.InvalidFlat(correctInvalidBuoys, _moneyResultDatabase);
                CreateReportElement(parent, $"Correct Invalid Buoys x{correctInvalidBuoys}",
                    $"+ {correctInvalidAmount}$", Color.green);
                int bonus = MoneyCalculator.BonusFromTimes(times, _anomalyDatabase.TimeToReportAnomalies, _moneyResultDatabase);
                CreateReportElement(parent, "Correct invalid speed report", $"+ {bonus}$", Color.green);
                total += correctInvalidAmount + bonus;
            }

            if (errorsCount > 0)
            {
                int missmatchAmount = MoneyCalculator.MissmatchFlat(errorsCount, _moneyResultDatabase);
                CreateReportElement(parent, $"Missmatch Buoys: {errorsCount}",
                    $"- {missmatchAmount}$", Color.red);
                total -= missmatchAmount;
            }

            //Total
            CreateReportElement(parent, "Total:", $"{total}", total > 0 ? Color.green : Color.red);
        }

        /// <summary>
        /// Instancie et configure un élément de rapport.
        /// </summary>
        private void CreateReportElement(RectTransform parent, string reason, string amount, Color color)
        {
            var element = Instantiate(_reportElementPrefab, parent.transform);
            element.SetReason(reason);
            element.SetMoneyResult(amount, color);
        }

        #endregion
    }
}
