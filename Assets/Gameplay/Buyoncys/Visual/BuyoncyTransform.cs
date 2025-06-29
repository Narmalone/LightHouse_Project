using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuyoncyTransform : MonoBehaviour
{
    [Header(" --- REFERENCES --- ")]
    [SerializeField] private WaterSurface _water;

    [Header(" --- PRÉCISION --- ")]
    [SerializeField, Range(1, 2000)] private int maxIterations = 20;
    [SerializeField] private float smoothFactor = 10f;

    private WaterSimSearchData _simData;

    private void Start()
    {
        if (_water == null)
            _water = GameWorldHandlerData.MainOceanSurface;

        _simData = new WaterSimSearchData
        {
        };
    }

    private void FixedUpdate()
    {
        if (_water == null) return;
        _water.FillWaterSearchData(ref _simData);
/*
        // Snap à la hauteur simulée
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, _simData.projectedPositionWS.y, Time.fixedDeltaTime * smoothFactor);
        _simData.
        transform.position = pos;

        Debug.DrawLine(transform.position, _simData.projectedPositionWS, Color.magenta);*/
    }
}
