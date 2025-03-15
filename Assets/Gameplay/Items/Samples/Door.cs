using LightHouse.Inputs;
using System;
using UnityEngine;

namespace LightHouse.Interactions.Samples
{
    public class Door : MonoBehaviour, IInteractable, IDescribable
    {
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private Vector3 _openRotationAngles = new Vector3(0, 90f, 0f);
        [SerializeField] private Transform _pivot;
        [SerializeField] private float _rotationSpeed = 2f;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private float _lerpTime = 0f;
        private bool _isMoving = false;

        public event Action OnDescriptionUpdated;
        public event Action OnNameUpdated;

        private void Awake()
        {
            _closedRotation = Quaternion.Euler(_pivot.eulerAngles);
            _openRotation = Quaternion.Euler(_openRotationAngles);
        }

        private void Update()
        {
            if (_isMoving)
            {
                _lerpTime += Time.deltaTime * _rotationSpeed;
                _pivot.rotation = Quaternion.Lerp(_pivot.rotation, _isOpen ? _openRotation : _closedRotation, _lerpTime);

                //Stop the motion when rotation is near to end
                if (_lerpTime >= 0.9f)
                {
                    _isMoving = false;
                }
            }
        }

        public string GetDescription()
        {
            return _isOpen ? $"Press {InputManager.GetBindingName(InputManager.Interact)} to Close"
                           : $"Press {InputManager.GetBindingName(InputManager.Interact)} to Open";
        }

        public string GetName()
        {
            return string.Empty;
        }

        public void Interact()
        {
            if (_isMoving) return; //avoid multiple interactions at the same time

            _isOpen = !_isOpen;
            _lerpTime = 0f; //Restart motion
            _isMoving = true; //Start motion

            //Callbacks to force the GetName and GetDescription to be called
            OnNameUpdated?.Invoke();
            OnDescriptionUpdated?.Invoke();
        }
    }
}
