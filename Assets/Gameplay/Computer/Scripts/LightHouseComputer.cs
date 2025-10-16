using LightHouse.Game.Computer.Cameras;
using LightHouse.KinematicCharacterController;
using UnityEngine;

namespace LightHouse.Game.Computer
{
    public class LightHouseComputer : MonoBehaviour
    {
        [SerializeField] private ComputerServices _computerServices;
        [SerializeField] private InteractableComputer _computerInteractionSystem;
        [SerializeField] private LightHouseComputerCamera _cameraController;
        [SerializeField] private LightHouseCamerasSystem _cameraSystem;

        [SerializeField] private OS.OS _os;

        private void Awake()
        {
            InitializeServices();
            _computerInteractionSystem.OnObjectInteracted += ComputerInteractionSystem_OnObjectInteracted;
            _os.OnLeftComputerCalled += Os_OnLeftComputerCalled;
        }

        private void OnDestroy()
        {
            _computerInteractionSystem.OnObjectInteracted -= ComputerInteractionSystem_OnObjectInteracted;
            _os.OnLeftComputerCalled -= Os_OnLeftComputerCalled;
        }

        private void ComputerInteractionSystem_OnObjectInteracted()
        {
            ComputerEnter();
        }

        private void Os_OnLeftComputerCalled()
        {
            ComputerExit();
        }

        private void InitializeServices()
        {
            _computerServices.CameraSystem = _cameraSystem;

            _os.SetService(this._computerServices);
        }

        public void ComputerEnter()
        {
            _cameraController.EnableComputerCamera();
           _computerInteractionSystem.Collider.enabled = false;
            //_computerInteractionSystem.gameObject.SetActive(false);
            _os.BootOS();
            _os.PlayerOnComputer = true;
            Player.ForceChangePlayerState?.Invoke(PlayerState.ComputerMode);
        }

        public void ComputerExit()
        {
            _cameraController.DisableComputerCamera();
            _computerInteractionSystem.Collider.enabled = true;
            //_computerInteractionSystem.gameObject.SetActive(true);
            _os.PlayerOnComputer = false;
            Player.ForceChangePlayerState?.Invoke(PlayerState.Normal);
        }
    }

}
