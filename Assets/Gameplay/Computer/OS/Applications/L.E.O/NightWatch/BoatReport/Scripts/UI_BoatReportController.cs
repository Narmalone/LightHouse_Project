using LightHouse.Game.Boats;
using LightHouse.Money;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Boats
{
    [System.Serializable]
    public struct MoneyBoatLine
    {
        public string Label;
        public Color LabelColor;
        public int Value; // peut être négatif pour les pénalités

        public MoneyBoatLine(string label, Color color, int amount)
        {
            Label = label;
            LabelColor = color;
            Value = amount;
        }
    }

    // Un bateau = 3 lignes (flat, bonus, pénalité)
    [System.Serializable]
    public struct MoneyBoatBreakdown
    {
        public string BoatName; // pratique pour l'affichage "par bateau"
        public MoneyBoatLine FlatAmountWon;   // ex: +200
        public MoneyBoatLine BonusFromTime;   // ex: +50
        public MoneyBoatLine NumberOfTry;     // ex: -60  (pénalité)

        public MoneyBoatBreakdown(
            string boatName,
            MoneyBoatLine flatLineInfo,
            MoneyBoatLine bonusFromTimeInfo,
            MoneyBoatLine numberOfTryLine)
        {
            BoatName = boatName;
            FlatAmountWon = flatLineInfo;
            BonusFromTime = bonusFromTimeInfo;
            NumberOfTry = numberOfTryLine;
        }

        public int TotalForThisBoat()
            => FlatAmountWon.Value + BonusFromTime.Value + NumberOfTry.Value;
    }

    // Cumul de plusieurs bateaux pour la session/nuit
    [System.Serializable]
    public struct MoneyAllBoatsBreakdown
    {
        public List<MoneyBoatBreakdown> AllBoats;

        public void EnsureInit()
        {
            if (AllBoats == null) AllBoats = new List<MoneyBoatBreakdown>();
        }

        public void Add(MoneyBoatBreakdown boatBk)
        {
            EnsureInit();
            AllBoats.Add(boatBk);
        }

        public void Reset()
        {
            EnsureInit();
            AllBoats.Clear();
        }

        public int GetTotalFlat()
        {
            EnsureInit();
            int total = 0;
            foreach (var b in AllBoats) total += b.FlatAmountWon.Value;
            return total;
        }

        public int GetTotalBonusFromTime()
        {
            EnsureInit();
            int total = 0;
            foreach (var b in AllBoats) total += b.BonusFromTime.Value;
            return total;
        }

        public int GetTotalNumberOfTry()
        {
            EnsureInit();
            int total = 0;
            foreach (var b in AllBoats) total += b.NumberOfTry.Value;
            return total;
        }

        public int GetGrandTotal()
            => GetTotalFlat() + GetTotalBonusFromTime() + GetTotalNumberOfTry();
    }

    /// <summary>
    /// Contrôleur UI pour le rapport d'anomalie sur les bateaux.
    /// Gère la sélection du nom, du drapeau, du type d'anomalie, et l'envoi de rapport.
    /// </summary>
    public class UI_BoatReportController : NightWatchReportWindow
    {
        #region Serialized Fields

        [Header("Data References")]
        [SerializeField] private BoatsNationalitiesManager _nationalityManager;
        [SerializeField] private BoatAnomaliesDatabase _anomalyDatabase;
        [SerializeField] private SO_BoatMoneyResults _moneyResultDatabase; // équivalent à _moneyResultDatabase des bouées

        [Header("UI Elements - Anomaly Selection")]
        [SerializeField] private AnomalyReportButton[] _anomalyButtons;
        [SerializeField] private UI_BoatsLowestTimerAlert _anomalyTimerController;

        [Header("UI Elements - Flags")]
        [SerializeField] private TMP_Dropdown _nationalitiesDropdown;
        [SerializeField] private Image _dropdownFlag;
        [SerializeField] private Image _summaryFlag;

        [Header("UI Elements - Boat Name")]
        [SerializeField] private TMP_InputField _boatNameInputField;

        [Header("UI Elements - Boat Frequency")]
        [SerializeField] private TMP_InputField _boatFrequencyInputField;

        [Header("UI Elements - Summary")]
        [SerializeField] private TextMeshProUGUI _summaryReportText;

        [Header("UI Elements - Actions")]
        [SerializeField] private Button _sendReportButton;
        [SerializeField] private Button _resetAllButton;

        [Header("UI Prefabs")]
        [SerializeField] private UI_ReportDatasPopup _sendDatasPrefab;
        [SerializeField] private UI_ReportElement _reportElementPrefab;

        [SerializeField] private AudioCue _keyboardCue;
        #endregion

        #region Private Fields

        private string _boatNameInput;
        private float _selectedBoatFrequency;
        private AnomalyType _selectedAnomalyType;
        private string _selectedAnomalyText;
        private Sprite _selectedFlag;

        public MoneyAllBoatsBreakdown TodayAllBoatsBreakdown { get; private set; }

        #endregion

        #region Public Properties

        public string BoatNameInput => _boatNameInput;
        public AnomalyType SelectedAnomaly => _selectedAnomalyType;

        public bool IsReportedToday = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            RegisterUIEvents();
            InitializeDropdownOptions();
            InitializeAnomalyButtons();
            _sendReportButton.interactable = false;

            TodayAllBoatsBreakdown.EnsureInit();
        }

        private void OnDestroy()
        {
            UnregisterUIEvents();
        }

        private void OnValidate()
        {
            _anomalyButtons = GetComponentsInChildren<AnomalyReportButton>();
        }

        #endregion

        #region UI Setup

        /// <summary>
        /// Initialise les options du dropdown des drapeaux.
        /// </summary>
        private void InitializeDropdownOptions()
        {
            _dropdownFlag.sprite = null;
            var options = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData { text = "None" } };

            foreach (var config in _nationalityManager.PossibleConfigs)
            {
                options.Add(new TMP_Dropdown.OptionData
                {
                    text = config.NationalityName,
                    image = config.Flag
                });
            }

            _summaryFlag.gameObject.SetActive(false);
            _dropdownFlag.gameObject.SetActive(false);

            _nationalitiesDropdown.ClearOptions();
            _nationalitiesDropdown.AddOptions(options);
            _nationalitiesDropdown.value = 0;
        }

        /// <summary>
        /// Abonne les boutons d'anomalie à la mise à jour de l'état sélectionné.
        /// </summary>
        private void InitializeAnomalyButtons()
        {
            foreach (var anomalyButton in _anomalyButtons)
            {
                var btn = anomalyButton;
                btn.AnomalyButton.onClick.AddListener(() =>
                {
                    _selectedAnomalyText = btn.AnomalyText.text;
                    _selectedAnomalyType = btn.AnomalyDefinition.Type;
                    UpdateSummaryReport();
                    OnBoatsUIEventsChanged();
                });
            }
        }

        #endregion

        #region Event Registration

        private void RegisterUIEvents()
        {
            _boatNameInputField.onValueChanged.AddListener(OnBoatNameChanged);
            _nationalitiesDropdown.onValueChanged.AddListener(OnFlagDropdownChanged);
            _boatFrequencyInputField.onValueChanged.AddListener(OnBoatFrequencyChanged);

            _boatFrequencyInputField.onEndEdit.AddListener(OnBoatFrequencyChanged);
            _boatNameInputField.onEndEdit.AddListener(OnBoatNameChanged);

            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            _resetAllButton.onClick.AddListener(OnResetAllClicked);
        }

        private void UnregisterUIEvents()
        {
            _boatNameInputField.onValueChanged.RemoveListener(OnBoatNameChanged);
            _boatNameInputField.onEndEdit.RemoveListener(OnBoatNameChanged);
            _nationalitiesDropdown.onValueChanged.RemoveListener(OnFlagDropdownChanged);
            _boatFrequencyInputField.onValueChanged.RemoveListener(OnBoatFrequencyChanged);
            _boatFrequencyInputField.onEndEdit.RemoveListener(OnBoatFrequencyChanged);

            _sendReportButton.onClick.RemoveListener(OnSendReportClicked);
            _resetAllButton.onClick.RemoveListener(OnResetAllClicked);
        }

        #endregion

        #region UI Event Handlers

        private void OnBoatsUIEventsChanged()
        {
            if (_boatNameInputField.text.Length <= 0 || _boatFrequencyInputField.text.Length <= 0 ||
                 _nationalitiesDropdown.value < 0 || _selectedAnomalyText == string.Empty)
                _sendReportButton.interactable = false;
            else if (_boatNameInputField.text.Length > 0 && _boatFrequencyInputField.text.Length > 0 &&
                 _nationalitiesDropdown.value > 0 && _selectedAnomalyText != string.Empty)
                _sendReportButton.interactable = true;
        }

        private void OnBoatNameChanged(string name)
        {
            if (ServiceLocator.Audio != null && _keyboardCue)
            {
                ServiceLocator.Audio.PlayAt(_keyboardCue, this.transform.position);
            }
            _boatNameInput = name;
            UpdateSummaryReport();
            OnBoatsUIEventsChanged();
        }

        private void OnBoatFrequencyChanged(string arg0)
        {
            Debug.Log(arg0);
            if (ServiceLocator.Audio != null && _keyboardCue)
            {
                ServiceLocator.Audio.PlayAt(_keyboardCue, this.transform.position);
            }
            if (FloatExtension.TryParse(arg0, out float result))
            {
                _selectedBoatFrequency = result;
                //know if it's valid
                if (_anomalyDatabase.TryGetAnomaly(result, out BoatAnomalyDatas datas))
                {
                    Debug.Log($"bateau avec nomalie trouvé à la fréquence: {result}, son nom est {datas.BoatName}");
                }
            }
            else
            {
                _selectedBoatFrequency = 0.0f;
            }
            OnBoatsUIEventsChanged();
        }

        private void OnFlagDropdownChanged(int index)
        {
            if (index <= 0)
            {
                _selectedFlag = null;
                _summaryFlag.gameObject.SetActive(false);
                _dropdownFlag.gameObject.SetActive(false);
            }
            else
            {
                _selectedFlag = _nationalityManager.PossibleConfigs[index - 1].Flag;
                _dropdownFlag.gameObject.SetActive(true);
                _summaryFlag.gameObject.SetActive(true);
                _dropdownFlag.sprite = _selectedFlag;
                _summaryFlag.sprite = _selectedFlag;
            }

            UpdateSummaryReport();
            OnBoatsUIEventsChanged();
        }

        private void OnResetAllClicked()
        {
            _nationalitiesDropdown.value = 0;
            _boatNameInputField.SetTextWithoutNotify(string.Empty);
            _boatFrequencyInputField.SetTextWithoutNotify(string.Empty);

            _boatNameInput = string.Empty;
            _selectedFlag = null;
            _selectedAnomalyText = string.Empty;

            _summaryFlag.gameObject.SetActive(false);
            _dropdownFlag.gameObject.SetActive(false);

            UpdateSummaryReport();
        }

        private void OnSendReportClicked()
        {
            EvaluateAndShowResults();
        }

        #endregion

        #region Private Helpers

        // --------------------------------------------
        // Evaluation principale
        // --------------------------------------------
        private void EvaluateAndShowResults()
        {
            var popup = Instantiate(_sendDatasPrefab, _nightWatch.transform as RectTransform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;
            _boatNameInput = _boatNameInputField.text;
            FloatExtension.TryParse(_boatNameInputField.text, out _selectedBoatFrequency);
            popup.OnLoadingCompleted += status =>
            {
                if (status != DataStatus.Success) return;

                // Choix du bateau (nom prioritaire, sinon fréquence)
                BoatAnomalyDatas picked = null;
                if (_anomalyDatabase.TryGetAnomaly(_boatNameInput, out var byName))
                    picked = byName;
                else if (_anomalyDatabase.TryGetAnomaly(_selectedBoatFrequency, out var byFreq))
                    picked = byFreq;

                if (picked == null) return;

                // 1) Calcul par bateau
                var perBoat = BuildBoatBreakdownFor(picked, _moneyResultDatabase, _anomalyDatabase.TimeToReportAnomalies);

                // 2) Accumuler + économie
                AccumulateBoatBreakdown(perBoat, applyToEconomyNow: true);

                // 3) UI : (A) bateau courant, (B) global
                GenerateBoatReportElements_ForSingleBoat(popup.BodyParentContent, perBoat);
                //GenerateBoatReportElements_GlobalRecap(popup.BodyParentContent, TodayAllBoatsBreakdown);

                popup.RefreshLayouts();

                // Résolution monde (inchangé)
                var boatInstance = BoatsHandlerData.Boats
                    .Find(x => x.Data.Name == picked.BoatName && x.Data.NationalityFlag == _selectedFlag);
                if (boatInstance != null)
                    boatInstance.AnomalyController.RemoveAnomaly();
                else
                    Debug.LogWarning("Bateau introuvable pour la résolution in-game.");
            };

            popup.StartLoading(() =>
            {
                return EvaluateSubmission(
                    out var _ /*matched*/,
                    out bool nameOK,
                    out bool freqOK,
                    out bool flagOK,
                    out bool typeOK
                );
            });
        }

        /// <summary>
        /// Vérifie la soumission et gère les TryAmount.
        /// Règle de succès : nameOK && freqOK && flagOK && typeOK.
        /// </summary>
        private DataStatus EvaluateSubmission(
            out BoatAnomalyDatas matched,
            out bool nameOK,
            out bool freqOK,
            out bool flagOK,
            out bool typeOK)
        {
            matched = null;
            nameOK = _anomalyDatabase.TryGetAnomaly(_boatNameInput, out var byName);
            freqOK = _anomalyDatabase.TryGetAnomaly(_selectedBoatFrequency, out var byFreq);

            // Déterminer le "candidat" final
            if (nameOK && freqOK)
            {
                // Cas où nom et fréquence pointent des bateaux différents → ambigu
                if (byName.BoatName != byFreq.BoatName)
                {
                    byName.TryAmount++;   // l’utilisateur a donné un nom valide mais mauvaise fréquence
                    byFreq.TryAmount++;   // …et une fréquence valide mais mauvais nom
                    flagOK = false;
                    typeOK = false;
                    return DataStatus.DataMissmatch;
                }
                matched = byName; // (== byFreq)
            }
            else if (nameOK)
            {
                matched = byName;
            }
            else if (freqOK)
            {
                matched = byFreq;
            }
            else
            {
                // Ni nom ni fréquence -> impossible d’identifier le bateau
                flagOK = false;
                typeOK = false;
                return DataStatus.DataMissmatch;
            }

            // À partir d’ici on sait quel bateau est visé (matched)
            typeOK = _anomalyDatabase.HasAnomaly(matched.BoatName, _selectedAnomalyType);
            flagOK = IsFlagCorrectFor(matched.BoatName, _selectedFlag);

            bool allOK = nameOK && freqOK && flagOK && typeOK;

            // Incréments TryAmount quand l’utilisateur visait ce bateau mais s’est trompé sur un point
            if (!allOK)
            {
                // S’il a trouvé par le nom mais pas la fréquence → 1 erreur
                if (nameOK && !freqOK) byName.TryAmount++;

                // S’il a trouvé par la fréquence mais pas le nom → 1 erreur
                if (freqOK && !nameOK) byFreq.TryAmount++;

                // Si nom & fréquence pointent le même bateau mais drapeau/type faux → 1 erreur
                if (nameOK && freqOK && (byName.BoatName == byFreq.BoatName) && (!flagOK || !typeOK))
                    matched.TryAmount++;
            }

            return allOK ? DataStatus.Success : DataStatus.DataMissmatch;
        }

        /// <summary>
        /// Vérifie que le drapeau sélectionné correspond au bateau indiqué.
        /// </summary>
        private bool IsFlagCorrectFor(string boatName, Sprite selectedFlag)
        {
            if (selectedFlag == null) return false;
            if (_nationalityManager.FindName(boatName, out BoatNationalityDatas data))
                return data.NationalityFlag == selectedFlag;
            return false;
        }

        // ----------------------------
        // CALCUL (par bateau)
        // ----------------------------
        private MoneyBoatBreakdown BuildBoatBreakdownFor(
            BoatAnomalyDatas anomaly,
            SO_BoatMoneyResults moneyDb,
            float timeToReportForBonus)
        {
            // Base: +valid
            int baseAmount = BoatMoneyCalculator.ValidFlat(moneyDb);

            // Pénalité essais (négative si >0)
            int penalty = 0;
            if (anomaly.TryAmount > 0)
                penalty = -BoatMoneyCalculator.MissmatchFlat(anomaly.TryAmount, moneyDb);

            // Bonus temps
            int timeBonus = BoatMoneyCalculator.BonusFromTime(anomaly.RemainingTime, timeToReportForBonus, moneyDb);

            var flatLine = new MoneyBoatLine("Boat reported", Color.green, baseAmount);
            var bonusLine = new MoneyBoatLine("Speed report bonus", Color.green, timeBonus);
            var triesLine = new MoneyBoatLine($"Attempt Count: {anomaly.TryAmount}", penalty < 0 ? Color.red : Color.green, penalty);

            return new MoneyBoatBreakdown(anomaly.BoatName, flatLine, bonusLine, triesLine);
        }

        // --------------------------------------------
        // ACCUMULATION (global) + économie immédiate
        // --------------------------------------------
        private void AccumulateBoatBreakdown(MoneyBoatBreakdown boatBk, bool applyToEconomyNow = true)
        {
            TodayAllBoatsBreakdown.Add(boatBk);

            if (applyToEconomyNow)
                PlayerCurrency.Add(boatBk.TotalForThisBoat());
        }

        // ----------------------------
        // UI 
        // ----------------------------
        private void GenerateBoatReportElements_ForSingleBoat(RectTransform parent, MoneyBoatBreakdown b)
        {
            CreateHeader(parent, $"Boat Report — {b.BoatName}");

            // Flat
            CreateMoneyLine(parent, b.FlatAmountWon);

            // Bonus temps
            if (b.BonusFromTime.Value != 0)
                CreateMoneyLine(parent, b.BonusFromTime);

            // Pénalités essais
            if (b.NumberOfTry.Value != 0)
                CreateMoneyLine(parent, b.NumberOfTry);

            int total = b.TotalForThisBoat();
            CreateReportElement(parent, "Total: ",
                $"{(total >= 0 ? "+ " : "- ")}{Mathf.Abs(total)}$",
                total >= 0 ? Color.green : Color.red);
        }

        public MoneyAllBoatsBreakdown GetTodaysResult()
        {
            if (!IsReportedToday)
            {
                var newInstance = new MoneyAllBoatsBreakdown();
                newInstance.EnsureInit();
                return newInstance;
            }
            return TodayAllBoatsBreakdown;
        }

        public void OnNightwatchEndedToday()
        {
            TodayAllBoatsBreakdown.Reset();
        }

        private void CreateMoneyLine(RectTransform parent, MoneyBoatLine line)
        {
            string sign = line.Value >= 0 ? "+ " : "- ";
            int abs = Mathf.Abs(line.Value);
            CreateReportElement(parent, line.Label, $"{sign}{abs}$", line.LabelColor);
        }

        private void CreateHeader(RectTransform parent, string title)
        {
            var header = Instantiate(_reportElementPrefab, parent.transform);
            header.SetDescription(title);
            header.SetMoneyResult("", Color.white);
        }

        private void CreateReportElement(RectTransform parent, string reason, string amount, Color color)
        {
            var element = Instantiate(_reportElementPrefab, parent.transform);
            element.SetDescription(reason);
            element.SetMoneyResult(amount, color);
        }

        private void UpdateSummaryReport()
        {
            _summaryReportText.text = $"{_selectedAnomalyText} on {_boatNameInput}";
        }

        #endregion
    }
}
