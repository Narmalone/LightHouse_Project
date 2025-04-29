using System;
using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Items.Inventory
{
    public abstract class InventoryItemBase : MonoBehaviour, IInventoryItem
    {
        #region FIELDS & PROPERTIES
        [Header(" --- BASE FIELDS --- ")]
        [SerializeField] protected string _name;
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

        public Sprite ItemSprite => _inventorySprite;
        [field: SerializeField] public ushort GlobalItemID { get; set; }
        [field: SerializeField] public ushort ItemSpecificID { get; set; }
        public bool IsItemInInventory { get; set; }
        public bool IsItemOnHands { get; set; }

        public bool IsItemRaycasted { get; set; }

        public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

        protected bool _textsHasBeenInitialized = false;
#pragma warning disable
        public event Action<string> OnNameUpdated;

        #endregion

        #region GET METHODS
        public virtual string GetName()
        {
            return _name;
        }
        public virtual GameObject GetGameObject() => this.gameObject;
        public virtual Collider GetCollider() => _detectionCollider;
        public virtual Rigidbody GetRigidBody() => _rb;

        public virtual string GetPickupName() => _currentPickupText;
        #endregion

        #region MONO CALLBACKS

        protected virtual void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
            InputManager.OnInitialized += InputManager_OnInitialized;
        }

        private void Start()
        {
            //means that the item has been instantiated
            if (!_textsHasBeenInitialized)
            {
                UpdateNameText();
                UpdatePickupText();
                _textsHasBeenInitialized = true;
            }
        }

        protected virtual void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
            InputManager.OnInitialized -= InputManager_OnInitialized;
        }
        #endregion

        #region INPUT MANAGER CALLBACKS

        protected virtual void InputManager_OnInitialized()
        {
            UpdateNameText();
            UpdatePickupText();
            _textsHasBeenInitialized = true;
        }
        #endregion

        #region INVENTORY CALLBACKS
        public virtual void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics)
            => ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, pos, force, enablePhysics);
        #endregion

        #region LOCALIZATION

        protected virtual void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            UpdateNameText();
            UpdatePickupText();
        }

        public async virtual void UpdateNameText()
        {
            LocalizedString targetString = _itemNameString;
            AsyncOperationHandle<string> actionTextOp = targetString.GetLocalizedStringAsync();
            await actionTextOp.Task;
            _name = actionTextOp.Result;
            targetString.RefreshString();
            OnNameUpdated?.Invoke(_name);
        }

        protected async virtual void UpdatePickupText()
        {
            string input = InputManager.Pickup_Bind_Name;
            _currentPickupText = await InteractionTextBuilder.Build(
                _pickupText,
                input,
                _pressToAction
            );
        }
        #endregion
    }

}
