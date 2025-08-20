using LightHouse.Game.Computer.Cameras;
using UnityEngine;

namespace LightHouse.Game.Computer
{
    public class LightHouseComputer : MonoBehaviour
    {
        [SerializeField] private ComputerServices _computerServices;
        [SerializeField] private LightHouseComputerCamera _cameraController;
        [SerializeField] private LightHouseCamerasSystem _cameraSystem;

        [SerializeField] private OS.OS _os;

        private void Awake()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            _computerServices.CameraSystem = _cameraSystem;

            _os.SetService(this._computerServices);
        }

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
