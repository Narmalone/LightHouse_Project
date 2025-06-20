using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GameWorld : MonoBehaviour
{
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private Transform _islandCenterPoint;
    [SerializeField] private WaterSurface _oceanSurface;

    private void Awake()
    {
        GameWorldHandlerData.PlayerSpawnPoint = _playerSpawnPoint;
        GameWorldHandlerData.IslandCenterPoint = _islandCenterPoint;
        GameWorldHandlerData.MainOceanSurface = _oceanSurface;
    }

    private void OnDestroy()
    {
        GameWorldHandlerData.Reset();
    }
}
