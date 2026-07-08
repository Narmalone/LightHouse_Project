using LightHouse.Core.Audio;
using LightHouse.Core.Inputs;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    private void Start()
    {
        InitializePlayerInputs();
    }

    private void OnDestroy()
    {
        ReleasePlayerInputs();
    }

    public void InitializePlayerInputs()
    {
        InputManager.Initialize();
    }

    public void ReleasePlayerInputs()
    {
        InputManager.DisposePlayerInputActions();
    }
}
