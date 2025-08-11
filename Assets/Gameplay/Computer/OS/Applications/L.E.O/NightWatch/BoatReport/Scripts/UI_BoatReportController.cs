using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Boats
{
    public class UI_BoatReportController : NightWatchReportWindow
    {
        [SerializeField] private BoatsNationalitiesManager _nationalityManager;
        [SerializeField] private BoatAnomaliesDatabase _anomalyDatabase;
        [SerializeField] private AnomalyReportButton[] _anomaliesButton;
        [SerializeField] private NightWatchSendDatas _nightWatchSendDatasPrefab;
        [SerializeField] private BoatAnomalyTimerController _anomalyTimerController;
        [SerializeField] TMP_Dropdown _nationalitiesDropdown;
        [SerializeField] Image _dropdownFlag;
        [SerializeField] private Image _summaryFlag;
        [SerializeField] private TMP_InputField IPF_enterBoatName;
        [SerializeField] private TextMeshProUGUI _summaryReportText;

        [SerializeField] private Button B_sendReport;
        [SerializeField] private Button B_resetAll;

        private string _boatNameInput;
        private AnomalyType _selectedAnomalyType;
        private string _selectedAnomalyText;
        private Sprite _selectedFlag;

        public string BoatNameInput => _boatNameInput;
        public AnomalyType SelectedAnomaly => _selectedAnomalyType;

        private void Awake()
        {
            IPF_enterBoatName.onValueChanged.AddListener(OnEnterBoatNameChanged);
            _nationalitiesDropdown.onValueChanged.AddListener(OnFlagDropdownChanged);

            SetupFlagsDropdown();
            SetupAnomalyButtons();

            B_sendReport.onClick.AddListener(OnSendReportCliqued);
            B_resetAll.onClick.AddListener(OnResetAllCliqued);
        }

        private void OnDestroy()
        {
            B_sendReport.onClick.RemoveListener(OnSendReportCliqued);
            B_resetAll.onClick.RemoveListener(OnResetAllCliqued);
        }

        private void OnValidate()
        {
            _anomaliesButton = GetComponentsInChildren<AnomalyReportButton>();
        }

        private void OnResetAllCliqued()
        {
            _nationalitiesDropdown.value = -1;
            IPF_enterBoatName.text = string.Empty;
            _selectedFlag = null;
            _selectedAnomalyText = string.Empty;
            _summaryFlag.gameObject.SetActive(false);
            _dropdownFlag.gameObject.SetActive(false);
            UpdateSummaryReport();
        }

        private void OnSendReportCliqued()
        {
            // On capture ce qu’il faut dans des variables locales pour la fermeture (lambda)
            string boatName = BoatNameInput;
            Sprite selectedFlag = _selectedFlag;
            AnomalyType selectedAnomaly = _selectedAnomalyType;

            // On instancie la fenêtre
            var popup = Instantiate(_nightWatchSendDatasPrefab, _nightWatch.transform as RectTransform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;

            // Abonnement au résultat final
            popup.OnLoadingCompleted += status =>
            {
                if (status == DataStatus.Success)
                {
                    // Résolution côté monde de jeu (suppression anomalie, etc.)
                    _anomalyDatabase.RemoveAnomaly(boatName);

                    var boatInstance = BoatsHandlerData.Boats
                        .Find(x => x.Data.Name == boatName && x.Data.NationalityFlag == selectedFlag);

                    if (boatInstance != null)
                    {
                        Debug.Log("✅ Bateau trouvé, on résout l'anomalie in-game.");
                        boatInstance.AnomalyController.RemoveAnomaly();
                    }
                    else
                    {
                        Debug.LogWarning("Bateau introuvable pour la résolution in-game.");
                    }
                }
            };

            // Démarre l’animation de chargement en déléguant la décision du statut
            popup.StartLoading(() =>
            {
                // Source de vérité UNIQUE : on calcule ici le DataStatus
                if (!_nationalityManager.FindName(boatName, out BoatData data))
                {
                    Debug.Log("❌ Aucun bateau trouvé pour ce nom.");
                    return DataStatus.Failed; // ex: nom incorrect
                }

                bool flagCorrect = data.NationalityFlag == selectedFlag;
                bool anomalyCorrect = _anomalyDatabase.HasAnomaly(data.Name, selectedAnomaly);

                if (flagCorrect && anomalyCorrect)
                {
                    Debug.Log("✅ Report VALID: Name + flag + anomalie OK");
                    return DataStatus.Success;
                }
                else
                {
                    if (!flagCorrect) Debug.Log("→ Mauvais drapeau");
                    if (!anomalyCorrect) Debug.Log("→ Mauvaise anomalie");
                    return DataStatus.DataMissmatch; // données incohérentes
                }
            });
        }

        private void SetupAnomalyButtons()
        {
            foreach (var anomalyButton in _anomaliesButton)
            {
                var btn = anomalyButton; // capture locale propre
                btn.AnomalyButton.onClick.AddListener(() =>
                {
                    _selectedAnomalyText = btn.AnomalyText.text;
                    _selectedAnomalyType = btn.AnomalyDefinition.Type;
                    UpdateSummaryReport();
                });
            }
        }

        private void SetupFlagsDropdown()
        {
            _dropdownFlag.sprite = null;
            List<TMP_Dropdown.OptionData> datas = new List<TMP_Dropdown.OptionData>();
            TMP_Dropdown.OptionData noneData = new TMP_Dropdown.OptionData { text = "None" };
            datas.Add(noneData);

            foreach (var config in _nationalityManager.PossibleConfigs)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData
                {
                    text = config.NationalityName,
                    image = config.Flag
                };
                datas.Add(optionData);
            }

            _summaryFlag.gameObject.SetActive(false);
            _dropdownFlag.gameObject.SetActive(false);

            _nationalitiesDropdown.ClearOptions();
            _nationalitiesDropdown.AddOptions(datas);
            _nationalitiesDropdown.value = 0;
        }

        private void OnEnterBoatNameChanged(string arg0)
        {
            _boatNameInput = arg0;
            UpdateSummaryReport();
        }

        private void OnFlagDropdownChanged(int arg0)
        {
            if (arg0 <= 0)
            {
                _selectedFlag = null;
                _summaryFlag.gameObject.SetActive(false);
                _dropdownFlag.gameObject.SetActive(false);
            }
            else
            {
                _selectedFlag = _nationalityManager.PossibleConfigs[arg0 - 1].Flag;
                _dropdownFlag.gameObject.SetActive(true);
                _summaryFlag.gameObject.SetActive(true);
                _dropdownFlag.sprite = _selectedFlag;
                _summaryFlag.sprite = _selectedFlag;
            }

            UpdateSummaryReport();
        }

        private void UpdateSummaryReport()
        {
            _summaryReportText.text = $"{_selectedAnomalyText} on {_boatNameInput}";
        }
    }

}
