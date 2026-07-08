using System;
using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Features.Items.Inventory
{
    public abstract class InventoryItemBase : MonoBehaviour, IInventoryItem
    {
        #region FIELDS & PROPERTIES
        [Header(" --- BASE FIELDS --- ")]
        [SerializeField] protected string _name;
        public bool EnableAutomaticSetName = true;
        [SerializeField] protected Sprite _inventorySprite;
        [SerializeField] protected Collider _detectionCollider;
        [SerializeField] protected Rigidbody _rb;
        [field: SerializeField] public Vector3 InventoryLocalPositionOffset { get; set; }
        [field: SerializeField] public Vector3 InventoryEulerAnglesForLocalRotation { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        [Header(" --- LOCALIZATION --- ")]
        public LocalizedStringDatabase_InventoryTexts _inventoryTextsDB;
        public LocalizedStringDatabase_InteractionTexts _interactionTextsDB;
        [SerializeField] protected LocalizedString _itemNameString;
        protected LocalizedString _pressToAction => _interactionTextsDB.Press_To_Action;
        protected LocalizedString _pickupText => _interactionTextsDB.Pickup;
        protected string _currentPickupText;
        public bool EnableAutomaticSetPickupText = true;

        public Sprite ItemSprite => _inventorySprite;
        [field: SerializeField] public ushort GlobalItemID { get; set; }
        [field: SerializeField] public ushort ItemSpecificID { get; set; }
        public bool IsItemInInventory { get; set; }
        public bool IsItemOnHands { get; set; }

        public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBePickedUp { get; set; } = true;

        public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

#pragma warning disable
        public event Action<string> OnNameUpdated;
#pragma warning disable
        public event Action<string> OnPickupTextUpdated;

        #endregion

        #region GET METHODS
        public virtual string GetName() => _name;
        public virtual GameObject GetGameObject() => this.gameObject;
        public virtual Collider GetCollider() => _detectionCollider;
        public virtual Rigidbody GetRigidBody() => _rb;

        public virtual string GetPickupName() => _currentPickupText;
        #endregion

        #region MONO CALLBACKS

        protected virtual void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
        }

        protected virtual void Start()
        {
            if (EnableAutomaticSetName)
            {
                SetNameTextToDefault();
            }
            if (EnableAutomaticSetPickupText)
            {
                SetPickupTextToDefault();
            }
        }

        protected virtual void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
        }
        #endregion

        #region INVENTORY CALLBACKS
        public virtual void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics)
            => ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, pos, force, enablePhysics);

        public virtual void InvokeOnPickupTextUpdated()
            => OnPickupTextUpdated?.Invoke(_currentPickupText);
        #endregion

        #region LOCALIZATION

        protected virtual void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            SetNameTextToDefault();
            SetPickupTextToDefault();
        }

        public async virtual void SetNameTextToDefault()
        {
            LocalizedString targetString = _itemNameString;
            AsyncOperationHandle<string> actionTextOp = targetString.GetLocalizedStringAsync();
            await actionTextOp.Task;
            _name = actionTextOp.Result;
            targetString.RefreshString();
            OnNameUpdated?.Invoke(_name);
        }

        public async virtual void SetPickupTextToDefault()
        {
            string input = InputManager.Pickup_Bind_Name;
            _currentPickupText = await InteractionTextBuilder.Build(
                _pickupText,
                input,
                _pressToAction
            );
        }

        public async virtual void SetPickupTextToCustom(LocalizedString customPickupText)
        {
            string input = InputManager.Pickup_Bind_Name;
            var op = customPickupText.GetLocalizedStringAsync();
            await op.Task;
            _currentPickupText = op.Result;
            InvokeOnPickupTextUpdated();
        }

        public async virtual void SetPickupTextToCustom(string customPickupText)
        {
            _currentPickupText = customPickupText;
            InvokeOnPickupTextUpdated();
        }
        #endregion

        public void SetUnpickable(bool useCallback = true)
        {
            CanBePickedUp = false;
            _currentPickupText = "Cannot pickup the item for now.";
            if(useCallback)
                InvokeOnPickupTextUpdated();
        }

        public void SetPickable(bool useCallback = true)
        {
            CanBePickedUp = true;
            SetPickupTextToDefault();
            if (useCallback)
                InvokeOnPickupTextUpdated();
        }
    }

}
