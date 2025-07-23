using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BoatReportController : NightWatchReportWindow
{
    [SerializeField] private BoatsNationalitiesManager _nationalityManager;
    [SerializeField] private AnomalyReportButton[] _anomaliesButton;
    [SerializeField] TMP_Dropdown _nationalitiesDropdown;
    [SerializeField] Image _dropdownFlag;
    [SerializeField] private Image _summaryFlag;
    [SerializeField] private TMP_InputField IPF_enterBoatName;
    [SerializeField] private TextMeshProUGUI _summaryReportText;

    [SerializeField] private Button B_sendReport;

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
        SetupAnomalyDropdown();
        SetupAnomalyButtons();

        B_sendReport.onClick.AddListener(OnSendReportCliqued);
    }

    private void OnDestroy()
    {
        B_sendReport.onClick.RemoveListener(OnSendReportCliqued);
    }

    private void OnSendReportCliqued()
    {
        //générer la pop up
        //check if sended datas are goods
        //Update the pop up if it's good or not
        if (_nationalityManager.FindName(BoatNameInput, out BoatData data))
        {
            Debug.Log($"Bateau correspondant: {data.Name}");
            if (data.NationalityFlag == _selectedFlag)
            {
                Debug.Log($"Flag correspondant: {data.NationalityFlag.name}");

                
            }
        }
    }
    private void SetupAnomalyButtons()
    {
        foreach(var anomalyButton in _anomaliesButton)
        {
            anomalyButton.AnomalyButton.onClick.AddListener(() =>
            {
                _boatNameInput = anomalyButton.AnomalyText.text;
                UpdateSummaryReport();
            });
        }
    }
    
    private void SetupAnomalyDropdown()
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
        //_nationalitiesDropdown.value = 0;
    }

    private void OnEnterBoatNameChanged(string arg0)
    {
        _selectedAnomaly = arg0;
        UpdateSummaryReport();
    }

    private void OnFlagDropdownChanged(int arg0)
    {
        var selectedFlag = _nationalityManager.PossibleConfigs[arg0].Flag;
        if(arg0 > 0 && _summaryFlag.color.a == 0)
        {
            _summaryFlag.color = new Color(1, 1, 1, 1);
        }
        _dropdownFlag.sprite = selectedFlag;
        _summaryFlag.sprite = selectedFlag;
    }

    private void UpdateSummaryReport()
    {
        _summaryReportText.text = _boatNameInput + " on " + _selectedAnomaly;
    }
}
