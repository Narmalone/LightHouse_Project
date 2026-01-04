using LightHouse.Handlers;
using LightHouse.Inputs;
using LightHouse.KinematicCharacterController;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.myCharacter
{
    public enum PlayerState
    {
        Normal,
        ComputerMode,
        Options,
        CutScenesModes,
        LastState 
    }

    [DefaultExecutionOrder(-20)]
    public class Player : MonoBehaviour
    {
        #region FIELDS
        public bool EnableDebugMode = false;

        // Etat courant
        public PlayerState PlayerState { get; private set; } = PlayerState.Normal;
        public PlayerState PreviousState { get; private set; } = PlayerState.Normal;

        // Dernier état (précédent réel)
        public PlayerState LastState = PlayerState.Normal;

        public static Action<PlayerState> ForceChangePlayerState;

        [Header("Character")]
        [SerializeField] private PlayerCharacter _playerCharacter;
        [SerializeField] private LayerMask _skyBlockerMask = 1 << 1;

        [Header("Camera")]
        [SerializeField] private PlayerCamera _playerCamera;

        [Header("Interactions")]
        [SerializeField] private InteractionItemsUIManager _interactions;

        [Header("Inventory")]
        [SerializeField] private PlayerInventoryManager _inventoryController;

        [SerializeField] private Image _playerCenterDotImage;

        [Header("Character Input Control")]
        [SerializeField] private bool _enableAllCharacterInputs = true;
        [SerializeField] private bool _enableMoveInput = true;
        [SerializeField] private bool _enableCameraRotationInput = true;
        [SerializeField] private bool _enableSprintInput = true;
        [SerializeField] private bool _enableJumpInput = true;
        [SerializeField] private bool _enableCrouchInput = true;

        private bool _isInitialized = false;
        private PlayerInputActions _inputActions;

        public PlayerCharacter Character => _playerCharacter;
        public PlayerCamera PlayerCamera => _playerCamera;
        public PlayerInventoryManager Inventory => _inventoryController;
        #endregion

        #region UNITY LIFECYCLE
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ForceChangePlayerState += PlayerChangeState;

            if (!InputManager.IsInitialized)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Enable();
                InputManager.SetPlayerInputActions(_inputActions);
            }
            _playerCharacter.Initialize();
        }

        private void Start()
        {
            if (EnableDebugMode)
            {
                PlayerHandlerData.InitializeHandlerData(this);
                if(!InputManager.IsInitialized)
                    InputManager.InputManagerInitialized();
                _isInitialized = true;

                if(GameWorldHandlerData.PlayerSpawnPoint != null)
                {
                    _playerCharacter.SetPosition(GameWorldHandlerData.PlayerSpawnPoint.position);
                    _playerCharacter.SetRotation(GameWorldHandlerData.PlayerSpawnPoint.rotation);
                }
            }
        }

        private void FixedUpdate()
        {
            IsOccluded = IsPlayerOccluded();
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
        }

        private void OnDestroy()
        {
            InputManager.DisposePlayerInputActions();
            _inputActions.Dispose();
            PlayerHandlerData.Dispose();
            ForceChangePlayerState -= PlayerChangeState;
        }
        #endregion

        #region CHARACTER INPUTS
        private void HandleCharacterInput()
        {
            if (!_enableAllCharacterInputs) return;

            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

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
                _playerCamera.UpdateWithInput(Time.deltaTime, cameraInput.Look, _inputActions.Player.Move.ReadValue<Vector2>());
            }
        }
        #endregion

        public bool IsOccluded;
        public bool IsPlayerOccluded()
        {
            Vector3 origin = Character.transform.position + Vector3.up * 0.1f;
            return Physics.Raycast(origin, Vector3.up, 15.0f, _skyBlockerMask, QueryTriggerInteraction.Ignore);
        }

        // ---- STATE MACHINE ----
        private void PlayerChangeState(PlayerState requested)
        {
            // Interpréter "LastState" comme "aller au dernier état entré"
            if (requested == PlayerState.LastState)
            {
                requested = LastState; // typiquement == PlayerState → no-op
            }

            // Si on demande le même état, ne rien faire
            if (requested == PlayerState) return;

            // Mémoriser l'ancien AVANT de basculer
            var previous = PlayerState;

            // Appliquer les effets d'entrée du nouvel état (ne pas toucher PlayerState ici)
            switch (requested)
            {
                case PlayerState.Normal: EnterNormal(); break;
                case PlayerState.ComputerMode: EnterCameraMode(); break;
                case PlayerState.Options: EnterOptionsMode(); break;
                case PlayerState.CutScenesModes: EnterCutScenes(); break;
                default: EnterNormal(); break;
            }

            // Valider le changement
            PreviousState = previous;   // <- vrai "précédent"
            PlayerState = requested;  // <- courant
            LastState = PlayerState; // <- dernier entré (souvent == courant)
        }

        // Helper : revenir au vrai état précédent (si tu en as besoin)
        public void RevertToPreviousState()
        {
            if (PreviousState != PlayerState)
                PlayerChangeState(PreviousState);
        }

        // Helper actuel : aller au “dernier entré” (souvent no-op si déjà dessus)
        public void RevertToLastState()
        {
            PlayerChangeState(PlayerState.LastState);
        }


        private void EnterNormal()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _playerCenterDotImage.gameObject.SetActive(true);
            _enableAllCharacterInputs = true;
            _inventoryController.Enable();
            _interactions.Enable();
        }

        private void EnterCameraMode()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _playerCenterDotImage.gameObject.SetActive(false);
            _enableAllCharacterInputs = false;
            _playerCharacter.ForceCutVelocity();
            _inventoryController.Disable();
            _interactions.Disable();
        }

        private void EnterOptionsMode()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _playerCenterDotImage.gameObject.SetActive(false);
            _enableAllCharacterInputs = false;
            _playerCharacter.ForceCutVelocity();
            _inventoryController.Disable();
            _interactions.Disable();
        }

        private void EnterCutScenes()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _playerCenterDotImage.gameObject.SetActive(false);
            _enableAllCharacterInputs = false;
            _playerCharacter.ForceCutVelocity();
            _inventoryController.Disable();
            _interactions.Disable();
        }
    }
}
