using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadarContent : ContentWindow
{
    [SerializeField] private CustomEvent _eventOnButtonReportBoat;
    [SerializeField] private CustomEvent_String _eventReportBoat;
    [SerializeField] private CustomEvent_String _eventAddBuoy;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Transform _parentBuoyReporterUI;
    [SerializeField] private BuoyReporterUI _prefabBuoyReporterUI;
    [SerializeField] private List<BuoyReporterUI> _buoyReporters;
    [SerializeField] private ToggleBoatReportType[] _toggleReportType;
    private int _buoyCount => _buoyReporters.Count;

    private BoatReportType _boatReportType;

    private void Awake()
    {
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

    private void OnReportBuoy(int id)
    {
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
        _buoyReporters.Add(buoyUI);
    }
}