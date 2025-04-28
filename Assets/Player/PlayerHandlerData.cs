using LightHouse.KinematicCharacterController;
using System;

namespace LightHouse.Handlers
{
    public static class PlayerHandlerData
    {
        public static Player MainPlayer;

        public static event Action OnHandlerInitialized;
        public static void InitializeHandlerData(Player player)
        {
            MainPlayer = player;
            OnHandlerInitialized?.Invoke();
        }

        public static void Dispose()
        {
            MainPlayer = null;
            OnHandlerInitialized = null;
        }
    }

}
