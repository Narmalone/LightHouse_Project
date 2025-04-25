using LightHouse.Interactions;
using UnityEngine;
using LightHouse.Inventory;
using LightHouse.Inputs;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using LightHouse.Localization;

namespace LightHouse.Items.Interactable
{
    public abstract class IDItemTracker : InteractableItemBase, IItemCallback
    {
        #region FIELDS & PROPERTIES
        [Header(" --- ID ITEM TRACKER --- ")]
        [SerializeField] protected ItemIDEnum _itemNeeded;

        [Header(" --- LOCALIZATION --- ")]
        [SerializeField] protected LocalizedStringDatabase_InteractionTexts _interactionTextsDB;
        [SerializeField] protected LocalizedString _missingItemInInventoryTxt => _interactionTextsDB.Need_Item;
        [SerializeField] protected LocalizedString _needItemOnHandsTxt => _interactionTextsDB.Need_Item_In_Hands;

        [SerializeField] protected string _currentText;

        [Header("Read Only / Debug purposes")]
        [SerializeField] protected bool _hasKeyInInventory = false;
        [SerializeField] protected bool _hasKeyOnHands = false;
        #endregion
        
        #region MONO'S CALLBACK
        protected virtual void Awake()
        {
            InventoryHandlerData.OnSelectedItemChanged += InventoryHandlerData_OnSelectedItemChanged;
            InventoryHandlerData.OnItemDropped += InventoryHandlerData_OnItemDropped;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        }

        protected virtual void OnDestroy()
        {
            InventoryHandlerData.OnSelectedItemChanged -= InventoryHandlerData_OnSelectedItemChanged;
            InventoryHandlerData.OnItemDropped -= InventoryHandlerData_OnItemDropped;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        #endregion

        #region LOCALIZATION
        protected virtual void OnLocaleChanged(Locale locale)
            => UpdateInteractionText();

        protected virtual void UpdateInteractionText()
        {
            LocalizedString targetString = null;
            string input = InputManager.InteractInInventory_Bind_Name;

            if (!_hasKeyInInventory)
            {
                targetString = _missingItemInInventoryTxt;
                input = _itemNeeded.ToString();
            }
            else if (_hasKeyInInventory && !_hasKeyOnHands)
            {
                targetString = _needItemOnHandsTxt;
                input = _itemNeeded.ToString();
            }

            targetString.Arguments = new object[] { new { key = input } };
            targetString.StringChanged += result =>
            {
                _currentText = result;
                if (IsItemRaycasted)
                    InvokeInteractionDescriptionUpdated();
            };
            targetString.RefreshString();
        }

        #endregion

        #region IInteractable
        public override string GetInteractionName()
            =>  _currentText;

        public override void Interact() => InvokeObjectInteracted();

        #endregion

        #region INVENTORY CALLBACKS
        protected virtual void InventoryHandlerData_OnItemDropped(IInventoryItem itm)
        {
            if (!IsItemRaycasted) return;
            CheckConditions();
            UpdateInteractionText();
        }

        protected virtual void InventoryHandlerData_OnSelectedItemChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return;
            CheckConditions();
            UpdateInteractionText();
        }
        #endregion

        #region IItemCallback
        public virtual void OnRaycastStart()
        {
            CheckConditions();
            UpdateInteractionText();
        }

        public virtual void OnRaycastEnd()
        {
            _hasKeyInInventory = false;
            _hasKeyOnHands = false;
            UpdateInteractionText();
        }
        #endregion

        #region Check & Other Abstract functions
        protected abstract void CheckConditions();
        #endregion
    }

}
