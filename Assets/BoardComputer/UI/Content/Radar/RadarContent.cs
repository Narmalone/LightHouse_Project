using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadarContent : ContentWindow
{
    [SerializeField] private CustomEvent _eventOnButtonReportBoat;
    [SerializeField] private CustomEvent_String _eventReportBoat;
    [SerializeField] private CustomEvent_String _eventReportBuoy;
    [SerializeField] private CustomEvent_String _eventRepairBuoy;
    [SerializeField] private CustomEvent_String _eventAddBuoy;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private RectTransform _parentBuoyReporterUI;
    [SerializeField] private BuoyReporterUI _prefabBuoyReporterUI;
    [SerializeField] private float _marginContentUI = 0.002f;
    [SerializeField] private List<BuoyReporterUI> _buoyReporters;
    [SerializeField] private ToggleBoatReportType[] _toggleReportType;
    private int _buoyCount => _buoyReporters.Count;

    private BoatReportType _boatReportType;

    private Vector2 _sizeDeltaContentBuoyUI;

    private void Awake()
    {
        _sizeDeltaContentBuoyUI.x = _parentBuoyReporterUI.sizeDelta.x;

        _eventRepairBuoy.handle += OnRepairBuoy;
        _eventAddBuoy.handle += OnBuoyAdded;
        _eventOnButtonReportBoat.handle += OnBoatReport;
    }

    private void Start()
    {
        InitializeAllBoat();
        OnBoatReportTypeChange(BoatReportType.DANGEREUX);
    }

    private void OnDestroy()
    {
        _eventRepairBuoy.handle -= OnRepairBuoy;
        _eventAddBuoy.handle -= OnBuoyAdded;
        _eventOnButtonReportBoat.handle -= OnBoatReport;

        foreach (var buoy in _buoyReporters)
        {
            buoy._reportEvent -= OnReportBuoy;
        }
    }
    
    private void InitializeAllBoat()
    {
        foreach ( var boat in _toggleReportType)
        {
            boat._updateType += OnBoatReportTypeChange;
        }
    }

    private void OnReportBuoy(string id)
    {
        _eventReportBuoy.Raise(id);
        Debug.Log($"Buoy {id} is not working, send somebody to repair it.");
    }

    private void OnBoatReportTypeChange(BoatReportType type)
    {
        _boatReportType = type;
    }

    private void OnBoatReport()
    {
        var id = _inputField.textComponent.text;
        _eventReportBoat.Raise(id.Remove(id.Length-1));
        _inputField.text = string.Empty;
    }

    private void OnBuoyAdded(string id)
    {
        var buoyUI = Instantiate(_prefabBuoyReporterUI, _parentBuoyReporterUI);
        buoyUI.Intialize(id);
        buoyUI._reportEvent += OnReportBuoy;
        _buoyReporters.Add(buoyUI);
        UpdateContentReporterHeight();
    }

    private void UpdateContentReporterHeight()
    {
        _sizeDeltaContentBuoyUI.y = (_prefabBuoyReporterUI.GetComponent<RectTransform>().rect.height + _marginContentUI )* _buoyReporters.Count;
        _parentBuoyReporterUI.sizeDelta = _sizeDeltaContentBuoyUI;
    }

    private void OnRepairBuoy(string id)
    {
        Debug.Log(id);
        var buoy = _buoyReporters.Find(x => x._id.Equals(id));
        Debug.Log(buoy._id);
        Debug.Log(buoy, buoy);
        buoy.Idle();
    }
}