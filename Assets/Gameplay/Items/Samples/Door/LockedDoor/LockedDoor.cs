using LightHouse.Inputs;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class LockedDoor : IDUseItemTracker, IDoor
    {
        #region SERIALIZED FIELDS
        [Header("Door Control")]
        [SerializeField] protected Transform _pivot; // Le pivot de la porte
        [SerializeField] protected Vector3 _openRotationAngles = new Vector3(0, 90f, 0f);
        [SerializeField] protected float _rotationSpeed = 2f;

        [Header("Debug")]
        [SerializeField] protected bool _isUnLocked = false;
        public bool IsUnLocked => _isUnLocked;

        protected bool _isOpen = false;
        public bool IsOpen => _isOpen;

        #endregion

        #region PRIVATE FIELDS
        protected Quaternion _closedRotation;
        protected Quaternion _openRotation;
        protected float _lerpTime = 0f;
        protected bool _isMoving = false;

        #endregion

        #region MONO'S CALLBACK

        protected override void Awake()
        {
            base.Awake();
            _closedRotation = _pivot.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(_openRotationAngles);
        }

        private void Start()
        {
            CanBeInteracted = false;
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
        #endregion

        #region Register Callbacks
        public override void OnRaycastStart()
        {
            base.OnRaycastStart();
            if (_isUnLocked) return;
        }

        public override void OnRaycastEnd()
        {
            base.OnRaycastEnd();
            if (_isUnLocked) return;
        }

        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            _isUnLocked = true;
            CanBeInteracted = true;
            Open();
        }

        #endregion

        #region IInteractable
        public override string GetInteractionName()
        {
            if (!_isUnLocked)
            {
                return base.GetInteractionName();
            }
            return _isOpen
                ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to close"
                : $"Press {InputManager.GetBindingName(InputManager.Interact)} to open";
        }

        public override void Interact()
        {
            if (!_isUnLocked) return;
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
                InvokeNameUpdated();
                InvokeInteractionDescriptionUpdated();
            }
        }

        #endregion

        public void SetIsUnlocked(bool value) => _isUnLocked = value;

        public void Open()
        {
            if (!_isUnLocked) _isUnLocked = true;
            _isOpen = false;
            OnDoorInteracted();
        }

        public void Close()
        {
            _isOpen = true;
            OnDoorInteracted();
        }
    }
}
