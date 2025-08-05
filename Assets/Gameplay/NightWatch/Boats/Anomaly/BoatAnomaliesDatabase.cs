using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BoatAnomalyDatas : ISignal
{
    public string BoatName;
    public AnomalyType AnomalyType;
    public float RemainingTime { get; set; } // Timer restant

    public string Key => BoatName;

    public string DisplayText { get; set; }
}


[CreateAssetMenu(fileName = "AnomalyDatabase", menuName = "LightHouse/Boats/New Anomaly Database")]
public class BoatAnomaliesDatabase : ScriptableObject
{
    public float TimeToReportAnomalies = 300f;  // en secondes

    public List<BoatAnomalyDatas> _anomalies = new List<BoatAnomalyDatas>();

    public event Action<ISignal> OnAnomalyAdded;
    public event Action<ISignal> OnAnomalyRemoved;

    public void SetAnomaly(string boatName, AnomalyType anomalyType, string displayText)
    {
        var existing = _anomalies.Find(a => a.BoatName == boatName);
        if (existing != null)
        {
            existing.AnomalyType = anomalyType;
            existing.RemainingTime = TimeToReportAnomalies;
            existing.DisplayText = displayText;
        }
        else
        {
            var data = new BoatAnomalyDatas
            {
                BoatName = boatName,
                AnomalyType = anomalyType,
                RemainingTime = TimeToReportAnomalies,
                DisplayText = displayText //Radio frequency
            };
            _anomalies.Add(data);
            OnAnomalyAdded?.Invoke(data);
        }
    }

    public void RemoveAnomaly(string boatName)
    {
        var anomaly = _anomalies.Find(a => a.BoatName == boatName);
        if (anomaly != null)
        {
            _anomalies.Remove(anomaly);
            OnAnomalyRemoved?.Invoke(anomaly);
        }
    }

    public IReadOnlyList<BoatAnomalyDatas> GetAnomalies() => _anomalies;

    /// <summary>
    /// Retourne true si, dans la base, le bateau <paramref name="boatName"/> a bien l’anomalie <paramref name="expectedAnomaly"/>.
    /// </summary>
    public bool HasAnomaly(string boatName, AnomalyType expectedAnomaly)
    {
        return _anomalies.Exists(a =>
            a.BoatName == boatName
            && a.AnomalyType == expectedAnomaly
        );
    }

    /// <summary>
    /// Appelée chaque frame par ton controller : décrémente le temps restant,
    /// retire les anomalies expirées et déclenche OnAnomalyRemoved pour chacune.
    /// </summary>
    public void TickTimers(float deltaTime)
    {
        for (int i = 0; i < _anomalies.Count; i++)
            _anomalies[i].RemainingTime -= deltaTime;

        // on supprime en fin de frame pour éviter les problèmes d’itération
        var expired = _anomalies.Where(a => a.RemainingTime <= 0f).ToList();
        /*foreach (var a in expired)
        {
            _anomalies.Remove(a);
            OnAnomalyRemoved?.Invoke(a);
        }*/
    }

    public void ResetAnomalies()
    {
        _anomalies.Clear();
    }
}

