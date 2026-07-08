using LightHouse.Core.Audio;
using LightHouse.Core.Inputs;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    private void Start()
    {
        InitializePlayerInputs();
    }

    public void InitializePlayerInputs()
    {
        InputManager.Initialize();
    }

    public void ReleasePlayerInputs()
    {
        InputManager.DisposePlayerInputActions();

    }

    private void OnApplicationQuit()
    {
        ReleasePlayerInputs();
    }
}
