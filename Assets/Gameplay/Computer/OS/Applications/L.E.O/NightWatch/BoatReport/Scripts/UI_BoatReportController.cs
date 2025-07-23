using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BoatReportController : NightWatchReportWindow
{
    [SerializeField] private BoatsNationalitiesManager _nationalityManager;
    [SerializeField] private BoatAnomaliesDatabase _anomalyDatabase;
    [SerializeField] private AnomalyReportButton[] _anomaliesButton;
    [SerializeField] TMP_Dropdown _nationalitiesDropdown;
    [SerializeField] Image _dropdownFlag;
    [SerializeField] private Image _summaryFlag;
    [SerializeField] private TMP_InputField IPF_enterBoatName;
    [SerializeField] private TextMeshProUGUI _summaryReportText;

    [SerializeField] private Button B_sendReport;
    [SerializeField] private Button B_resetAll;

    private string _boatNameInput;
    private string _selectedAnomaly;
    private Sprite _selectedFlag;

    public string BoatNameInput => _boatNameInput;
    public string SelectedAnomaly => _selectedAnomaly;

    private void Awake()
    {
        IPF_enterBoatName.onValueChanged.AddListener(OnEnterBoatNameChanged);
        _nationalitiesDropdown.onValueChanged.AddListener(OnFlagDropdownChanged);
        _summaryFlag.sprite = null;
        _summaryFlag.color = new Color(1, 1, 1, 0);
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

    }

    private void OnSendReportCliqued()
    {
        //générer la pop up
        //check if sended datas are goods
        //Update the pop up if it's good or not
        if (_nationalityManager.FindName(BoatNameInput, out BoatData data))
        {
            Debug.Log($"Bateau correspondant: {data.Name}");

            bool flagCorrect = data.NationalityFlag == _selectedFlag;
            bool anomalyCorrect = _anomalyDatabase.HasAnomaly(data.Name, _selectedAnomaly);

            if (flagCorrect && anomalyCorrect)
            {
                Debug.Log("✅ Report VALID: Name + flag + anomalie OK");
            }
            else
            {
                Debug.Log("❌ Report invalide.");
                if (!flagCorrect) Debug.Log("→ Mauvais drapeau");
                if (!anomalyCorrect) Debug.Log("→ Mauvaise anomalie");
            }
        }
    }
    private void SetupAnomalyButtons()
    {
        foreach(var anomalyButton in _anomaliesButton)
        {
            anomalyButton.AnomalyButton.onClick.AddListener(() =>
            {
                _selectedAnomaly = anomalyButton.AnomalyText.text;
                UpdateSummaryReport();
            });
        }
    }
    
    private void SetupFlagsDropdown()
    {
        _dropdownFlag.sprite = null;
        List<TMP_Dropdown.OptionData> datas = new List<TMP_Dropdown.OptionData>();
        foreach (var config in _nationalityManager.PossibleConfigs)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = config.NationalityName;
            optionData.image = config.Flag;
            datas.Add(optionData);
        }
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
        _selectedFlag = _nationalityManager.PossibleConfigs[arg0].Flag;
        if(arg0 > 0 && _summaryFlag.color.a == 0)
        {
            _summaryFlag.color = new Color(1, 1, 1, 1);
        }
        _dropdownFlag.sprite = _selectedFlag;
        _summaryFlag.sprite = _selectedFlag;
    }

    private void UpdateSummaryReport()
    {
        _summaryReportText.text = _selectedAnomaly + " on " + _boatNameInput;
    }
}
