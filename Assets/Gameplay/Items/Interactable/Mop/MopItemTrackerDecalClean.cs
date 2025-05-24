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

            //if we do have the key but not
            if (HasKeyOnHands)
            {
                Mop currentHoldedMop = _inventoryItemUsable as Mop;
                if (!currentHoldedMop.IsWet)
                {
                    //If we have the mop on our hands and she's not wet we force to not subscribe to it
                    
                    return;
                }
                else
                {
                    
                }
            }
        }

        protected override void InventoryHandlerData_OnSelectedItemChanged(IInventoryItem obj)
        {
            base.InventoryHandlerData_OnSelectedItemChanged(obj);
        }

        protected override void InventoryHandlerData_OnItemDropped(IInventoryItem itm)
        {
            base.InventoryHandlerData_OnItemDropped(itm);
        }

        private void OnValidate()
        {
            if(_targetDecal == null)
                _targetDecal = GetComponent<DecalProjector>();
        }
    }

}
