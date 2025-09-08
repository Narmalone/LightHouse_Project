using LightHouse.Game.Buyoncies;
using LightHouse.Game.Computer.LEO.NightWatch.Signals;
using LightHouse.Money;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{

    public struct MoneyLine
    {
        public string Label;
        public int Amount;       // positif ou négatif
        public Color Color;

        public MoneyLine(string label, int amount, Color color)
        {
            Label = label;
            Amount = amount;
            Color = color;
        }
    }

    public struct MoneyBreakdown
    {
        public int Total;
        public List<MoneyLine> Lines;
    }


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
        [SerializeField] private UI_ReportDatasPopup _sendDatasPrefab;
        [SerializeField] private Button _sendReportButton;
        [SerializeField] private Button _resetAllButton;
        [SerializeField] private UI_ReportElement _reportElementPrefab;

        [Header("Config")]
        [SerializeField] private SO_BuoyMoneyResults _moneyResultDatabase;

        public BuoyReportResult CurrentTodayResult;
        public MoneyBreakdown TodayMoneyBreakdown { get; private set; } // “global” pour la session courante

        /// <summary>
        /// Hook optionnel : permet à un système externe de demander une suppression forcée à <see cref="UI_Signals"/>.
        /// Param1 = key (ex: "01"), Param2 = generateHistory (true => yes, false => no), Param3 = asValid (true => valid, false => invalid).
        /// </summary>
        public event Action<string, bool, bool> OnBuoyReportFailed;

        #endregion

        #region Private Fields

        private readonly BuoyReportEvaluator _evaluator = new BuoyReportEvaluator();
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


        public void OnNightwatchEndedToday()
        {
            ResetBuoysState();
            _sendReportButton.interactable = false;
            IsReportCompleted = false;
            CurrentTodayResult = null;
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

        public BuoyReportResult GetTodaysResult()
        {
            //means the player didn't do anything with the buoys during the night
            if (!IsReportCompleted)
            {
                //CurrentTodayResult = new BuoyReportResult(0, 0, _buoys.Length, new List<float> { -1 }, new List<int> { -1 });
                EvaluateAndShowResults(false);
                Debug.Log("aucuns report n'a été réalisé cette nuit. Génération");
            }
            return CurrentTodayResult;
        }

        /// <summary>
        /// Évalue les rapports de bouées et affiche les résultats.
        /// </summary>
        private void EvaluateAndShowResults(bool showResultPopup = true)
        {
            var anomalies = _anomalyDatabase.GetAnomalies();
            var anomalyMap = anomalies.ToDictionary(a => a.ID, a => a.RemainingTime);
            var allAnomalyIds = anomalyMap.Keys.ToList();

            // 1) Eval
            CurrentTodayResult = _evaluator.Evaluate(_buoys, anomalyMap);

            // 2) Appliquer les effets d’état UI (report/failed/suppressions)
            ApplyEvaluationEffects(CurrentTodayResult, allAnomalyIds);

            // 3) CALCUL ARGENT (ici, hors UI) + side-effects gameplay
            TodayMoneyBreakdown = CalculateMoney(CurrentTodayResult);  // <-- calcul “global”
            PlayerCurrency.Add(TodayMoneyBreakdown.Total);             // <-- applique économie ici
            CurrentTodayResult.TotalEarnedDuringTheNight = TodayMoneyBreakdown.Total;

            // 4) UI : uniquement du rendu
            if(showResultPopup)
                ShowResultsPopup(CurrentTodayResult, TodayMoneyBreakdown);
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
        private void ShowResultsPopup(BuoyReportResult result, MoneyBreakdown breakdown)
        {
            var popup = Instantiate(_sendDatasPrefab, _nightWatch.transform as RectTransform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;

            popup.OnLoadingCompleted += status =>
            {
                if (status == DataStatus.Success)
                {
                    GenerateReportElements(
                        popup,
                        popup.BodyParentContent,
                        breakdown); // <-- on passe le breakdown déjà calculé

                    popup.RefreshLayouts();
                    IsReportCompleted = ReportCompleted();
                }
            };

            popup.StartLoading(() => DataStatus.Success);
        }


        /// <summary>
        /// Génère les éléments UI affichant les gains/pertes.
        /// </summary>
        private void GenerateReportElements(UI_ReportDatasPopup datas, RectTransform parent, MoneyBreakdown breakdown)
        {
            // 1) Rendu UI des lignes (aucun calcul ici)
            foreach (var line in breakdown.Lines)
            {
                string sign = line.Amount >= 0 ? "+ " : "- ";
                int abs = Mathf.Abs(line.Amount);
                CreateReportElement(parent, line.Label, $"{sign}{abs}$", line.Color);
            }

            // 2) Ligne Total
            var totalColor = breakdown.Total >= 0 ? Color.green : Color.red;
            string totalSign = breakdown.Total >= 0 ? "+ " : "- ";
            int totalAbs = Mathf.Abs(breakdown.Total);
            CreateReportElement(parent, "Total:", $"{totalSign}{totalAbs}$", totalColor);
        }



        private MoneyBreakdown CalculateMoney(BuoyReportResult result)
        {
            var lines = new List<MoneyLine>();
            int total = 0;

            if (result.CorrectValidCount > 0)
            {
                int correctValidAmount = BuoyMoneyCalculator.ValidFlat(result.CorrectValidCount, _moneyResultDatabase);
                lines.Add(new MoneyLine($"Correct Valid Buoys x{result.CorrectValidCount}", +correctValidAmount, Color.green));
                total += correctValidAmount;
            }

            if (result.CorrectInvalidCount > 0)
            {
                int correctInvalidAmount = BuoyMoneyCalculator.InvalidFlat(result.CorrectInvalidCount, _moneyResultDatabase);
                lines.Add(new MoneyLine($"Correct Invalid Buoys x{result.CorrectInvalidCount}", +correctInvalidAmount, Color.green));

                int bonus = BuoyMoneyCalculator.BonusFromTimes(
                    result.RemainingTimesForCorrectInvalid,
                    _anomalyDatabase.TimeToReportAnomalies,
                    _moneyResultDatabase);

                if (bonus != 0)
                    lines.Add(new MoneyLine("Correct invalid speed report", +bonus, Color.green));

                total += correctInvalidAmount + bonus;
            }

            if (result.ErrorCount > 0)
            {
                int mismatchAmount = BuoyMoneyCalculator.MissmatchFlat(result.ErrorCount, _moneyResultDatabase);
                lines.Add(new MoneyLine($"Missmatch Buoys: {result.ErrorCount}", -mismatchAmount, Color.red));
                total -= mismatchAmount;
            }

            return new MoneyBreakdown { Total = total, Lines = lines };
        }


        /// <summary>
        /// Instancie et configure un élément de rapport.
        /// </summary>
        private void CreateReportElement(RectTransform parent, string reason, string amount, Color color)
        {
            var element = Instantiate(_reportElementPrefab, parent.transform);
            element.SetDescription(reason);
            element.SetMoneyResult(amount, color);
        }

        #endregion
    }
}
