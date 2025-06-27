using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuyoncyTransform : MonoBehaviour
{
    [Header(" --- REFERENCES --- ")]
    [SerializeField] private WaterSurface _water;
    [SerializeField] private Rigidbody _rb;

    private WaterSearchParameters _waterSearchParams;
    private WaterSearchResult _waterSearchResult;

    private void Start()
    {
        if (_water == null)
        {
            _water = GameWorldHandlerData.MainOceanSurface;
        }
    }
    private void FixedUpdate()
    {
        if (_water == null) return;

        _waterSearchParams.startPositionWS = transform.position;
        _water.ProjectPointOnWaterSurface(_waterSearchParams, out _waterSearchResult);

        // Remplacer Y par la hauteur exacte de l'eau
        Vector3 pos = transform.position;
        pos.y = _waterSearchResult.projectedPositionWS.y;
        transform.position = pos;
    }

}
