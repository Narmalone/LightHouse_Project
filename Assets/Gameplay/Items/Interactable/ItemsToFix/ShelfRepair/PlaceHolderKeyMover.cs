using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Locators;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightHouse.Items.Interactable
{
    public class PlaceHolderKeyMover : IDUseItemTracker
    {
        private bool _isObjectTargetMoving = false;
        [SerializeField] private float SharpnessPosition = 25f;
        [SerializeField] private float SharpnessRotation = 25f;
        public event Action<PlaceHolderKeyMover> OnPlaceHolderKeyCompleted;

        private IInventoryItem TargetItem;
        public Transform TargetObject => TargetItem.GetGameObject().transform;
        public bool CanObjectMoveToPosition { get => _isObjectTargetMoving; set { _isObjectTargetMoving = value; } }

        protected override void Usable_OnItemUsed()
        {
            IInventoryItem item = _inventoryItemUsable as IInventoryItem;
            if (item != null)
            {
                TargetItem = item;
                TargetObject.gameObject.layer = 0;
                item.InvokeForceDropItemFromInventory(InventoryHandlerData.InventoryTargetPosition.position, 0.0f, false);
            }
            base.Usable_OnItemUsed();
            CanBeInteracted = false;
            CanBeRaycasted = false;
            _isObjectTargetMoving = true;
        }

        protected virtual void FixedUpdate()
        {
            if (!_isObjectTargetMoving || TargetObject == null) return;

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

            float posDist = Vector3.Distance(TargetObject.transform.position, transform.position);
            Vector3 eulersDist = (TargetObject.transform.eulerAngles - transform.eulerAngles);
            
            if (posDist <= 0.0001f && eulersDist.magnitude < 0.01f)
            {
                //à changer plus tard pour mettre de fausses "planches" par exemple qui n'ont pas de 
                //script / listener ou autre...
                _isObjectTargetMoving = false;
                TargetItem.GetCollider().enabled = true;
                TargetObject.transform.position = this.transform.position;
                TargetObject.transform.rotation = this.transform.rotation;
                //Destroy(TargetItem.GetGameObject());
                TargetItem = null;
                OnPlaceHolderKeyCompleted?.Invoke(this);
            }
        }

    }

}
