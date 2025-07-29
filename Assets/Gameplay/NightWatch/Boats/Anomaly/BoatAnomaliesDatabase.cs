using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnomalyDatabase", menuName = "LightHouse/Boats/New Anomaly Database")]
public class BoatAnomaliesDatabase : ScriptableObject
{
    public float TimeToReportAnomalies = 5.0f;
    // Clé = Nom du bateau ; Valeur = anomalie active (ou null)
    private Dictionary<string, string> _anomalies = new Dictionary<string, string>();
    public event Action OnAnomalyAdded;

    public void SetAnomaly(string boatName, string anomaly)
    {
        _anomalies[boatName] = anomaly;
        OnAnomalyAdded?.Invoke();
    }

    public bool TryGetAnomaly(string boatName, out string anomaly)
    {
        return _anomalies.TryGetValue(boatName, out anomaly);
    }

    public bool HasAnomaly(string boatName, string expectedAnomaly)
    {
        return _anomalies.TryGetValue(boatName, out var actual) &&
               string.Equals(actual, expectedAnomaly, System.StringComparison.OrdinalIgnoreCase);
    }

    public void ResetAll()
    {
        _anomalies.Clear();
    }
}
