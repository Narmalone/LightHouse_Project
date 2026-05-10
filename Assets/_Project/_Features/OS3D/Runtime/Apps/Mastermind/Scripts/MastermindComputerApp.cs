using LightHouse.Core.Services;
using LightHouse.Features.Computer.Mastermind;
using LightHouse.Features.Computer.OS;
using UnityEngine;

public class MastermindComputerApp : ComputerApp
{
    [SerializeField] private MastermindGameController _gameController;
    public override void OnClose(bool playSound = true)
    {
        if(_onCloseSound != null && playSound && ServiceLocator.Audio != null)
             ServiceLocator.Audio.PlayAt(_onCloseSound, this.transform.position);

        Destroy(this.gameObject);
    }

    public override void OnMinimize()
    {
        
    }

    public override void OnOpen(bool playSound = true)
    {
        if(_onOpenSound != null && playSound && ServiceLocator.Audio != null)
             ServiceLocator.Audio.PlayAt(_onOpenSound, this.transform.position);

        if(_gameController != null)
            _gameController.Initialize();
    }
}
