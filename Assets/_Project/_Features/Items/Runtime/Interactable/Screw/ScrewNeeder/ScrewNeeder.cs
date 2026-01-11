using LightHouse.Core.Inventory;
using LightHouse.Core.Player.Inventory;
using LightHouse.Features.Items.Inventory;
using UnityEngine;

namespace LightHouse.Features.Items.Interactable.Screw
{
    public class ScrewNeeder : IDUseItemTracker
    {
        [Header("--- SCREW NEEDER ---")]
        [SerializeField] private ItemIDEnum _screwItemID = ItemIDEnum.Screw;
        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            if (SlotManager.GetFirstItemInSlot((ushort)_screwItemID, out IInventoryItem item))
            {
                this.gameObject.SetActive(false);
                this._detectionCollider.enabled = false;
                item.InvokeForceDropItemFromInventory(this.transform.position, 0, true);
                Debug.Log(this.transform.position);
            }
        }
    }

}
