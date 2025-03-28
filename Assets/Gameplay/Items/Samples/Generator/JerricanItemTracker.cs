using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class JerricanItemTracker : ObjectItemTracker
    {
        [SerializeField] protected string _whenHasItemButNotOnHands = "Please take your jerrican on your hands to fill.";
        [SerializeField] protected string _doesntHaveTheItem = "You should find a jerrican somewhere...";
        protected JerricanEssence _currentJerricanOnhands;

        public event Action<JerricanEssence> OnJerricanUsed;
        private void Awake()
        {
            PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
        }

        private void OnDestroy()
        {
            PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;
        }
        public override string GetInteractionName()
        {
            return _interactionName;
        }

        public override string GetName() => _itemName;

        public override void Interact() { }

        public override void OnRaycastStart()
        {
            bool hasJerricanOnHands = IsCurrentSelectedItemTypeOf<JerricanEssence>();
            if (hasJerricanOnHands)
            {
                InteractionName = $"{InputManager.GetBindingName(InputManager.InteractInInventory)} to fill";
                _currentJerricanOnhands = HasTargetItemOnHands<JerricanEssence>();
                if (_currentJerricanOnhands == null) return;
                SubscribeToItem(_currentJerricanOnhands as IInventoryItemUsable);
            }
            else
            {
                if (HasItemInInventoryOfType<JerricanEssence>())
                {
                    InteractionName = _whenHasItemButNotOnHands;
                }
                else
                {
                    InteractionName = _doesntHaveTheItem;
                }
            }
        }

        public override void OnRaycastEnd()
        {
            if (_currentJerricanOnhands != null)
            {
                UnSubscribeToItem(_currentJerricanOnhands);
                _currentJerricanOnhands = null;
            }
        }

        private void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return;
            bool hasItemOnHands = IsCurrentSelectedItemTypeOf<JerricanEssence>();

            if(hasItemOnHands)
            {
                InteractionName = $"{InputManager.GetBindingName(InputManager.InteractInInventory)} to fill";
            }
            else
            {
                if (HasItemInInventoryOfType<JerricanEssence>())
                {
                    InteractionName = _whenHasItemButNotOnHands;
                }
                else
                {
                    InteractionName = _doesntHaveTheItem;
                }
            }
            //if player doesn't have a selected jerrican before and now has one
            if (_currentJerricanOnhands == null && hasItemOnHands)
            {
                _currentJerricanOnhands = HasTargetItemOnHands<JerricanEssence>();
                if (_currentJerricanOnhands == null) return;
                SubscribeToItem(_currentJerricanOnhands as IInventoryItemUsable);
            }
            //if he switched from a jerrican to an other jerrican
            else if(_currentJerricanOnhands != null && hasItemOnHands)
            {
                UnSubscribeToItem(_currentJerricanOnhands);
                _currentJerricanOnhands = HasTargetItemOnHands<JerricanEssence>();
                SubscribeToItem(_currentJerricanOnhands as IInventoryItemUsable);
            }
        }

        public void SubscribeToItem(IInventoryItemUsable usable)
        {
            usable.CanBeUsedFromInventory = true;
            usable.InvokeOnCanBeUsedFromInventoryChanged();
            usable.OnItemUsed += Usable_OnItemUsed;
        }

        public void UnSubscribeToItem(IInventoryItemUsable usable)
        {
            usable.CanBeUsedFromInventory = false;
            usable.InvokeOnCanBeUsedFromInventoryChanged();
            usable.OnItemUsed -= Usable_OnItemUsed;
        }

        private void Usable_OnItemUsed()
        {
            OnJerricanUsed?.Invoke(_currentJerricanOnhands);
        }
    }

}
