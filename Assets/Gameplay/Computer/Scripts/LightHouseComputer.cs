using UnityEngine;

namespace LightHouse.Game.Computer
{
    public class LightHouseComputer : MonoBehaviour
    {
        [SerializeField] private LightHouseComputerCamera _cameraController;
        public void ComputerEnter()
        {
            _cameraController.EnableComputerCamera();
        }

        public void ComputerExit()
        {
            _cameraController.DisableComputerCamera();
        }
    }

}
