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
            if(SlotManager.FindItemInSlot((ushort)_screwItemID,0, out byte slotId))
            {
                base.Usable_OnItemUsed();
                this.gameObject.SetActive(false);
                this._detectionCollider.enabled = false;
                if (SlotManager.GetFirstItemInSlot((ushort)_screwItemID, out IInventoryItem item))
                {
                    item.InvokeForceDropItemFromInventory(this.transform.position, 0, true);
                    Debug.Log(this.transform.position);
                }
                
            }
            else
            {
                Debug.Log("Cannot use ScrewDriver, no Screw in inventory.");
            }
            
        }
    }

}
