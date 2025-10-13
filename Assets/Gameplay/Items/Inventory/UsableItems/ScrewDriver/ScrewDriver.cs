using System;
using System.Threading.Tasks;
using LightHouse.CustomAttributes;
using LightHouse.Handlers;
using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Items.Detection;
using LightHouse.Items.Interactable;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Items.Inventory
{
    public class ScrewDriver : InventoryItemBase, IInventoryItemUsable
    {
        [TagSelector, SerializeField] private string _screwTag;
        [SerializeField] private LayerMask _interactableItemsMasks;
        [SerializeField] private float _screwRadius = 1.0f;
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private ItemIDEnum _itemToAddInInventory = ItemIDEnum.Screw;
        
        [SerializeField] private GameObject _screwPrefab;
        
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
        public event Action<string> UseTextSlotChanged;
        public bool CanBeUsedFromInventory { get; set; } = false;

        [field: SerializeField] public float UseHoldTime { get; set; } = 0.5f;
        
        private IInventoryItemUsable _inventoryItemUsableImplementation;
        

        public string UseTextSlot()
        {
            return "Use Screwdriver";
        }

        public void UseFromInventory()
        {
            EmitRadius();
            OnItemUsed?.Invoke();
        }

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }
        
        public void EmitRadius()
        {
            if (ItemsDetectionSystem.CurrentHitedObjectPosition == null) return;
            Collider[] hitColliders = Physics.OverlapSphere(ItemsDetectionSystem.CurrentHitedObjectPosition.position, _screwRadius, _interactableItemsMasks, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag(_screwTag))
                {
                    ScrewInteractable screwComp = hit.GetComponent<ScrewInteractable>();
                    if (screwComp != null && screwComp.gameObject != this.gameObject)
                    {
                        if (!SlotManager.FindFirstEmptySlot(out ItemSlot slot) && !SlotManager.TryFindStackableSlot(this.GlobalItemID,out ItemSlot stackSlot))
                        {
                            if (_screwPrefab != null)
                            {
                                GameObject screwInstance = Instantiate(_screwPrefab, ItemsDetectionSystem.CurrentHitedObjectPosition.position, Quaternion.identity);
                            }
                        }
                        GameObject prefab = Instantiate(_itemDatabase.GetPrefab((ushort)_itemToAddInInventory));
                        IInventoryItem inventoryItem = prefab.GetComponent<IInventoryItem>();
                        PlayerHandlerData.MainPlayer.Inventory.AddItemToInventory(SlotManager.CurrentSlotIndex, inventoryItem);
                        
                        Destroy(screwComp.gameObject);
                    }
                }                
            }
        }
    }
}

