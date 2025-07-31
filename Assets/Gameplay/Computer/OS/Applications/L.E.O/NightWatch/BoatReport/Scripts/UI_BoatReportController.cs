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
    private string _selectedAnomaly;
    private Sprite _selectedFlag;

    public string BoatNameInput => _boatNameInput;
    public string SelectedAnomaly => _selectedAnomaly;

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

    }

    private void OnSendReportCliqued()
    {
        //générer la pop up
        //check if sended datas are goods
        //Update the pop up if it's good or not
        bool isSuccessFull = false;
        if (_nationalityManager.FindName(BoatNameInput, out BoatData data))
        {
            Debug.Log($"Bateau correspondant: {data.Name}");

            bool flagCorrect = data.NationalityFlag == _selectedFlag;
            bool anomalyCorrect = _anomalyDatabase.HasAnomaly(data.Name, _selectedAnomaly);

            if (flagCorrect && anomalyCorrect)
            {
                Debug.Log("✅ Report VALID: Name + flag + anomalie OK");
                isSuccessFull = true;
                //Get difference here to know in how many time he resolved the problem
                //How can we know it's the good solved
            }
            else
            {
                Debug.Log("❌ Report invalide.");
                if (!flagCorrect) Debug.Log("→ Mauvais drapeau");
                if (!anomalyCorrect) Debug.Log("→ Mauvaise anomalie");
            }
        }
        var instance = Instantiate(_nightWatchSendDatasPrefab, _nightWatch.transform as RectTransform);
        var rectTransform = instance.transform as RectTransform;;
        rectTransform.anchoredPosition = Vector3.zero;
        instance.IsSuccessfull = isSuccessFull;
        if (isSuccessFull)
        {
            var boatInstance = BoatsHandlerData.Boats.Find(x => x.Data.Name == BoatNameInput && x.Data.NationalityFlag == _selectedFlag);
            Debug.Log(boatInstance);
            if (boatInstance != null) Debug.Log("L'instance du bateau a été trouvée, l'anomalie va être résolue");
            boatInstance.AnomalyController.RemoveAnomaly();
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
        TMP_Dropdown.OptionData noneData = new TMP_Dropdown.OptionData();
        noneData.text = "None";
        datas.Add(noneData);
        foreach (var config in _nationalityManager.PossibleConfigs)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = config.NationalityName;
            optionData.image = config.Flag;
            datas.Add(optionData);
        }
        _summaryFlag.gameObject.SetActive(false);
        _dropdownFlag.gameObject.SetActive(false);

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
        }
        else
        {
            _selectedFlag = _nationalityManager.PossibleConfigs[arg0 - 1].Flag;
        }
        if (arg0 > 0)
        {
            _dropdownFlag.gameObject.SetActive(true);
            _summaryFlag.gameObject.SetActive(true);
            _dropdownFlag.sprite = _selectedFlag;
            _summaryFlag.sprite = _selectedFlag;
        }
        else if(arg0 == 0)
        {
            _summaryFlag.gameObject.SetActive(false);
            _dropdownFlag.gameObject.SetActive(false);
        }
        
    }

    private void UpdateSummaryReport()
    {
        _summaryReportText.text = _selectedAnomaly + " on " + _boatNameInput;
    }
}
