using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BuyoncyAnomalyDatas : ISignal
{
    public int ID;
    public float RemainingTime { get; set; }

    public string Key => ID.ToString("00");

    public string DisplayText { get; set; }
}

[CreateAssetMenu(fileName = "BuyoncyAnomalyDatabase", menuName = "LightHouse/Buyoncies/New Database")]
public class BuyoncyAnomalyDatabase : ScriptableObject
{
    public float TimeToReportAnomalies = 300f;  // en secondes
    public List<BuyoncyAnomalyDatas> _anomalies = new List<BuyoncyAnomalyDatas>();

    public event Action<ISignal> OnAnomalyAdded;
    public event Action<ISignal> OnAnomalyRemoved;

    public void SetAnomaly(int id)
    {
        var existing = _anomalies.Find(a => a.ID == id);
        if (existing != null)
        {
            existing.RemainingTime = TimeToReportAnomalies;
        }
        else
        {
            var data = new BuyoncyAnomalyDatas
            {
                ID = id,
                RemainingTime = TimeToReportAnomalies,
                DisplayText = "Anomaly Detected"
            };
            _anomalies.Add(data);
            OnAnomalyAdded?.Invoke(data);
        }
    }

    public void RemoveAnomaly(int id)
    {
        var anomaly = _anomalies.Find(a => a.ID == id);
        if (anomaly != null)
        {
            _anomalies.Remove(anomaly);
            OnAnomalyRemoved?.Invoke(anomaly);
        }
    }

    public void RemoveAnomalies(List<int> ids)
    {
        // On collecte toutes les anomalies à supprimer (évite les problèmes de foreach sur liste modifiée)
        var toRemove = _anomalies.Where(a => ids.Contains(a.ID)).ToList();

        foreach (var anomaly in toRemove)
        {
            _anomalies.Remove(anomaly);
            OnAnomalyRemoved?.Invoke(anomaly);
        }
    }


    public IReadOnlyList<BuyoncyAnomalyDatas> GetAnomalies() => _anomalies;

    /// <summary>
    /// Retourne true si, dans la base, le bateau <paramref name="boatName"/> a bien l’anomalie <paramref name="expectedAnomaly"/>.
    /// </summary>
    public bool HasAnomaly(int id)
    {
        return _anomalies.Exists(a => a.ID == id);
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
       /* foreach (var a in expired)
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
