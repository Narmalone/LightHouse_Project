using UnityEngine;
using LightHouse.Inputs;
using System;
using LightHouse.Inventory;
using UnityEngine.Localization;
using LightHouse.Localization;
using System.Threading.Tasks;

namespace LightHouse.Items.Inventory
{
    //You should use this class to make inventory with key for other objects
    public class Key : InventoryItemBase, IInventoryItemUsable
    {
        public LocalizedString _use => _interactionTextsDB.Use;
        public LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;

        protected string _currentUseText;

        #region SERIALIZED FIELDS
        [Header("KEY Fields")]
        [SerializeField] private bool _destroyOnUsed;
        [field: SerializeField] public bool CanBeUsedFromInventory { get; set; }
        #endregion

        #region IInventory Fields
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;
        #endregion

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

        protected async virtual Task UpdateUseKey()
        {
            _currentUseText = await InteractionTextBuilder.Build(
                _use,
                InputManager.InteractInInventory_Bind_Name,
                _holdToAction
            );
        }

        #region IInventoryItem Functions
        public virtual void UseFromInventory()
        {
            OnItemUsed?.Invoke();
            InvokeForceDropItemFromInventory(transform.position, 0.0f, false);
            if (_destroyOnUsed)
                Destroy(this.gameObject);
        }
        public virtual string UseTextSlot() => _currentUseText;
        public void InvokeOnCanBeUsedFromInventoryChanged() => CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);

        #endregion
    }
}
