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
        }

        private void OnDestroy()
        {
            _computerInteractionSystem.OnObjectInteracted -= ComputerInteractionSystem_OnObjectInteracted;
        }

        private void ComputerInteractionSystem_OnObjectInteracted()
        {
            ComputerEnter();
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
            _os.PlayerOnComputer = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Player.ForceChangePlayerState?.Invoke(PlayerState.CameraMode);
        }

        public void ComputerExit()
        {
            _cameraController.DisableComputerCamera();
            _os.PlayerOnComputer = false;
        }
    }

}
