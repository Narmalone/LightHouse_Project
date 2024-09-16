using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadarContent : ContentWindow
{
    [SerializeField] private CustomEvent _eventOnButtonReportBoat;
    [SerializeField] private CustomEvent_String _eventReportBoat;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private BuoyReporterUI[] _buoyReporters;
    [SerializeField] private ToggleBoatReportType[] _toggleReportType;
    private int _buoyCount => _buoyReporters.Length;

    private BoatReportType _boatReportType;

    private void Awake()
    {
        _eventOnButtonReportBoat.handle += OnBoatReport;
    }

    private void Start()
    {
        InitializeAllBuoy();
        InitializeAllBoat();
        OnBoatReportTypeChange(BoatReportType.DANGEREUX);
    }

    private void OnDestroy()
    {
        _eventOnButtonReportBoat.handle -= OnBoatReport;

        foreach (var buoy in _buoyReporters)
        {
            buoy._reportEvent -= OnReportBuoy;
        }
    }
    private void InitializeAllBuoy()
    {
        int id = 0;
        foreach ( var buoy in _buoyReporters)
        {
            id++;
            buoy.Intialize(id);
            buoy._reportEvent += OnReportBuoy;
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
}