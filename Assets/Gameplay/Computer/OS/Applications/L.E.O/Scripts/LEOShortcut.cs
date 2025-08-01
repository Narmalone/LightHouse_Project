using LightHouse.Game.Computer.OS;
using UnityEngine;

public class LEOShortcut : ShortCutController
{
    private void Start()
    {
        OnExecute();
        _currentInstance.OnClose();
    }
}
