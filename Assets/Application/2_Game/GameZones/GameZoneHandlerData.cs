using System;
using UnityEngine;

namespace LightHouse.Game.World
{
    public static class GameZoneHandlerData
    {
        public static event Action<GameZone> OnGameZoneChanged;
        public static GameZone CurrentZone { get; private set; }

        public static void SetGameZone(GameZone nextZone)
        {
            if (nextZone == CurrentZone) return;
            CurrentZone = nextZone;
            OnGameZoneChanged?.Invoke(nextZone);
        }
    }

}
