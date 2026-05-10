using LightHouse.Core.Services;
using LightHouse.Features.Computer.MineSweeper;
using LightHouse.Features.Computer.OS;
using UnityEngine;

public class MineSweeperComputerApp : ComputerApp
{
    [SerializeField] private MineSweeperGameController _gameController;
    public override void OnClose(bool playSound = true)
    {
        if (_onCloseSound != null && playSound && ServiceLocator.Audio != null)
            ServiceLocator.Audio.PlayAt(_onCloseSound, this.transform.position);

        Destroy(this.gameObject);
    }

    public override void OnMinimize()
    {

    }

    public override void OnOpen(bool playSound = true)
    {
        if (_onOpenSound != null && playSound && ServiceLocator.Audio != null)
            ServiceLocator.Audio.PlayAt(_onOpenSound, this.transform.position);

        if (_gameController != null)
            _gameController.Initialize();
    }
}
