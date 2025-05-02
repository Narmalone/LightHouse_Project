using UnityEngine;

public static class GameWorldHandlerData
{
    public static bool IsInitialized { get; private set; } = false;

    public static Transform PlayerSpawnPoint;
}
