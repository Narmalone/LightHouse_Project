using LightHouse.Handlers;
using LightHouse.Inventory;
using UnityEngine;

namespace LightHouse.Items.Interactable
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
