using LightHouse.GrabableItems;
using LightHouse.Handlers;
using LightHouse.Inputs;
using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.KinematicCharacterController;
using LightHouse.Items.Detection;
using System;
using UnityEngine;

namespace LightHouse.Items.Grabable
{
    [RequireComponent(typeof(Rigidbody))]
    public class BaseGrabableItem : MonoBehaviour, IInteractable, IItemCallback
    {
        #region FIELDS
        [Header("Main References")]
        [SerializeField] private string _itemName;
        [SerializeField] private string _alreadyGrabbingAnObjectText;
        [SerializeField] private Collider _col;
        [SerializeField] private Rigidbody rb;

        [Header("Grab Settings")]
        [SerializeField] private float _moveForce = 500f;
        [SerializeField] private float _rotationSpeed = 20f;
        [SerializeField] private float _maxClampedVelocity = 10f;
        [SerializeField] private Vector3 _offsetPos;
        [SerializeField] private float _objectAtRange = 1.0f;
        [SerializeField] private float _maxItemRangeToAutomaticRelease = 4.0f;

        [Header("Automatically Setted")]
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        //DEBUG ONLY
        private Transform _targetPoint;
        private bool _isGrabbed = false;
        private float _baseMass;
        private Vector3 _grabRayOrigin;
        private Vector3 _grabRayDirection;
        private PlayerCharacter _playerCharacter;
        private CameraRaycastDetector _inventoryRaycastDetector;
        #endregion

        #region IInteractable
        public string GetName() => _itemName;
        public GameObject GetGameObject() => this.gameObject;
        public Collider GetCollider() => _col;

        public string GetInteractionName()
        {
            if (!CanBeInteracted && !_isGrabbed)
                return _alreadyGrabbingAnObjectText;
            return _isGrabbed ?
                $"Press {InputManager.GetBindingName(InputManager.Interact)} to release." :
                $"Press {InputManager.GetBindingName(InputManager.Interact)} to grab.";
        }

        public void Interact()
        {
            if (_isGrabbed)
                Release();
            else
                Grab(_targetPoint);
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
        }

        public void OnRaycastEnd() { }
        #endregion

        #region MONO CALLBACK
        private void Awake()
        {
            _baseMass = rb.mass;
            PlayerGrabableItems.OnPlayerGrabableInitialized += PlayerGrabableItems_OnPlayerGrabableInitialized;
            PlayerHandlerData.OnHandlerInitialized += PlayerHandlerData_OnHandlerInitialized;
        }

        private void FixedUpdate()
        {
            if (!_isGrabbed || _targetPoint == null) return;

            _grabRayOrigin = _inventoryRaycastDetector.RayOrigin;
            Vector3 fullDirection = _inventoryRaycastDetector.RayDirection.normalized;
            Vector3 targetPos = _grabRayOrigin + (fullDirection * _objectAtRange) + _offsetPos;
            Vector3 toTarget = targetPos - rb.position;
            float dist = toTarget.magnitude;

            if (dist < 0.01f)
                rb.velocity = Vector3.zero;
            else
                rb.velocity = Vector3.ClampMagnitude(toTarget * _moveForce * Time.fixedDeltaTime, _maxClampedVelocity);

            if (dist >= _maxItemRangeToAutomaticRelease)
            {
                rb.velocity = Vector3.zero;
                Release();
                return;
            }

            // Rotation fluide
            Quaternion desiredRotation = _targetPoint.rotation;
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, desiredRotation, _rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedRotation);
        }

        private void OnDestroy()
        {
            PlayerGrabableItems.OnPlayerGrabableInitialized -= PlayerGrabableItems_OnPlayerGrabableInitialized;
            PlayerHandlerData.OnHandlerInitialized -= PlayerHandlerData_OnHandlerInitialized;
        }
        #endregion

        #region Grab & Release
        public void Grab(Transform grabTarget)
        {
            if (_isGrabbed || grabTarget == null) return;

            rb.freezeRotation = true;
            rb.useGravity = false;
            rb.mass = 0.01f;

            _playerCharacter.IgnoreCollider(_col);
            Physics.IgnoreCollision(_col, _playerCharacter.Motor.Capsule, true);

            InventoryHandlerData.SetGrabbingObject(true);

            _isGrabbed = true;
            _targetPoint = grabTarget;

            if (IsItemRaycasted)
                OnInteractionNameChanged?.Invoke();
        }

        public void Release()
        {
            rb.useGravity = true;
            rb.freezeRotation = false;
            rb.mass = _baseMass;

            InventoryHandlerData.SetGrabbingObject(false);

            _playerCharacter.RestoreCollider(_col);
            Physics.IgnoreCollision(_col, _playerCharacter.Motor.Capsule, false);
            _isGrabbed = false;

            if (IsItemRaycasted)
                OnInteractionNameChanged?.Invoke();
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
        #endregion
    }
}
