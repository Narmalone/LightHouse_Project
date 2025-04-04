using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Locators;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightHouse.Items.Samples
{
    public class PlaceHolderKeyMover : KeyItemUseTracker
    {
        private bool _isObjectTargetMoving = false;
        [SerializeField] private float SharpnessPosition = 25f;
        [SerializeField] private float SharpnessRotation = 25f;
        public event Action<PlaceHolderKeyMover> OnPlaceHolderKeyCompleted;
        public Key TargetObject;
        public bool CanObjectMoveToPosition { get => _isObjectTargetMoving; set { _isObjectTargetMoving = value; } }
        public override string GetInteractionName()
        {
            
            return base.GetInteractionName();
        }

        public override void Interact()
        {
            
        }

        private void Start()
        {
            PlayerInventory.OnItemDropped += PlayerInventory_OnItemDropped;
        }

        private void PlayerInventory_OnItemDropped(IInventoryItem obj)
        {
            
        }
        public override void OnRaycastEnd()
        {
            base.OnRaycastEnd();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            PlayerInventory.OnItemDropped -= PlayerInventory_OnItemDropped;
        }

        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();

            //TO DO :: Handle si l'objet est stackable dans l'inventaire -> revoir le systeme
            //de stacking d'objets ?

            if (KeyHandedItem is IInventoryStackable stack)
            {
                if(stack.CurrentStack > 1)
                {
                    TargetObject = Locator<PlayerInventory>.Instance.GetItemFromSlot(KeyHandedItem) as Key;
                    KeyHandedItem.ForceRemoveItemFromInventory(KeyHandedItem.transform.position, 0f, false, false);
                }
                else
                {
                    TargetObject = KeyHandedItem.ForceRemoveItemFromInventory(KeyHandedItem.transform.position, 0f, false, false);
                }

            }
            else
            {
                TargetObject = KeyHandedItem.ForceRemoveItemFromInventory(KeyHandedItem.transform.position, 0f, false, false);
            }
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

            TargetObject.transform.localScale = this.transform.localScale;

            float posDist = Vector3.Distance(TargetObject.transform.position, transform.position);
            Vector3 eulersDist = (TargetObject.transform.eulerAngles - transform.eulerAngles);
            
            if (posDist <= 0.0001f && eulersDist.magnitude < 0.01f)
            {
                TargetObject.transform.position = this.transform.position;
                TargetObject.transform.rotation = this.transform.rotation;

                _isObjectTargetMoving = false;
                TargetObject.GetCollider().enabled = true;
                TargetObject.gameObject.layer = 0;
                TargetObject.enabled = false;
                OnPlaceHolderKeyCompleted?.Invoke(this);
                TargetObject = null;
            }
        }

    }

}
