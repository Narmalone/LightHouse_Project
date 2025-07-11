using LightHouse.Game.Computer.OS;
using System;
using UnityEngine;

namespace LightHouse.Game.Computer
{
    public class LightHouseComputer : MonoBehaviour
    {
        [SerializeField] private LightHouseComputerCamera _cameraController;
        [SerializeField] private OS.OS _os;
        public void ComputerEnter()
        {
            _cameraController.EnableComputerCamera();
            _os.PlayerOnComputer = true;
        }

        public void ComputerExit()
        {
            _cameraController.DisableComputerCamera();
            _os.PlayerOnComputer = false;
        }
    }

}
