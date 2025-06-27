using LightHouse.Game.WaterExtension;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BoatsNationalitiesManager _boatsManager;
    [SerializeField] private BoatData _data;

    [SerializeField] private VectorPathDatabase _randomPointController;
    [SerializeField] private BoidController _controller;
    [SerializeField] private BoatAnomalyController _anomalyController;
    public BoatAnomalyController AnomalyController => _anomalyController;

    private void Awake()
    {
        _data = _boatsManager.Register();
        _controller.Initialize(_randomPointController.GetRandomPath());
    }

    private void OnDestroy()
    {
        _boatsManager.Unregister(_data);
    }
}
