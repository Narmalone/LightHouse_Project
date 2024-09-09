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
        }
    }
}