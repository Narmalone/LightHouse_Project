using System;
using System.Collections;
using System.Collections.Generic;
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
                    //Debug.Log($"Detected barnacle by tag: {hit.name}");
                    Bernacle barnacleComp = hit.GetComponent<Bernacle>();
                    if (barnacleComp != null && barnacleComp.gameObject != this.gameObject)
                    {
                        //Debug.Log($"Found nearby barnacle: {barnacleComp.name}");
                        //barnacleComp.gameObject.SetActive(false);
                        if (barnacleComp.IsItemRaycasted)
                        {

                        }
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
