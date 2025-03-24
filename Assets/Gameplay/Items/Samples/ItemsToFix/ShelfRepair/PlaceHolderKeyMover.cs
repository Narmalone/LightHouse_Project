using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class PlaceHolderKeyMover : PlaceHolderKey
    {
        private bool _isObjectTargetMoving = false;
        [SerializeField] private float SharpnessPosition = 25f;
        [SerializeField] private float SharpnessRotation = 25f;
        public event Action<PlaceHolderKeyMover> PlaceHolderKeyInteracted;

        public bool CanObjectMoveToPosition { get => _isObjectTargetMoving; set { _isObjectTargetMoving = value; } }
        public override string GetInteractionName()
        {
            if (_hasKeyAndItemOnHand)
            {
                return $"Press {InputManager.GetBindingName(InputManager.Interact)} to put in place.";
            }
            return $"Take a {_placeHolderName} in your inventory.";
        }

        public override void Interact()
        {
            if (_hasKeyAndItemOnHand && TargetObject != null)
            {
                _canCheck = false;
                CanBeInteracted = false;
                PlaceHolderKeyInteracted?.Invoke(this);
            }
        }

        private void FixedUpdate()
        {
            if (!_isObjectTargetMoving) return;

            TargetObject.transform.position = Vector3.Lerp
                (
                    a: TargetObject.transform.position,
                    b: this.transform.position,
                    t: 1f - Mathf.Exp(-SharpnessPosition * Time.deltaTime)
                );
            TargetObject.transform.rotation = Quaternion.Lerp
                (
                    a: TargetObject.transform.rotation,
                    b: this.transform.rotation,
                    t: 1f - Mathf.Exp(-SharpnessRotation * Time.deltaTime)
                );

            TargetObject.transform.localScale = this.transform.localScale;

            float dst = Vector3.Distance(TargetObject.transform.position, transform.position);

            if(dst <= 0.001f)
            {
                _isObjectTargetMoving = false;
                TargetObject.GetCollider().enabled = true;
                TargetObject.gameObject.layer = 0;
                TargetObject.enabled = false;
                InvokePlaceHolderCompleted();
            }
        }

    }

}
