using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Interactions.Samples
{
    public class Door : MonoBehaviour, IInteractable
    {
        #region SERIALIZED FIELDS
        [Header("Door Control")]
        [SerializeField] protected Transform _pivot; // Le pivot de la porte
        [SerializeField] protected Vector3 _openRotationAngles = new Vector3(0, 90f, 0f);
        [SerializeField] protected float _rotationSpeed = 2f;
        [SerializeField] protected Collider _doorCollider;

        [Header("Inventory")]
        protected PlayerInventory _inventory;
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        [Header("Debug")]
        [SerializeField] protected bool _isOpen = false;

        #endregion

        #region PRIVATE FIELDS
        protected Quaternion _closedRotation;
        protected Quaternion _openRotation;
        protected float _lerpTime = 0f;
        protected bool _isMoving = false;

        #endregion

        public bool IsOpen => _isOpen;

        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        #region IINTERACTABLE EVENTS
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;
        public event Action OnObjectInteracted;
        #endregion

        #region MONO'S CALLBACK
        protected virtual void Awake()
        {
            // Store local rotations
            _closedRotation = _pivot.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(_openRotationAngles);
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
        }

        protected virtual void Update()
        {
            if (_isMoving)
            {
                _lerpTime += Time.deltaTime * _rotationSpeed;
                _pivot.localRotation = Quaternion.Lerp(_pivot.localRotation, _isOpen ? _openRotation : _closedRotation, _lerpTime);

                // Stoppe le mouvement lorsqu'on est proche de la fin
                if (_lerpTime >= 0.9f)
                {
                    _isMoving = false;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
        }

        #endregion

        #region Register Callbacks
        protected virtual void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _inventory = obj;
        }
        #endregion

        #region IItemName
        public GameObject GetGameObject() => this.gameObject;

        public Collider GetCollider() => this._doorCollider;

        #endregion

        #region IInteractable
        public virtual string GetInteractionName()
        {
            return _isOpen ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to Close"
                           : $"Press {InputManager.GetBindingName(InputManager.Interact)} to Open";
        }

        public virtual string GetName()
        {
            return string.Empty;
        }

        public virtual void Interact()
        {
            OnDoorInteracted();
        }

        public virtual void OnDoorInteracted()
        {
            if (_isMoving)
            {
                _pivot.localRotation = _isOpen ? _openRotation : _closedRotation;
            }

            _isOpen = !_isOpen;
            _lerpTime = 0f; // Restart movement
            _isMoving = true; //Start motion

            if (IsItemRaycasted)
            {
                // Update the Player's Interactions Raycast data
                OnNameUpdated?.Invoke();
                OnInteractionNameChanged?.Invoke();
            }
        }

        protected void InvokeInteractionNameChanged()
        {
            OnInteractionNameChanged?.Invoke();
        }

        #endregion
    }
}
