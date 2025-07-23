using UnityEngine;
using System.Collections.Generic;

public class BoatAnomalyController : MonoBehaviour
{
    private BoatAnomaly _currentAnomaly = null;
    [SerializeField] private float _minAnomalyProgress = 0.1f;
    [SerializeField] private float _maxAnomalyProgress = 0.5f;

    public void AddAnomaly(BoatAnomaly anomalyInstance)
    {
        _currentAnomaly = anomalyInstance;
    }

    public void RemoveAnomaly(BoatAnomaly anomaly)
    {
        _currentAnomaly = null;
        Destroy(anomaly);
    }

    public BoatAnomaly GetActiveAnomaly() => _currentAnomaly;
}
