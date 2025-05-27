using LightHouse.Handlers;
using LightHouse.Inventory;
using LightHouse.Items.Inventory;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Items.Interactable
{
    public class MopItemTrackerDecalClean : IDUseItemTracker
    {
        [Header("DECALS")]
        [SerializeField] private DecalProjector _targetDecal;

        protected override void OnGameInitialized()
        {
            base.OnGameInitialized();
            PlayerHandlerData.MainPlayer.Character.IgnoreCollider(this._detectionCollider);
        }

        protected override void Usable_OnItemUsed()
        {
            var currMop = _inventoryItemUsable as Mop;
            if (!currMop.IsWet) return;
            base.Usable_OnItemUsed();
            this.gameObject.SetActive(false);
        }

        public override void OnRaycastStart()
        {
            CheckConditions();
        }

        protected override bool CheckConditions()
        {
            _hasKeyInInventory = SlotManager.IsItemExistInInventory((ushort)_itemNeeded);
            _hasKeyOnHands = SlotManager.IsItemOnHands((ushort)_itemNeeded, out IInventoryItem itm);

            if (_inventoryItemUsable != null)
            {
                UnsubscribeFromCheckCondition();
                _inventoryItemUsable = null;
            }

            if (!_hasKeyOnHands || itm == null)
            {
                OnConditionChecked?.Invoke();
                return false;
            }

            if (itm is IInventoryItemUsable usable)
            {
                _inventoryItemUsable = usable;

                if (IsMopUsable(out Mop mop))
                {
                    SubscribeFromCheckCondition();
                    UpdateInteractionText();
                    return true;
                }
            }
            OnConditionChecked?.Invoke();
            return false;
        }

        protected override void InventoryHandlerData_OnSelectedItemChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return;
            CheckConditions();
        }

        protected override void InventoryHandlerData_OnItemDropped(IInventoryItem itm)
        {
            if (!IsItemRaycasted) return;
            CheckConditions();
        }

        private bool IsMopUsable(out Mop mop)
        {
            mop = null;
            if (HasKeyOnHands)
            {
                mop = _inventoryItemUsable as Mop;
                if (mop == null) return false;
                if (!mop.IsWet)
                    return false;
                else
                    return true;
            }
            return false;
        }

        private void OnValidate()
        {
            if(_targetDecal == null)
                _targetDecal = GetComponent<DecalProjector>();
        }
    }

}
