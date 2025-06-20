using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public static class GameWorldHandlerData
{
    public static bool IsInitialized { get; private set; } = false;

    public static Transform PlayerSpawnPoint;
    public static Transform IslandCenterPoint;

    public static WaterSurface MainOceanSurface;

    public static void Reset()
    {
        PlayerSpawnPoint = null;
        IslandCenterPoint = null;
        MainOceanSurface = null;
    }
}
