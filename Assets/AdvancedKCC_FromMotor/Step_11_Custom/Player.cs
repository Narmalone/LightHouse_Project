using UnityEngine;
using LightHouse.Inputs;

namespace LightHouse.AdvancedController
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private PlayerCamera playerCamera;
        [Space]
        [SerializeField] private CameraSpring cameraSpring;
        [SerializeField] private CameraLean cameraLean;

        private PlayerInputActions _inputActions;
        private PlayerInputActions.PlayerActions playerActions;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();

            InputManager.SetPlayerInputActions(_inputActions);
            playerActions = _inputActions.Player;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            playerCharacter.Initialize();
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
            cameraSpring.Initialize();
            cameraLean.Initialize();
        }

        private void OnDestroy()
        {
            InputManager.DisposePlayerInputActions();
            _inputActions.Dispose();
        }

        private void Update()
        {
            //Get Cam input and update it's rotation
            CameraInput cameraInput = new CameraInput { Look = playerActions.Look.ReadValue<Vector2>() };
            playerCamera.UpdateRotation(cameraInput);

            //une fois que le chara avec le motor ont été mis à jour on fait match la pos de la cam avec
            //la target
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());

            //Get character input and update it
            CharacterInput characterInput = new CharacterInput
            {
                Rotation = playerCamera.transform.rotation,
                Move = playerActions.Move.ReadValue<Vector2>(),
                Jump = playerActions.Jump.WasPressedThisFrame(),
                Sprint = playerActions.Sprint.IsPressed()
                    ? SprintInput.Sprinting
                    : SprintInput.StopSprint,
                Crouch = playerActions.Crouch.WasPressedThisFrame()
                    ? CrouchInput.Toggle
                    : CrouchInput.None
            };
            playerCharacter.UpdateInput(characterInput);
            playerCharacter.UpdateBody(Time.deltaTime); //control stand camera height by stand value
        }

        private void LateUpdate()
        {
            Transform cameraTarget = playerCharacter.GetCameraTarget();
            CharacterState currSate = playerCharacter.GetCharacterState();
            cameraSpring.UpdateSpring(Time.deltaTime, cameraTarget.up);
            cameraLean.UpdateLean(Time.deltaTime, currSate.Acceleration, cameraTarget.up);
        }

        public void Teleport(Vector3 pos)
        {
            playerCharacter.SetPosition(pos);
        }
    }

}
