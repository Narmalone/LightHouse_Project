using System;

namespace LightHouse.Core.Player
{
    public enum EnvironmentState
    {
        Indoor,
        Outdoor
    }

    public static class PlayerHandlerData
    {
        public static PlayerController MainPlayer;

        public static event Action OnHandlerInitialized;
        public static bool IsInitialized = false;


        public static bool IsPlayerOccluded()
        {
            if (MainPlayer == null) return false;
            return MainPlayer.IsOccluded;
        }

        public static void InitializeHandlerData(PlayerController player)
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
