using LightHouse.Inputs;
using UnityEngine;
using LightHouse.Handlers;
using LightHouse.Game.BootStrap;
using System;

namespace LightHouse.KinematicCharacterController
{
    /// <summary>
    /// Pre-states of the players
    /// <see cref="Normal"/> is when the player can freely move
    /// <see cref="CameraMode"/> is when the player is seeing the cameras from the computer
    /// <see cref="CutScenesModes"/> is when the player is seeing 
    /// </summary>
    public enum PlayerState
    {
        Normal,
        CameraMode,
        CutScenesModes
    }
    [DefaultExecutionOrder(-20)]
    public class Player : MonoBehaviour
    {
        #region FIELDS
        public bool EnableDebugMode = false;
        public PlayerState PlayerState = PlayerState.Normal;
        public static Action<PlayerState> ForceChangePlayerState;

        [Header("Character")]
        [SerializeField] private PlayerCharacter _playerCharacter;

        [Header("Camera")]
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private CameraSpring _cameraSpring;
        [SerializeField] private CameraLean _cameraLean;

        [Header("Interactions")]
        [SerializeField] private InteractionItemsUIManager _interactions;

        [Header("Inventory")]
        [SerializeField] private PlayerInventoryManager _inventoryController;

        [Header("Character Input Control")]
        [SerializeField] private bool _enableAllCharacterInputs = true;
        [SerializeField] private bool _enableMoveInput = true;
        [SerializeField] private bool _enableCameraRotationInput = true;
        [SerializeField] private bool _enableSprintInput = true;
        [SerializeField] private bool _enableJumpInput = true;
        [SerializeField] private bool _enableCrouchInput = true;

        private bool _isInitialized = false;
        private PlayerInputActions _inputActions;
        #endregion

        #region PROPERTIES
        public PlayerCharacter Character => _playerCharacter;
        public PlayerInventoryManager Inventory => _inventoryController;
        public InteractionItemsUIManager Interactions => _interactions;
        public PlayerCamera PlayerCamera => _playerCamera;
        public CameraSpring CameraSpring => _cameraSpring;
        public CameraLean CameraLean => _cameraLean;
        #endregion

        #region UNITY LIFECYCLE
        private void Awake()
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            ForceChangePlayerState += PlayerChangeState;
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            _playerCharacter.Initialize();
            _playerCamera.SetFollowTransform(_playerCharacter.GetCameraTarget());
            _cameraSpring.Initialize();
            _cameraLean.Initialize();

            BootStrap.OnGameAssetsLoaded += BootStrap_OnGameSceneInitialized;
        }

        private void Start()
        {
            if (EnableDebugMode)
            {
                PlayerHandlerData.InitializeHandlerData(this);
                InputManager.SetPlayerInputActions(_inputActions);
                InputManager.InputManagerInitialized();
                _cameraSpring.enabled = true;
                _isInitialized = true;
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;
            HandleCharacterInput();
            _playerCharacter.UpdateCapsuleMeshRoot(Time.deltaTime);

        }

        private void LateUpdate()
        {
            if (!_isInitialized) return;
            if (!_enableAllCharacterInputs) return;
            HandleCameraInput();

            Transform camTarget = _playerCharacter.GetCameraTarget();
            CharacterState state = _playerCharacter.GetState();

            HandleSpring(Time.deltaTime, camTarget.up);
            HandleLean(Time.deltaTime, state.AccelerationVelocity, camTarget.up);
        }

        private void OnDestroy()
        {
            InputManager.DisposePlayerInputActions();
            _inputActions.Dispose();
            PlayerHandlerData.Dispose();
            BootStrap.OnGameAssetsLoaded -= BootStrap_OnGameSceneInitialized;
            ForceChangePlayerState -= PlayerChangeState;
        }
        #endregion

        #region BootStrap
        private void BootStrap_OnGameSceneInitialized()
        {
            PlayerHandlerData.InitializeHandlerData(this);
            InputManager.SetPlayerInputActions(_inputActions);
            InputManager.InputManagerInitialized();

            Character.SetPosition(GameWorldHandlerData.PlayerSpawnPoint.position);
            Character.SetRotation(GameWorldHandlerData.PlayerSpawnPoint.rotation);
            _cameraSpring.enabled = true;

            _isInitialized = true;
        }
        #endregion

        #region CHARACTER INPUTS
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

            _playerCharacter.SetInputs(ref characterInputs);
        }
        #endregion

        #region CAMERA INPUTS
        private void HandleCameraInput()
        {
            if (_enableCameraRotationInput)
            {
                CameraInput cameraInput = new CameraInput() { Look = _inputActions.Player.Look.ReadValue<Vector2>() };
                _playerCamera.UpdateWithInput(Time.deltaTime, cameraInput.Look);
            }
        }
        #endregion

        #region CAMERA LEAN
        private void HandleLean(float deltaTime, Vector3 acceleration, Vector3 up) => _cameraLean.UpdateLean(deltaTime, acceleration, up);
        #endregion

        #region CAMERA SPRING
        private void HandleSpring(float deltaTime, Vector3 up) => _cameraSpring.UpdateSpring(deltaTime, up);
        #endregion

        private void PlayerChangeState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Normal:
                    _enableAllCharacterInputs = true;
                    _inventoryController.Enable();
                    _interactions.Enable();
                    break;
                case PlayerState.CameraMode:
                    _enableAllCharacterInputs = false;
                    _playerCharacter.ForceCutVelocity();
                    _inventoryController.Disable();
                    _interactions.Disable();
                    break;
                case PlayerState.CutScenesModes:
                    break;
            }
        }
    }

}
