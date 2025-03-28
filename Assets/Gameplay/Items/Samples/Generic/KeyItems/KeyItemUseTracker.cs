using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class KeyItemUseTracker : KeyItemTracker
    {
        [SerializeField] protected string _hasKeyInInventoryButNotOnHands = "Take your items in your hands.";
        [SerializeField] protected string _doesntHaveKeyInInventory = "Find the item to use it";
        [SerializeField] protected string _hasKeyOnhands = $"to use.";

        public event Action OnKeyUsed;
        public Key KeyHandedItem = null;
        public Key LastStoredKey = null;
        public override string GetInteractionName()
        {
            if(!HasItemOnHands && !HasItemInInventoryButNotOnHands)
            {
                return _doesntHaveKeyInInventory;
            }
            else if (HasItemInInventoryButNotOnHands && !HasItemOnHands)
            {
                return _hasKeyInInventoryButNotOnHands;
            }
            else if (HasItemOnHands)
            {
                return _hasKeyOnhands;
            }
            return string.Empty;
        }

        public override string GetName() => string.Empty;

        public override void Interact() { }

        public override void OnRaycastStart()
        {
            base.OnRaycastStart();
            KeyHandedItem = TryGetKeyItem();
            //if we see some the good the KeyHandedItem
            if (KeyHandedItem != null)
            {
                LastStoredKey = KeyHandedItem;
                SubscribeToItem(KeyHandedItem as IInventoryItemUsable);
            }
            InvokeOnInteractionNameChanged();
        }

        public override void OnRaycastEnd()
        {
            //When we stop raycast the item we unsubscribe to the item
            if (KeyHandedItem == null && LastStoredKey != null)
            {
                UnSubscribeToItem(LastStoredKey as IInventoryItemUsable);
                LastStoredKey = null;
            }
            else if(KeyHandedItem != null)
            {
                UnSubscribeToItem(KeyHandedItem as IInventoryItemUsable);
                KeyHandedItem = null;
                LastStoredKey = null;
            } 
        }

        /// <summary>
        /// called when the on hands selected changed
        /// </summary>
        /// <param name="obj"></param>
        protected override void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return;
            base.PlayerInventory_OnHandsItemSelectedChanged(obj);
            KeyHandedItem = TryGetKeyItem();

            //if we have an other same item like jerrican essence for example we want to unsubscribe the old
            //and subscribe to the new one
            if (KeyHandedItem != null && LastStoredKey != null)
            {
                UnSubscribeToItem(LastStoredKey as IInventoryItemUsable);
                SubscribeToItem(KeyHandedItem as IInventoryItemUsable);
                LastStoredKey = KeyHandedItem;
            }
            //in case we had nothing on hands before
            else if (KeyHandedItem != null)
            {
                LastStoredKey = KeyHandedItem;
                SubscribeToItem(KeyHandedItem as IInventoryItemUsable);
            }
            //if we stored something and then nothing
            else if (KeyHandedItem == null && LastStoredKey != null)
            {
                UnSubscribeToItem(LastStoredKey as IInventoryItemUsable);
                LastStoredKey = null;
            }
            InvokeOnInteractionNameChanged();
        }

        public void SubscribeToItem(IInventoryItemUsable item)
        {
            item.CanBeUsedFromInventory = true;
            item.InvokeOnCanBeUsedFromInventoryChanged();
            item.OnItemUsed += Usable_OnItemUsed;
            _hasKeyOnhands = item.UseInInventoryText();
        }

        public void UnSubscribeToItem(IInventoryItemUsable item)
        {
            item.CanBeUsedFromInventory = false;
            item.InvokeOnCanBeUsedFromInventoryChanged();
            item.OnItemUsed -= Usable_OnItemUsed;
        }

        protected virtual void Usable_OnItemUsed() { OnKeyUsed?.Invoke(); }
    }

}
