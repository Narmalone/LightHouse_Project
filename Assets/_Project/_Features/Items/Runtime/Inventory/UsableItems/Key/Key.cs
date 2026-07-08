using LightHouse.Core.Inputs;
using UnityEngine.Localization;

using UnityEngine;
using System;
using System.Threading.Tasks;
using LightHouse.Core.Localization;

namespace LightHouse.Features.Items.Inventory
{
    //You should use this class to make inventory with key for other objects
    public class Key : InventoryItemBase, IInventoryItemUsable
    {
        #region LOCALIZATION FIELDS
        public LocalizedString _use => _interactionTextsDB.Use;
        public LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;

        protected string _currentUseText;

        #endregion

        #region SERIALIZED FIELDS
        [Header(" --- KEY FIELDS --- ")]
        [SerializeField] private bool _destroyOnUsed = true;
        [SerializeField] private bool _dropItemOnUsed = true;
        [field: SerializeField] public bool CanBeUsedFromInventory { get; set; }

        [field: SerializeField] public float UseHoldTime { get; set; } = 0.5f;
        
        #endregion

        #region IInventory Fields
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
        public event Action<string> UseTextSlotChanged;
        #endregion

        #region LOCALIZATION
        protected async override void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            base.LocalizationSettings_SelectedLocaleChanged(obj);
            await UpdateUseKey();
        }

        private async void Start()
        {
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
        #endregion

        #region IInventoryItem Functions
        public virtual void UseFromInventory()
        {
            OnItemUsed?.Invoke();
            if (_dropItemOnUsed)
            {
                InvokeForceDropItemFromInventory(transform.position, 0.0f, false);
            }
            if (_destroyOnUsed)
            {
                InvokeForceDropItemFromInventory(transform.position, 0.0f, false);
                Destroy(this.gameObject);
            }
        }
        public virtual string UseTextSlot() => _currentUseText;
        public void InvokeOnCanBeUsedFromInventoryChanged() => CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);

        #endregion
    }
}
