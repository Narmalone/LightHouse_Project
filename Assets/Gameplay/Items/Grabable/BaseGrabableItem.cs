#region LIGHTHOUSE NAMES
using LightHouse.GrabableItems;
using LightHouse.Handlers;
using LightHouse.Inputs;
using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.KinematicCharacterController;
using LightHouse.Items.Detection;
using LightHouse.Localization;
#endregion

#region UNITY NAMES
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
#endregion

namespace LightHouse.Items.Interactable
{
    [RequireComponent(typeof(Rigidbody))]
    public class BaseGrabableItem : InteractableItemBase, IInteractable, IItemCallback
    {
        #region FIELDS
        [Header(" --- GRABABLE FIELDS --- ")]
        [SerializeField] private Rigidbody _rb;

        [Header(" --- LOCALIZATION --- ")]
        [SerializeField] private LocalizedStringDatabase_InteractionTexts _interactionTextsDB;
        private LocalizedString _grabText => _interactionTextsDB.Grab;
        private LocalizedString _releaseText => _interactionTextsDB.Release;
        private LocalizedString _alreadyGrabbingAnItemText => _interactionTextsDB.Already_Grabbing_Item;
        private LocalizedString _pressToAction => _interactionTextsDB.Press_To_Action;

        [Header(" --- GRAB FIELDS --- ")]
        [SerializeField] private float _moveForce = 500f;
        [SerializeField] private float _rotationSpeed = 20f;
        [SerializeField] private float _maxClampedVelocity = 10f;
        [SerializeField] private Vector3 _offsetPos;
        [SerializeField] private float _objectAtRange = 1.0f;
        [SerializeField] private float _maxItemRangeToAutomaticRelease = 4.0f;

#pragma warning disable

        private string _currentText;
        //DEBUG ONLY
        private Transform _targetPoint;
        private bool _isGrabbed = false;
        private float _baseMass;
        private Vector3 _grabRayOrigin;
        private Vector3 _grabRayDirection;
        private PlayerCharacter _playerCharacter;
        private PlayerRaycastSystem _inventoryRaycastDetector;
        #endregion

        #region IInteractable
        public override string GetInteractionName()
        {
            return _currentText;
        }

        public override void Interact()
        {
            if (_isGrabbed)
                Release();
            else
                Grab(_targetPoint);
            InvokeObjectInteracted();
        }
        #endregion

        #region IItemCallback
        public void OnRaycastStart()
        {
            if (_isGrabbed) return;
            if (InventoryHandlerData.IsGrabbingObject)
                CanBeInteracted = false;
            else
                if (!CanBeInteracted) CanBeInteracted = true;
            UpdateInteractionText();
        }

        public void OnRaycastEnd() { }
        #endregion

        #region MONO CALLBACK
        protected override void Awake()
        {
            base.Awake();
            _baseMass = _rb.mass;
            PlayerGrabableItems.OnPlayerGrabableInitialized += PlayerGrabableItems_OnPlayerGrabableInitialized;
            PlayerHandlerData.OnHandlerInitialized += PlayerHandlerData_OnHandlerInitialized;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            InputManager.OnInitialized += InputManager_OnInputManagerInitialized;

        }

        private void InputManager_OnInputManagerInitialized()
        {
            UpdateInteractionText();
        }

        private void FixedUpdate()
        {
            if (!_isGrabbed || _targetPoint == null) return;

            _grabRayOrigin = ItemsDetectionSystem.RayOrigin;
            Vector3 fullDirection = ItemsDetectionSystem.RayDirection.normalized;
            Vector3 targetPos = _grabRayOrigin + (fullDirection * _objectAtRange) + _offsetPos;
            Vector3 toTarget = targetPos - _rb.position;
            float dist = toTarget.magnitude;

            if (dist < 0.01f)
                _rb.linearVelocity = Vector3.zero;
            else
                _rb.linearVelocity = Vector3.ClampMagnitude(toTarget * _moveForce * Time.fixedDeltaTime, _maxClampedVelocity);

            if (dist >= _maxItemRangeToAutomaticRelease)
            {
                _rb.linearVelocity = Vector3.zero;
                Release();
                return;
            }

            // Rotation fluide
            Quaternion desiredRotation = _targetPoint.rotation;
            Quaternion smoothedRotation = Quaternion.Slerp(_rb.rotation, desiredRotation, _rotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(smoothedRotation);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            PlayerGrabableItems.OnPlayerGrabableInitialized -= PlayerGrabableItems_OnPlayerGrabableInitialized;
            PlayerHandlerData.OnHandlerInitialized -= PlayerHandlerData_OnHandlerInitialized;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            InputManager.OnInitialized -= InputManager_OnInputManagerInitialized;
        }
        #endregion

        #region LOCALIZATION

        private void OnLocaleChanged(Locale locale)
        {
            UpdateInteractionText();
        }

        protected async void UpdateInteractionText()
        {
            string input = InputManager.Interact_Bind_Name;

            // Cas sans interaction possible : texte direct
            if (!CanBeInteracted && !_isGrabbed)
            {
                var op = _alreadyGrabbingAnItemText.GetLocalizedStringAsync();
                await op.Task;
                _currentText = op.Result;

                if (IsItemRaycasted)
                    InvokeInteractionDescriptionUpdated();
                return;
            }

            // Choix du texte d'action en fonction de l'état actuel
            var actionText = _isGrabbed ? _releaseText : _grabText;

            // Utilisation du builder pour construire le texte final
            _currentText = await InteractionTextBuilder.Build(
                actionText,
                input,
                _pressToAction
            );

            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region Grab & Release
        public void Grab(Transform grabTarget)
        {
            if (_isGrabbed || grabTarget == null) return;

            _rb.freezeRotation = true;
            _rb.useGravity = false;
            _rb.mass = 0.01f;

            _playerCharacter.IgnoreCollider(_detectionCollider);
            Physics.IgnoreCollision(_detectionCollider, _playerCharacter.Motor.Capsule, true);
            InventoryHandlerData.SetGrabbingObject(true);

            _isGrabbed = true;
            _targetPoint = grabTarget;
            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
            UpdateInteractionText();
        }

        public void Release()
        {
            _rb.useGravity = true;
            _rb.freezeRotation = false;
            _rb.mass = _baseMass;

            InventoryHandlerData.SetGrabbingObject(false);
            _playerCharacter.RestoreCollider(_detectionCollider);
            Physics.IgnoreCollision(_detectionCollider, _playerCharacter.Motor.Capsule, false);
            _isGrabbed = false;
            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
            UpdateInteractionText();
        }
        #endregion

        #region PlayerHandlerData Callback
        private void PlayerHandlerData_OnHandlerInitialized()
        {
            if(_playerCharacter == null)
                _playerCharacter = PlayerHandlerData.MainPlayer.Character;
            _inventoryRaycastDetector = PlayerHandlerData.MainPlayer.Inventory.RaycastDetector;
        }
        #endregion

        #region Player Grabable Callback
        private void PlayerGrabableItems_OnPlayerGrabableInitialized(PlayerGrabableItems obj)
        {
            if(_targetPoint == null)
                _targetPoint = obj.PlayerGrabableItemsParent;
        }

        protected override void OnGameInitialized()
        {
            
        }
        #endregion
    }
}
