using UnityEngine;

namespace Narmalone.AdvancedController.V3.StepOne
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private PlayerCamera playerCamera;

        private PlayerInputActions _inputActions;
        private PlayerInputActions.PlayerActions input;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            input = _inputActions.Player;

            playerCharacter.Initialize();
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
            playerCamera.UpdateRotation(cameraInput);
        }

        private void LateUpdate()
        {
            //une fois que le chara avec le motor ont été mis à jour on fait match la pos de la cam avec
            //la target
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
        }
    }

}
