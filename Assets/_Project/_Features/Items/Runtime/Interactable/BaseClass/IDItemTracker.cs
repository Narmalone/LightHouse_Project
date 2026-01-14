using LightHouse.Core.Inputs;
using LightHouse.Features.Interactions;
using LightHouse.Core.Localization;
using LightHouse.Features.Items.Inventory;
using LightHouse.Core.Inventory;
using LightHouse.Core.Player.Inventory;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Features.Items.Interactable
{
    public abstract class IDItemTracker : InteractableItemBase, IItemCallback
    {
        #region FIELDS & PROPERTIES
        [Header(" --- ID ITEM TRACKER --- ")]
        [SerializeField] protected ItemIDEnum _itemNeeded;

        protected LocalizedString _missingItemInInventoryTxt => _interactionTextsDB.Need_Item;
        protected LocalizedString _needItemOnHandsTxt => _interactionTextsDB.Need_Item_In_Hands;
        protected LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;
        [SerializeField] protected LocalizedString _itemNeededName;

        [SerializeField] protected string _currentItemNeededName;

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
                InteractionText = result;
                if (IsItemRaycasted)
                    InvokeInteractionDescriptionUpdated();
            };
            str.RefreshString();
        }

        private async void ResolveWithWrapper(LocalizedString wrapper, LocalizedString actionText, string input)
        {
            var finalText = await InteractionTextBuilder.Build(
                actionText: actionText,
                bindKey: input,
                prefixSentence: wrapper
            );

            InteractionText = finalText;
            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region IInteractable

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
        protected abstract bool CheckConditions();
        public void ForceCheckConditions()
        {
            CheckConditions();
        }
        #endregion
    }

}
