using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Core.World
{
    public static class GameWorldHandlerData
    {
        public static bool IsInitialized { get; private set; } = false;

        public static Transform PlayerSpawnPoint { get; set; }
        public static Transform IslandCenterPoint { get; set; }

        public static WaterSurface MainOceanSurface { get; set; }

        public static void Reset()
        {
            PlayerSpawnPoint = null;
            IslandCenterPoint = null;
            MainOceanSurface = null;
        }
    }

}
