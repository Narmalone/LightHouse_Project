using System;
using UnityEngine;

namespace LightHouse.Game.World
{
    public static class GameZoneHandlerData
    {
        public static event Action<ZoneType> OnGameZoneChanged;
        public static ZoneType CurrentZone { get; private set; }

        public static void SetGameZone(ZoneType nextZone)
        {
            if (nextZone == CurrentZone) return;
            CurrentZone = nextZone;
            OnGameZoneChanged?.Invoke(nextZone);
        }
    }

}
