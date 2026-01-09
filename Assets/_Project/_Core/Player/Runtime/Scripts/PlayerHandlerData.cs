using LightHouse.KinematicCharacterController;
using System;

namespace LightHouse.Handlers
{
    public static class PlayerHandlerData
    {
        public static Player MainPlayer;

        public static event Action OnHandlerInitialized;
        public static bool IsInitialized = false;


        public static bool IsPlayerOccluded()
        {
            if (MainPlayer == null) return false;
            return MainPlayer.IsOccluded;
        }

        public static void InitializeHandlerData(Player player)
        {
            IsInitialized = true;
            MainPlayer = player;
            OnHandlerInitialized?.Invoke();
        }

        public static void Dispose()
        {
            IsInitialized = false;
            MainPlayer = null;
            OnHandlerInitialized = null;
        }
    }

}
