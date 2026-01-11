using LightHouse.Core.Attributes;
using LightHouse.Core.Inputs;
using LightHouse.Core.Inventory;
using LightHouse.Core.Localization;
using LightHouse.Core.Player;
using LightHouse.Core.Player.Inventory;
using LightHouse.Features.Items.Detection;
using LightHouse.Features.Items.Interactable;
using LightHouse.Features.Items.Inventory.Databases;

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Features.Items.Inventory.Scrapper
{
    public class Scrapper : InventoryItemBase, IInventoryItemUsable
    {
        [TagSelector, SerializeField] private string _barnacleTag;
        [SerializeField] private LayerMask _interactableItemsMasks;
        [SerializeField] private float _scrapRadius = 1.0f;
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private ItemIDEnum _itemToAddInInventory = ItemIDEnum.BernacleInventory;
        public LocalizedString _use => _interactionTextsDB.Use;
        public LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;
        public bool CanBeUsedFromInventory { get; set; } = false;
        [field: SerializeField] public float UseHoldTime { get; set; } = 0.5f;
        
        protected string _currentUseText;
        private IInventoryItemUsable _inventoryItemUsableImplementation;

        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
        public event Action<string> UseTextSlotChanged;

        protected async override void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            base.LocalizationSettings_SelectedLocaleChanged(obj);
            await UpdateUseKey();
        }

        protected async override void InputManager_OnInitialized()
        {
            base.InputManager_OnInitialized();
            await UpdateUseKey();
        }

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }

        public void UseFromInventory()
        {
            EmitRadius();
            OnItemUsed?.Invoke();
        }

        public void EmitRadius()
        {
            if (ItemsDetectionSystem.CurrentHitedObjectPosition == null) return;
            Collider[] hitColliders = Physics.OverlapSphere(ItemsDetectionSystem.CurrentHitedObjectPosition.position, _scrapRadius, _interactableItemsMasks, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag(_barnacleTag))
                {
                    BernacleInteractable barnacleComp = hit.GetComponent<BernacleInteractable>();
                    if (barnacleComp != null && barnacleComp.gameObject != this.gameObject)
                    {
                        GameObject prefab = Instantiate(_itemDatabase.GetPrefab((ushort)_itemToAddInInventory));
                        IInventoryItem inventoryItem = prefab.GetComponent<IInventoryItem>();
                        PlayerHandlerData.MainPlayer.Inventory.AddItemToInventory(SlotManager.CurrentSlotIndex, inventoryItem);
                        Destroy(barnacleComp.gameObject);
                    }
                }                
            }
        }

        protected async virtual Task UpdateUseKey()
        {
            _currentUseText = await InteractionTextBuilder.Build(
                _use,
                InputManager.InteractInInventory_Bind_Name,
                _holdToAction
            );
        }
        public virtual string UseTextSlot() => _currentUseText;

    }

}
