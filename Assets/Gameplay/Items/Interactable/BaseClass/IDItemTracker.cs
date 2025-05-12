using LightHouse.Interactions;
using UnityEngine;
using LightHouse.Inventory;
using LightHouse.Inputs;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using LightHouse.Localization;
using System;

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
        [SerializeField] protected LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;
        [SerializeField] protected LocalizedString _itemNeededName;

        [SerializeField] protected string _currentItemNeededName;
        [SerializeField] protected string _currentInteractionText;

        [Header("Read Only / Debug purposes")]
        [SerializeField] protected bool _hasKeyInInventory = false;
        [SerializeField] protected bool _hasKeyOnHands = false;

        public bool HasKeyInInventory => _hasKeyInInventory;
        public bool HasKeyOnHands => _hasKeyOnHands;
        #endregion
        
        #region MONO'S CALLBACK
        protected override void Awake()
        {
            base.Awake();   
            InventoryHandlerData.OnSelectedItemChanged += InventoryHandlerData_OnSelectedItemChanged;
            InventoryHandlerData.OnItemDropped += InventoryHandlerData_OnItemDropped;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            _currentItemNeededName = _itemNeededName.GetLocalizedString();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InventoryHandlerData.OnSelectedItemChanged -= InventoryHandlerData_OnSelectedItemChanged;
            InventoryHandlerData.OnItemDropped -= InventoryHandlerData_OnItemDropped;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        #endregion

        #region LOCALIZATION
        protected virtual void OnLocaleChanged(Locale locale)
        {
            _currentItemNeededName = _itemNeededName.GetLocalizedString();
            UpdateInteractionText();
        }

        protected virtual void UpdateInteractionText()
        {
            string input = InputManager.InteractInInventory_Bind_Name;

            if (!_hasKeyInInventory)
            {
                var rawString = _missingItemInInventoryTxt;
                rawString.Arguments = new object[] { new { key = _currentItemNeededName } };
                ResolveAndSetText(rawString);
            }
            else if (_hasKeyInInventory && !_hasKeyOnHands)
            {
                var rawString = _needItemOnHandsTxt;
                rawString.Arguments = new object[] { new { key = _currentItemNeededName } };
                ResolveAndSetText(rawString);
            }
            else
            {
                // On veut ici afficher : "Maintiens [E] pour utiliser"
                var action = _interactionTextsDB.Use; // par exemple
                ResolveWithWrapper(_holdToAction, action, input);
            }
        }


        private void ResolveAndSetText(LocalizedString str)
        {
            str.StringChanged += result =>
            {
                _currentInteractionText = result;
                if (IsItemRaycasted)
                    InvokeInteractionDescriptionUpdated();
            };
            str.RefreshString();
        }

        private async void ResolveWithWrapper(LocalizedString wrapper, LocalizedString actionText, string input)
        {
            var finalText = await InteractionTextBuilder.Build(
                baseText: actionText,
                bindKey: input,
                wrapperTemplate: wrapper
            );

            _currentInteractionText = finalText;
            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region IInteractable
        public override string GetInteractionName()
            =>  _currentInteractionText;

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
        public void ForceCheckConditions()
        {
            CheckConditions();
        }
        #endregion
    }

}
