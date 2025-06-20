using LightHouse.Game.WaterExtension;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BoatsNationalitiesManager _boatsManager;

    [SerializeField] private BoatData _data;


    [SerializeField] private RandomPointOnWaterSurface _randomPointController;
    [SerializeField] private BoatAnomalyController _anomalyController;
    public BoatAnomalyController AnomalyController => _anomalyController;



    private void Awake()
    {
        _data = _boatsManager.Register();
    }

    private void Update()
    {
        float progress = _randomPointController.GetProgress01(_rb.transform.position);
        Debug.Log(progress);
    }

    private void OnDestroy()
    {
        _boatsManager.Unregister(_data);
    }
}
