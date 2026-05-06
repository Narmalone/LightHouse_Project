using LightHouse.Features.Computer.OS;
using LightHouse.Features.Computer.Settings;
using UnityEngine;

public class ComputerSettingsApp : ComputerApp
{
    [SerializeField] private ColorSettings colorSettings;
    public override void OnClose(bool playSound = true)
    {
        
    }

    public override void OnMinimize()
    {

    }

    public override void OnOpen(bool playSound = true)
    {
        
    }
}
