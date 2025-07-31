using UnityEngine;
using System.Collections.Generic;
using System;

public class BoatAnomalyController : MonoBehaviour
{
    private BoatAnomaly _currentAnomaly = null;
    [SerializeField] private float _minAnomalyProgress = 0.1f;
    [SerializeField] private float _maxAnomalyProgress = 0.5f;
    public event Action OnAnomalyAdded;
    public event Action OnAnomalyResolved;

    public void AddAnomaly(BoatAnomaly anomalyInstance)
    {
        _currentAnomaly = anomalyInstance;
        OnAnomalyAdded?.Invoke();
    }

    public void RemoveAnomaly()
    {
        Destroy(_currentAnomaly.gameObject);
        _currentAnomaly = null;
        OnAnomalyResolved?.Invoke();
    }

    public BoatAnomaly GetActiveAnomaly() => _currentAnomaly;
}
