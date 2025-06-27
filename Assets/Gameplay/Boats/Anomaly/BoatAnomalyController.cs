using UnityEngine;
using System.Collections.Generic;

public class BoatAnomalyController : MonoBehaviour
{
    private List<BoatAnomaly> activeAnomalies = new();
    [SerializeField] private float _minAnomalyProgress = 0.1f;
    [SerializeField] private float _maxAnomalyProgress = 0.5f;

    public void AddAnomaly(BoatAnomaly anomalyPrefab)
    {
        activeAnomalies.Add(anomalyPrefab);
    }

    public void RemoveAnomaly(BoatAnomaly anomaly)
    {
        activeAnomalies.Remove(anomaly);
        Destroy(anomaly);
    }

    public IReadOnlyList<BoatAnomaly> GetActiveAnomalies() => activeAnomalies;
}
