using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class OceanController : MonoBehaviour
{
    [SerializeField] private WaterSurface _water;

    private WeatherManager _weatherManager;

    private void Awake()
    {
        
    }

    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
    }
}
