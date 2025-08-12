using LightHouse.Game.Boats;
using System.Collections.Generic;
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

        [Header("UI Elements - Anomaly Selection")]
        [SerializeField] private AnomalyReportButton[] _anomalyButtons;
        [SerializeField] private UI_BoatsLowestTimerAlert _anomalyTimerController;

        [Header("UI Elements - Flags")]
        [SerializeField] private TMP_Dropdown _nationalitiesDropdown;
        [SerializeField] private Image _dropdownFlag;
        [SerializeField] private Image _summaryFlag;

        [Header("UI Elements - Boat Name")]
        [SerializeField] private TMP_InputField _boatNameInputField;

        [Header("UI Elements - Summary")]
        [SerializeField] private TextMeshProUGUI _summaryReportText;

        [Header("UI Elements - Actions")]
        [SerializeField] private Button _sendReportButton;
        [SerializeField] private Button _resetAllButton;

        [Header("UI Prefabs")]
        [SerializeField] private UI_NightWatchPopup_ReportDatas _sendDatasPrefab;

        #endregion

        #region Private Fields

        private string _boatNameInput;
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
                });
            }
        }

        #endregion

        #region Event Registration

        private void RegisterUIEvents()
        {
            _boatNameInputField.onValueChanged.AddListener(OnBoatNameChanged);
            _nationalitiesDropdown.onValueChanged.AddListener(OnFlagDropdownChanged);

            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            _resetAllButton.onClick.AddListener(OnResetAllClicked);
        }

        private void UnregisterUIEvents()
        {
            _boatNameInputField.onValueChanged.RemoveListener(OnBoatNameChanged);
            _nationalitiesDropdown.onValueChanged.RemoveListener(OnFlagDropdownChanged);

            _sendReportButton.onClick.RemoveListener(OnSendReportClicked);
            _resetAllButton.onClick.RemoveListener(OnResetAllClicked);
        }

        #endregion

        #region UI Event Handlers

        private void OnBoatNameChanged(string name)
        {
            _boatNameInput = name;
            UpdateSummaryReport();
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
        }

        private void OnResetAllClicked()
        {
            _nationalitiesDropdown.value = 0;
            _boatNameInputField.text = string.Empty;
            _selectedFlag = null;
            _selectedAnomalyText = string.Empty;

            _summaryFlag.gameObject.SetActive(false);
            _dropdownFlag.gameObject.SetActive(false);

            UpdateSummaryReport();
        }

        private void OnSendReportClicked()
        {
            var boatName = BoatNameInput;
            var selectedFlag = _selectedFlag;
            var selectedAnomaly = _selectedAnomalyType;

            var popup = Instantiate(_sendDatasPrefab, _nightWatch.transform as RectTransform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;

            popup.OnLoadingCompleted += status =>
            {
                if (status == DataStatus.Success)
                {
                    _anomalyDatabase.RemoveAnomaly(boatName);

                    var boatInstance = BoatsHandlerData.Boats
                        .Find(x => x.Data.Name == boatName && x.Data.NationalityFlag == selectedFlag);

                    if (boatInstance != null)
                        boatInstance.AnomalyController.RemoveAnomaly();
                    else
                        Debug.LogWarning("Bateau introuvable pour la résolution in-game.");
                }
            };

            popup.StartLoading(() =>
            {
                if (!_nationalityManager.FindName(boatName, out BoatData data))
                    return DataStatus.DataMissmatch;

                bool flagCorrect = data.NationalityFlag == selectedFlag;
                bool anomalyCorrect = _anomalyDatabase.HasAnomaly(data.Name, selectedAnomaly);

                return (flagCorrect && anomalyCorrect) ? DataStatus.Success : DataStatus.DataMissmatch;
            });
        }

        #endregion

        #region Private Helpers

        private void UpdateSummaryReport()
        {
            _summaryReportText.text = $"{_selectedAnomalyText} on {_boatNameInput}";
        }

        #endregion
    }
}
