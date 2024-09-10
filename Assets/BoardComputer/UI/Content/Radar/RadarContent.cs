using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarContent : ContentWindow
{
    [SerializeField] private BuoyReporterUI[] _buoyReporters;
    private int _buoyCount => _buoyReporters.Length;

    private void Start()
    {
        InitializeAllBuoy();
    }

    private void InitializeAllBuoy()
    {
        int id = 0;
        foreach ( var buoy in _buoyReporters)
        {
            id++;
            buoy.Intialize(id);
            buoy._reportEvent += OnReport;
        }
    }

    private void OnDestroy()
    {
        foreach (var buoy in _buoyReporters)
        {
            buoy._reportEvent -= OnReport;
        }
    }

    private void OnReport(int id)
    {
        Debug.Log($"Buoy {id} is not working, send somebody to repair it.");
    }
}