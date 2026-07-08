using LightHouse.Core.Inputs;
using LightHouse.Core.Player.Interactions.UI;
using LightHouse.Core.Player.Inventory;
using LightHouse.Core.Save;
using LightHouse.Core.Save.Sample;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Core.Player
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
    public class PlayerController : MonoBehaviour, IBind<PlayerData>, ICapturableState
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
        private PlayerInputActions _inputActions => InputManager.PLAYER_INPUTS_ACTIONS;

        public PlayerCharacter Character => _playerCharacter;
        public PlayerCamera PlayerCamera => _playerCamera;
        public PlayerInventoryManager Inventory => _inventoryController;
        public InteractionItemsUIManager Interactions => _interactions;

        public bool EnableAllCharacterInputs
        {
            get => _enableAllCharacterInputs;
            set => _enableAllCharacterInputs = value;
        }

        public bool EnableCameraRotationInput
        {
            get => _enableCameraRotationInput;
            set => _enableCameraRotationInput = value;
        }
        #endregion

        #region SAVE 
        [SerializeField] private PlayerData _data;
        [field:  SerializeField] public SerializableGuid Id { get; set; }

        #endregion

        #region UNITY LIFECYCLE
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ForceChangePlayerState += PlayerChangeState;
            _playerCharacter.Initialize();

            PlayerHandlerData.InitializeHandlerData(this);
                
            _isInitialized = true;
        }

        private void FixedUpdate()
        {
            IsOccluded = IsPlayerOccluded();
            if(_wasOccluded != IsOccluded)
            {
                if (IsOccluded) AudioEnvironmentController.Instance.SetIndoor();
                else { AudioEnvironmentController.Instance.SetOutdoor(); }
                _wasOccluded = IsOccluded;
            }
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("saving...");
                Save.SaveLoadSystem.Instance.SaveGame();
            }

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
        private bool _wasOccluded;
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

        #region SAVE
        /// <summary>
        /// Est appelé quand la scène de jeu est chargée uniquement
        /// </summary>
        /// <param name="data"></param>
        public void Bind(PlayerData data)
        {
            this._data = data;
            this._data.Id = Id;
            this.Character.SetPosition(data.position);
            this.Character.SetRotation(data.rotation);
        }

        /// <summary>
        /// Rentrer les données à save
        /// </summary>
        public void CaptureState()
        {
            _data.position = Character.transform.position;
            _data.rotation = Character.transform.rotation;
        }
        #endregion
    }
}
