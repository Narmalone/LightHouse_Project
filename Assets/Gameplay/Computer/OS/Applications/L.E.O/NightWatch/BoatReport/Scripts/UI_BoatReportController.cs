using LightHouse.Game.Boats;
using LightHouse.Game.Boats.Frequencies;
using LightHouse.Game.Computer.LEO.NightWatch.Buoys;
using LightHouse.Money;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Boats
{
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
        #endregion

        #region Private Fields

        private string _boatNameInput;
        private float _selectedBoatFrequency;
        private AnomalyType _selectedAnomalyType;
        private string _selectedAnomalyText;
        private Sprite _selectedFlag;
        #endregion

        #region Public Properties

        public string BoatNameInput => _boatNameInput;
        public AnomalyType SelectedAnomaly => _selectedAnomalyType;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            RegisterUIEvents();
            InitializeDropdownOptions();
            InitializeAnomalyButtons();
            _sendReportButton.interactable = false;
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

            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            _resetAllButton.onClick.AddListener(OnResetAllClicked);
        }

        private void UnregisterUIEvents()
        {
            _boatNameInputField.onValueChanged.RemoveListener(OnBoatNameChanged);
            _nationalitiesDropdown.onValueChanged.RemoveListener(OnFlagDropdownChanged);
            _boatFrequencyInputField.onValueChanged.RemoveListener(OnBoatFrequencyChanged);

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
            _boatNameInput = name;
            UpdateSummaryReport();
            OnBoatsUIEventsChanged();
        }

        private void OnBoatFrequencyChanged(string arg0)
        {
            if(FloatExtension.TryParse(arg0, out float result))
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
            _boatNameInputField.text = string.Empty;
            _boatFrequencyInputField.text = string.Empty;
            
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

        private void EvaluateAndShowResults()
        {
            var popup = Instantiate(_sendDatasPrefab, _nightWatch.transform as RectTransform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;

            // Quand l'UI a fini son "loading"
            popup.OnLoadingCompleted += status =>
            {
                if (status != DataStatus.Success) return;

                // On a réussi → on récupère les datas du bateau pour afficher le récap + donner l’argent
                if (_anomalyDatabase.TryGetAnomaly(_boatNameInput, out var dataByName))
                {
                    GenerateReportElements(popup, popup.BodyParentContent, status, dataByName);
                    popup.RefreshLayouts();

                    // Résolution côté monde
                    var boatInstance = BoatsHandlerData.Boats
                        .Find(x => x.Data.Name == dataByName.BoatName && x.Data.NationalityFlag == _selectedFlag);
                    if (boatInstance != null)
                        boatInstance.AnomalyController.RemoveAnomaly();
                    else
                        Debug.LogWarning("Bateau introuvable pour la résolution in-game.");
                }
                else
                {
                    // fallback : si on a matché par fréquence uniquement
                    if (_anomalyDatabase.TryGetAnomaly(_selectedBoatFrequency, out var dataByFreq))
                    {
                        GenerateReportElements(popup, popup.BodyParentContent, status, dataByFreq);
                        popup.RefreshLayouts();

                        var boatInstance = BoatsHandlerData.Boats
                            .Find(x => x.Data.Name == dataByFreq.BoatName && x.Data.NationalityFlag == _selectedFlag);
                        if (boatInstance != null)
                            boatInstance.AnomalyController.RemoveAnomaly();
                    }
                }
            };

            // Toute la logique d'évaluation se fait ici (et retourne Success ou Mismatch)
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

            Debug.Log(nameOK);
            Debug.Log(freqOK);

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
                // Ni nom ni fréquence → impossible d’identifier le bateau
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
            if (_nationalityManager.FindName(boatName, out BoatData data))
                return data.NationalityFlag == selectedFlag;
            return false;
        }


        private void GenerateReportElements(UI_ReportDatasPopup datas, RectTransform parent, DataStatus status, BoatAnomalyDatas anomalyDatas)
        {
            int total = 0;

            //Valid
            int validValue = BoatMoneyCalculator.ValidFlat(_moneyResultDatabase);
            CreateReportElement(parent, "Boat reported", $"+ {validValue}$", Color.green);
            total += validValue;

            if(anomalyDatas.TryAmount > 0)
            {
                int penalty = BoatMoneyCalculator.MissmatchFlat(anomalyDatas.TryAmount, _moneyResultDatabase);
                CreateReportElement(parent, $"Attempt Count: {anomalyDatas.TryAmount}", $" -{penalty}$", Color.red);
                total -= penalty;
            }

            int bonusFromTime = BoatMoneyCalculator.BonusFromTime(anomalyDatas.RemainingTime, _anomalyDatabase.TimeToReportAnomalies, _moneyResultDatabase);
            total += bonusFromTime;

            PlayerCurrency.Add(total);

            CreateReportElement(parent, "Speed report bonus", $"+ {bonusFromTime}$", Color.green);

            CreateReportElement(parent, "Total: ", $"{(total > 0 ? "+ " : "- ")}{total}", total > 0 ? Color.green : Color.red);
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
