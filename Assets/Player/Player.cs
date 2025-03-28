using LightHouse.Inputs;
using UnityEngine;
using LightHouse.Inventory;

namespace LightHouse.KinematicCharacterController
{
    [DefaultExecutionOrder(-20)]
    public class Player : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private PlayerCharacter _playerCharacter;

        [Header("Camera")]
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private CameraSpring _cameraSpring;
        [SerializeField] private CameraLean _cameraLean;

        [Header("Inventory")]
        [SerializeField] private PlayerInventory _inventory;

        [Header("Interactions")]
        [SerializeField] private PlayerInteractions _interactions;

        [Header("Character Input Control")]
        [SerializeField] private bool _enableAllCharacterInputs = true;
        [SerializeField] private bool _enableMoveInput = true;
        [SerializeField] private bool _enableCameraRotationInput = true;

        [SerializeField] private bool _enableSprintInput = true;
        [SerializeField] private bool _enableJumpInput = true;
        [SerializeField] private bool _enableCrouchInput = true;

        private PlayerInputActions _inputActions;

        //PROPERTIES
        public PlayerCharacter PlayerCharacter => _playerCharacter;
        public PlayerCamera Camera => _playerCamera;
        public CameraSpring CameraSpring => _cameraSpring;
        public CameraLean CameraLean => _cameraLean;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            InputManager.SetPlayerInputActions(_inputActions);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _inventory.RegisterInput();

            _playerCharacter.Initialize();
            _playerCamera.SetFollowTransform(_playerCharacter.GetCameraTarget());
            _cameraSpring.Initialize();
            _cameraLean.Initialize();
        }

        private void Update()
        {
            HandleCharacterInput();
            _playerCharacter.UpdateCapsuleMeshRoot(Time.deltaTime);
        }

        private void LateUpdate()
        {
            HandleCameraInput();

            Transform camTarget = _playerCharacter.GetCameraTarget();
            CharacterState state = _playerCharacter.GetState();

            HandleSpring(Time.deltaTime, camTarget.up);
            HandleLean(Time.deltaTime, state.AccelerationVelocity, camTarget.up);

            HandleInventory();
        }

        private void OnDestroy()
        {
            _inventory.UnregisterInput();
            InputManager.DisposePlayerInputActions();
            _inputActions.Dispose();
        }

        private void HandleCharacterInput()
        {
            if (!_enableAllCharacterInputs) return;

            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            if (_enableMoveInput)
                characterInputs.MoveInput = _inputActions.Player.Move.ReadValue<Vector2>();

            if (_enableCameraRotationInput)
                characterInputs.CameraRotation = _playerCamera.transform.rotation;

            if (_enableJumpInput)
                characterInputs.Jump = _inputActions.Player.Jump.WasPressedThisFrame();

            if (_enableCrouchInput)
            {
                characterInputs.Crouch = _inputActions.Player.Crouch.WasPressedThisFrame()
               ? CrouchInput.Toggle
               : CrouchInput.None;
            }

            if (_enableSprintInput)
            {
                characterInputs.Sprint = _inputActions.Player.Sprint.IsPressed()
               ? SprintInput.Sprinting
               : SprintInput.None;
            }

            // Apply inputs to character
            _playerCharacter.SetInputs(ref characterInputs);
        }

        private void HandleCameraInput()
        {
            //Known issue, can cause some trouble if we let possibility to player to move
            //while this is false, because of the character.SetInputs() due to ProjectOnPlane
            //and planar calculation
            if (_enableCameraRotationInput)
            {
                CameraInput cameraInput = new CameraInput() { Look = _inputActions.Player.Look.ReadValue<Vector2>() };
                _playerCamera.UpdateWithInput(Time.deltaTime, cameraInput.Look);
            }
            
        }

        private void HandleLean(float deltaTime, Vector3 acceleration, Vector3 up)
        {
            _cameraLean.UpdateLean(deltaTime, acceleration, up);
        }

        private void HandleSpring(float deltaTime, Vector3 up)
        {
            _cameraSpring.UpdateSpring(deltaTime, up);
        }

        private void HandleInventory()
        {
            _inventory.HandleInventoryPosition();
            _inventory.HandleInventoryRotation();
        }
    }

}
