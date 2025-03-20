using LightHouse.Inputs;
using LightHouse.KinematicCharacterController;
using System;
using UnityEngine;

namespace LightHouse.Interactions.Samples
{
    public class Door : MonoBehaviour, IInteractable
    {
        #region SERIALIZED FIELDS
        [Header("Door Control")]
        [SerializeField] private Transform _pivot; // Le pivot de la porte
        [SerializeField] private Vector3 _openRotationAngles = new Vector3(0, 90f, 0f);
        [SerializeField] private float _rotationSpeed = 2f;

        [Header("Inventory")]
        private PlayerInventory _inventory;
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [SerializeField] private KeyType _key;

        [Header("Debug")]
        [SerializeField] private bool _hasKey = false;
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private bool _isUnLocked = false;

        #endregion

        #region PRIVATE FIELDS
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private float _lerpTime = 0f;
        private bool _isMoving = false;

        #endregion

        #region IINTERACTABLE EVENTS
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;
        public event Action OnObjectInteracted;
        #endregion

        #region MONO'S CALLBACK
        private void Awake()
        {
            // Store local rotations
            _closedRotation = _pivot.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(_openRotationAngles);
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
        }

        private void Update()
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

        private void OnDestroy()
        {
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
        }

        #endregion

        #region Register Callbacks
        private void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _inventory = obj;
        }
        #endregion

        #region IInteractable
        public string GetInteractionName()
        {
            _hasKey = _inventory.HasItem(_key);
            if (_hasKey && !_isUnLocked)
            {
                return $"Unlock door with {InputManager.GetBindingName(InputManager.Interact)}";
            }
            else if (!_isUnLocked)
            {
                return "The Door is locked you have to find the key.";
            }
            return _isOpen ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to Close"
                           : $"Press {InputManager.GetBindingName(InputManager.Interact)} to Open";
        }

        public string GetName()
        {
            return string.Empty;
        }

        public void Interact()
        {
            if (_hasKey && !_isUnLocked)
            {
                _isUnLocked = true;
                OnInteractionNameChanged?.Invoke(); //force update the GetInteractionName
                return;
            }
            if (!_isUnLocked) return;
            if (_isMoving) return; //Avoir multiple Interactions

            _isOpen = !_isOpen;
            _lerpTime = 0f; // Restart movement
            _isMoving = true; //Start motion

            // Update the Player's Interactions Raycast data
            OnNameUpdated?.Invoke();
            OnInteractionNameChanged?.Invoke();
        }

        #endregion
    }
}
