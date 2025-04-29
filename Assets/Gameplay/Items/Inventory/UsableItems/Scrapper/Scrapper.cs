using System;
using System.Collections;
using System.Collections.Generic;
using LightHouse.Handlers;
using LightHouse.Inventory;
using LightHouse.Items.Interactable;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Scrapper : InventoryItemBase, IInventoryItemUsable
    {
        [TagSelector, SerializeField] private string Tag;
        [SerializeField] private LayerMask TargetMasks;
        [SerializeField] private float _scrapRadius = 1.0f;
        public ItemDatabase ItemDatabase;
        public ItemIDEnum ItemToAddInInventory;
        public bool CanBeUsedFromInventory { get; set; } = false;

        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }

        public void UseFromInventory()
        {
            EmitRadius();
        }

        public void EmitRadius()
        {
            float radius = 3f;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, TargetMasks, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag(Tag))
                {
                    BernacleInteractable barnacleComp = hit.GetComponent<BernacleInteractable>();
                    if (barnacleComp != null && barnacleComp.gameObject != this.gameObject)
                    {
                        var obj = Instantiate(ItemDatabase.GetPrefab((ushort)ItemToAddInInventory));
                        var s = obj.GetComponent<IInventoryItem>();
                        PlayerHandlerData.MainPlayer.Inventory.AddItemToInventory(SlotManager.CurrentSlotIndex, s);
                        Destroy(barnacleComp.gameObject);
                    }
                }                
            }
        }


        public string UseTextSlot()
        {
            return "";
        }
    }

}
