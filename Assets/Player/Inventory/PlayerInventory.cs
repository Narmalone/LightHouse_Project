using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using LightHouse.Interactions;
using LightHouse.Inputs;
using LightHouse.Utilities;
using LightHouse.Items.Samples;
using LightHouse.Locators;

namespace LightHouse.Inventory
{
    [DefaultExecutionOrder(-1)]
    public class PlayerInventory : MonoBehaviour
    {
        #region SERILIAZED FIELDS
        [Header("Inventory Settings")]
        [SerializeField] private byte _inventoryCapacity = 4;
        [SerializeField] private int _currentSlotIndex = -1;
        [SerializeField] private byte _currentSlotTakenInventory = 0;
        [SerializeField] private Vector3 _inventoryOffset = new Vector3(0.5f, -0.5f, 0.8f); // (X = décalage horizontal, Y = vertical, Z = profondeur)

        [Header("Inventory References")]
        [SerializeField] private Transform _inventoryParent = null;
        [SerializeField] private Transform _inventoryTarget = null;

        [Header("Raycast")]
        [SerializeField] private bool _enableInventoryRaycast = true;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _raycastDistance = 3.0f;
        [SerializeField] private LayerMask _targetMask = 1 << -1;
        [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Drop Settings")]
        [SerializeField] private float _maxDropPower = 10f;
        [SerializeField] private float _securityOverlapSphereRadius = 0.3f;
        [SerializeField] private LayerMask _securityObstacleMasks = 1 << 0;

        [Header("UI")]
        [SerializeField] private CanvasInventory _inventoryCanvas;
        [SerializeField] private CanvasInteraction _interactionCanvas;

        [Header("Item Keys")]
        public List<KeyType> _keyTypesInInventory = new List<KeyType>();

        #endregion

        #region PRIVATE FIELDS
        [Header("DEBUG")]
        //Slots Infos
        private List<ItemSlot> _itemSlots = new();
        [SerializeField] private Collider[] _dropOverlappingCollider = new Collider[0];

        private ItemSlot _lastSelectedSlot = null;
        [SerializeField] private ItemSlot _currentSelectedSlot = null;

        public ItemSlot CurrentSelectedSlot => _currentSelectedSlot;
        public IInventoryItem CurrentSelectedItem => _currentSelectedSlot.InventoryItem;

        //Raycast Info
        private bool _isRaycastingSomething = false;
        private GameObject _lastObjectSeen;
        private IInventoryItem _raycastedInventoryItem;
        private IInventoryItemCallback _raycastedInventoryItemCallback;
        private IInventoryItemUsable _raycastedInventoryItemUsable;

        public Transform InventoryParent => _inventoryParent;
        public Transform InventoryTarget => _inventoryTarget;

        public IInventoryItem CurrentRaycastedInventoryItem;

        //Drops Info
        private float _dropPower = 0f;
        private bool _isChargingDrop = false;

        public static event Action<IInventoryItem> OnHandsItemSelectedChanged;
        public static event Action<IInventoryItem> OnItemAdded;
        public static event Action<IInventoryItem> OnItemDropped;
        public static event Action<IInventoryItem> OnItemStackDropped;

        #endregion

        #region INPUTS REGISTERING
        public void RegisterInput()
        {
            InputManager.Scroll.performed += Scroll_performed;
        }

        public void UnregisterInput()
        {
            InputManager.Scroll.performed -= Scroll_performed;
        }
        #endregion

        #region MONO'S CALLBACK
        private void Awake()
        {
            _itemSlots = _inventoryCanvas.GenerateItemSlot(_inventoryCapacity).ToList();
            _interactionCanvas.HideItemPickup();
            Locator<PlayerInventory>.Register(this);
        }

        private void Update()
        {
            if (_enableInventoryRaycast)
            {
                Ray cameraRay = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
                _isRaycastingSomething = RaycastUtility.TryRaycast(cameraRay, _raycastDistance, _targetMask, _queryTriggerInteraction, out RaycastHit hit);
                if (_isRaycastingSomething)
                {
                    HandleNewObject(hit.collider.gameObject);
                    HandlePickupInput();
                }
                else
                    ResetSeenObject();

                HandleInteractItemInInventory();
                HandleDropInput();
                Debug.DrawRay(_playerCamera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 start = _inventoryTarget.position;
            float sphereRadius = _securityOverlapSphereRadius;
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            _dropOverlappingCollider = colliders;
            Gizmos.color = _dropOverlappingCollider.Length > 0 ? Color.red : Color.green;
            Gizmos.DrawSphere(_inventoryTarget.position, 0.3f);
        }

        private void OnDestroy() => Locator<PlayerInventory>.Clear();

        #endregion

        #region Handle Inventory Position and Rotation
        /// <summary>
        /// Called from <cf Player> it handle the rotation of the inventory target, 
        /// which is the child of Character Controller that rotates and move </cf>
        /// </summary>
        public void HandleInventoryRotation()
        {
            _inventoryTarget.rotation = _playerCamera.transform.rotation;
            _inventoryParent.rotation = _inventoryTarget.rotation;
        }

        /// <summary>
        /// Called from <cf Player> it handle the position of the inventory target in the local space of the camera
        /// using TransformPoint() and a generated offset to make custom things </cf>
        /// Later we could also add some ItemsData to make custom offset for each objects
        /// </summary>
        public void HandleInventoryPosition()
        {
            // Appliquer l'offset dans l'espace local de la caméra
            Vector3 adjustedPosition = _playerCamera.transform.TransformPoint(_inventoryOffset);

            // Appliquer la nouvelle position sans affecter la rotation
            _inventoryTarget.position = adjustedPosition;
            _inventoryParent.position = _inventoryTarget.position;
        }
        #endregion

        #region Inputs Callbacks
        private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int scrollDirection = Mathf.RoundToInt(obj.ReadValue<Vector2>().y);
            int inversedScroll = -scrollDirection; //invert scroll direction (scroll up to left, scroll down to right)

            if (inversedScroll == 0) return; 

            _currentSlotIndex += inversedScroll;
            // Cyclic scrolling, -1 is for nothing selected
            if (_currentSlotIndex < -1)
                _currentSlotIndex = _inventoryCapacity - 1;
            else if (_currentSlotIndex >= _inventoryCapacity)
                _currentSlotIndex = -1;

            ChangeSelectedSlot(_currentSlotIndex);
        }

        private void HandleInteractItemInInventory()
        {
            if (!InputManager.InteractInInventory.WasPressedThisFrame() || _currentSelectedSlot == null || _currentSelectedSlot.InventoryItemUsable == null)
                return;
            _currentSelectedSlot.InventoryItemUsable.UseFromInventory();
        }

        private void HandlePickupInput()
        {
            if (_raycastedInventoryItem == null || !InputManager.PickUp.WasPerformedThisFrame()) return;
            AddItem(_raycastedInventoryItem, _raycastedInventoryItemCallback, _raycastedInventoryItemUsable);
        }

        private void HandleDropInput()
        {
            if (_currentSlotIndex < 0 || _currentSelectedSlot.InventoryItem == null) return;

            if (InputManager.Drop.IsPressed())
            {
                _isChargingDrop = true;
                _dropPower = Mathf.Clamp(_dropPower + Time.deltaTime * _maxDropPower, 0, _maxDropPower);
            }
            else if (_isChargingDrop && InputManager.Drop.WasReleasedThisFrame())
            {
                DropCurrentSelectedItem(_inventoryTarget.position, _dropPower);
                _dropPower = 0f;
                _isChargingDrop = false;
            }
            else if (InputManager.Drop.WasPerformedThisFrame())
            {
                DropCurrentSelectedItem(_inventoryTarget.position, 1f);
            }
        }

        #endregion

        #region RAYCAST HANDLE

        /// <summary>
        /// Called if raycast hit something and store datas into variable
        /// </summary>
        /// <param name="hitObject"> the hitInfo.gameobject result </param>
        private void HandleNewObject(GameObject hitObject)
        {
            if (hitObject != _lastObjectSeen)
            {
                _lastObjectSeen = hitObject;
                _raycastedInventoryItem = hitObject.GetComponent<IInventoryItem>();
                _raycastedInventoryItemCallback = hitObject.GetComponent<IInventoryItemCallback>();
                _raycastedInventoryItemUsable = hitObject.GetComponent<IInventoryItemUsable>();
                
                if (_raycastedInventoryItem != null)
                    ShowPickupItemInteractionUI(_raycastedInventoryItem);
                //in case we are looking an item and looking an other item whithout seeing nothing (without the ResetSeenObjectMethod())
                else
                {
                    if (!_interactionCanvas.IsHided)
                        SetEnablePickupInteractionTextUI(false);
                }
            }
        }

        /// <summary>
        /// Called if we don't raycast anything at all
        /// </summary>
        private void ResetSeenObject()
        {
            if(_lastObjectSeen != null)
                _lastObjectSeen = null;

            if (_raycastedInventoryItem != null)
                _raycastedInventoryItem = null;

            if (_raycastedInventoryItemCallback != null)
                _raycastedInventoryItemCallback = null;

            if (_raycastedInventoryItemUsable != null)
                _raycastedInventoryItemUsable = null;

            if (_interactionCanvas.ItemPickup_TPM.isActiveAndEnabled)
                SetEnablePickupInteractionTextUI(false);
        }

        #endregion

        #region Change Current Selected Slot
        /// <summary>
        /// Change the current selected slot based on somme index
        /// <cf _currentSlotIndex> </cf>
        /// </summary>
        public void ChangeSelectedSlot(int index)
        {
            if (_lastSelectedSlot != null)
            {
                if (_lastSelectedSlot.ItemName_TMP.isActiveAndEnabled)
                    SetEnableItemNameTextUI(_lastSelectedSlot, false);

                if (_lastSelectedSlot.InventoryItem != null)
                {
                    _lastSelectedSlot.InventoryItem.GetGameObject().SetActive(false);
                    if (_lastSelectedSlot.InventoryItem.IsItemOnHands) _lastSelectedSlot.InventoryItem.IsItemOnHands = false;
                }
                SetEnableItemUseKeyUI(_lastSelectedSlot, false);
            }

            //Handle case where we have no one slot selected
            if (_currentSlotIndex < 0 || _currentSlotIndex >= _inventoryCapacity)
            {
                _currentSelectedSlot = null;
                OnHandsItemSelectedChanged?.Invoke(null);
                return;
            }

            _currentSelectedSlot = _itemSlots[_currentSlotIndex];
            _lastSelectedSlot = _currentSelectedSlot;

            IInventoryItem currentSelectedItem = _currentSelectedSlot.InventoryItem;
            if (currentSelectedItem != null)
            {
                SetItemNameTextUI(_currentSelectedSlot, currentSelectedItem.GetName());
                IsInventoryUsableUI(_currentSelectedSlot, _currentSelectedSlot.InventoryItemUsable);
                currentSelectedItem.GetGameObject().SetActive(true);

                if (!_currentSelectedSlot.ItemName_TMP.isActiveAndEnabled)
                    SetEnableItemNameTextUI(_currentSelectedSlot, true);

                currentSelectedItem.IsItemOnHands = true;
            }
            OnHandsItemSelectedChanged?.Invoke(currentSelectedItem);
        }
        #endregion

        #region Main Inventory Items and Slots Management

        public bool IsCurrentSlotAlreadyTaken()
        {
            return _currentSlotIndex < 0 || _currentSlotIndex >= _inventoryCapacity || _currentSelectedSlot.InventoryItem != null;
        }

        private void SetItemToSlot(ItemSlot slot, IInventoryItem item, IInventoryItemCallback callback, IInventoryItemUsable usable)
        {
            slot.SetInventoryItem(item);
            slot.SetSpriteItem(item);
            slot.SetItemCallback(callback);
            slot.SetItemUsable(usable);
            SetItemNameTextUI(slot, item.GetName());
        }

        /// <summary>
        /// Called when the player press the pickup key while looking an item
        /// </summary>
        /// <param name="item"> the item we are trying to add in the inventory </param>
        /// <returns> if the items sucessfully been added to inventory</returns>
        public bool AddItem(IInventoryItem item, IInventoryItemCallback itemCallback, IInventoryItemUsable itemUsable)
        {
            if (TryStackItem(_raycastedInventoryItem)) return true;

            if (_currentSlotTakenInventory >= _inventoryCapacity)
                return false;

            if (item is Key)
            {
                Key key = (Key)item;
                _keyTypesInInventory.Add(key.KeyType);
            }

            SetEnablePickupInteractionTextUI(false);
            GameObject obj = PrepareItemForInventory(item);

            if (IsCurrentSlotAlreadyTaken())
            {
                ItemSlot targetSlot = GetEmptyPlaceInInventory();

                //In case this item is still not stacked
                if (item is IInventoryStackable)
                {
                    if (!targetSlot.ItemStack_TMP.isActiveAndEnabled) SetEnableStackCountText(targetSlot, true);
                    UpdateStackItemCountUI(targetSlot);
                }
                SetItemToSlot(targetSlot, item, itemCallback, itemUsable);
                obj.SetActive(false);
            }
            //else we presume that the item can be store on our hands bcs it's the current selected slot
            else if (_currentSelectedSlot == _itemSlots[_currentSlotIndex])
            {
                SetItemToSlot(_currentSelectedSlot, item, itemCallback, itemUsable);
                //In case this item is still not stacked
                if (item is IInventoryStackable)
                {
                    if (!_currentSelectedSlot.ItemStack_TMP.isActiveAndEnabled) SetEnableStackCountText(_currentSelectedSlot, true);
                    UpdateStackItemCountUI(_currentSelectedSlot);
                }

                SetEnableItemNameTextUI(_currentSelectedSlot, true);
                IsInventoryUsableUI(_currentSelectedSlot, itemUsable);
                item.IsItemOnHands = true;
                OnHandsItemSelectedChanged?.Invoke(item);
            }

            if (item is IInventoryItemUsable inventoryUsableItem) inventoryUsableItem.CanBeUsedFromInventoryChanged += InventoryUsableItem_CanBeUsedFromInventoryChanged;

            _currentSlotTakenInventory++;
            itemCallback?.OnItemAddedToInventory();
            OnItemAdded?.Invoke(item);
            item.ForceRemoveItemInInventory += () => Item_ForceItemRemove(item, itemCallback);
            return true;
        }

        private GameObject PrepareItemForInventory(IInventoryItem item)
        {
            GameObject obj = item.GetGameObject();
            obj.transform.SetParent(_inventoryParent);
            obj.transform.position = _inventoryTarget.position;
            obj.transform.rotation = _inventoryTarget.rotation;

            item.GetCollider().enabled = false;
            item.GetRigidBody().isKinematic = true;
            item.IsItemInInventory = true;
            return obj;
        }

        /// <summary>
        /// Drop the item inside the current selected slot on a specific position with a hold force
        /// </summary>
        /// <param name="dropPosition"> the position where you want to drop the item </param>
        /// <param name="force"> the force added to the forward of the position if a rigibody is attached to it </param>
        public void DropCurrentSelectedItem(Vector3 dropPosition, float force, bool enableRigidbodyOnDrop = true)
        {
            if (_currentSelectedSlot.InventoryItem == null) return;
            IInventoryItem itemToDrop;
            Vector3 safeDropPos = Vector3.zero;
            if (IsCurrentSelectedItemStackable())
            {
                IInventoryStackable stackable = _currentSelectedSlot.InventoryItem as IInventoryStackable;
                itemToDrop = _currentSelectedSlot.RemoveStackedItem();
                safeDropPos = GetAdjustedDropPosition(dropPosition, 1);
                stackable.RemoveFromStack(1);
                UpdateStackItemCountUI(_currentSelectedSlot);
            }
            else
            {
                safeDropPos = GetAdjustedDropPosition(dropPosition);
                itemToDrop = _currentSelectedSlot.InventoryItem;
                RemoveItemFromInventory(_currentSelectedSlot, itemToDrop, _currentSelectedSlot.InventoryItemCallback);
            }

            GameObject obj = PrepareItemToDropFromInventory(itemToDrop, enableRigidbodyOnDrop);
            obj.transform.position = safeDropPos;
            obj.transform.rotation = Quaternion.Euler(0f, _playerCamera.transform.eulerAngles.y, 0f);
            Rigidbody rb = itemToDrop.GetRigidBody();
            if (rb != null && safeDropPos == dropPosition)
                rb.AddForce(_inventoryTarget.forward * force, ForceMode.Impulse);
        }
        private GameObject PrepareItemToDropFromInventory(IInventoryItem item, bool enablePhysicsOnDrop = true)
        {
            GameObject obj = item.GetGameObject();
            obj.transform.SetParent(null);
            obj.SetActive(true);
            if (enablePhysicsOnDrop)
            {
                item.GetCollider().enabled = true;
                item.GetRigidBody().isKinematic = false;
            }
            return obj;
        }

        private Vector3 GetAdjustedDropPosition(Vector3 intendedDropPosition, int lengthToSafePos = 0)
        {
            Vector3 start = _inventoryTarget.position;
            float sphereRadius = _securityOverlapSphereRadius; // Ajuste la taille en fonction des objets

            // 1. Vérifier si un obstacle est proche avec un OverlapSphere
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            _dropOverlappingCollider = colliders;
            if (colliders.Length > lengthToSafePos)
            {
                Debug.Log("Obstacle trop proche ! Dépose l'objet juste devant le joueur.");
                Vector3 adjustedPosition = start - _inventoryTarget.forward * 0.5f; // Reculer légèrement pour ne pas être dans l'obstacle
                Debug.DrawRay(start, -_inventoryTarget.forward * 0.5f, Color.yellow, 5.0f);
                return adjustedPosition;
            }

            return intendedDropPosition;
        }

        /// <summary>
        /// Try to see if a specific item is inside a slot, and if so we remove it
        /// </summary>
        /// <param name="item"> the item you try to remove </param>
        public GameObject RemoveItemFromInventory(ItemSlot slot, IInventoryItem item, IInventoryItemCallback itemCallback)
        {
            if (slot == null) return null;
            if (item is IInventoryStackable stack)
            {
                if (slot.ItemStack_TMP.isActiveAndEnabled)
                    SetEnableStackCountText(slot, false);
            }
            if (item is IInventoryItemUsable inventoryUsableItem) inventoryUsableItem.CanBeUsedFromInventoryChanged -= InventoryUsableItem_CanBeUsedFromInventoryChanged;

            SetEnableItemNameTextUI(slot, false);
            SetEnableItemUseKeyUI(slot, false);
            TryRemoveKeyType(item);
            if (item.IsItemInInventory) item.IsItemInInventory = false;
            if (item.IsItemOnHands) item.IsItemOnHands = false;
            itemCallback?.OnItemRemovedFromInventory();
            item.ForceRemoveItemInInventory -= () => Item_ForceItemRemove(item, itemCallback);
            _currentSlotTakenInventory--;
            OnItemDropped?.Invoke(item);
            //slot is setting the item to null for the callbacks
            slot.ResetSlot();
            OnHandsItemSelectedChanged?.Invoke(null); //once it's null we say it to this delegate
            return item.GetGameObject();
        }

        public void TryRemoveKeyType(IInventoryItem item)
        {
            if (item is not Key) return;
            Key key = (Key)item;
            if (_keyTypesInInventory.Contains(key.KeyType))
                _keyTypesInInventory.Remove(key.KeyType);
        }

        #endregion

        #region Get / Find / Conditions
        /// <summary>
        /// Try to get the first empty slot in the inventory, if not we return null
        /// </summary>
        public ItemSlot GetEmptyPlaceInInventory()
        {
            for (int i = 0; i < _itemSlots.Count; i++)
            {
                if (_itemSlots[i].InventoryItem == null)
                    return _itemSlots[i];
            }
            return null;
        }

        /// <summary>
        /// Getting a slot from an item
        /// </summary>
        /// <param name="item"> the item we want to see</param>
        /// <returns></returns>
        public ItemSlot TryFindItemInSlots(IInventoryItem item)
        {
            foreach (ItemSlot slot in _itemSlots)
            {
                if (slot.InventoryItem == item)
                    return slot;
            }
            return null;
        }

        public bool HasItem(KeyType type)
        {
            return _keyTypesInInventory.Find(key => key == type);
        }

        public bool HasItems(KeyType[] requiredItem)
        {
            return requiredItem.All(item => HasItem(item));
        }

        //TO DOO Pouvoir récupérer des objets stackables
        public List<T> GetItemsOfType<T>() where T : class, IInventoryItem
        {
            return _itemSlots
                .Where(slot => slot.InventoryItem is T)
                .Select(slot => slot.InventoryItem as T)
                .ToList();
        }

        public T GetCurrentSelectedItemOfType<T>() where T : class, IInventoryItem
        {
            if (_currentSelectedSlot == null) return null;
            return _currentSelectedSlot.InventoryItem as T;
        }

        public bool GetCurrentSelectedItemTypeOf<T>() where T : class, IInventoryItem
        {
            if (_currentSelectedSlot == null) return false;
            return _currentSelectedSlot.InventoryItem is T;
        }

        public T GetItemOfType<T>(int slotIndex) where T : class, IInventoryItem
        {
            return _itemSlots[slotIndex].InventoryItem as T;
        }

        #endregion

        #region STACKABLE ITEMS

        private bool IsCurrentSelectedItemStackable()
        {
            return _currentSelectedSlot.InventoryItem is IInventoryStackable && _currentSelectedSlot.StackCount > 0;
        }

        public bool TryStackItem(IInventoryItem item)
        {
            if (item is not IInventoryStackable newStack) return false;

            foreach (ItemSlot slot in _itemSlots)
            {
                if (slot.InventoryItem is IInventoryStackable existingStack &&
                    existingStack.CanStackWith(item) &&
                    existingStack.CurrentStack < existingStack.MaxStack)
                {
                    int amount = newStack.CurrentStack;
                    existingStack.AddToStack(amount);
                    var slotobj = item.GetGameObject();
                    slotobj.transform.SetParent(InventoryParent);
                    slotobj.transform.position = InventoryParent.transform.position;
                    slotobj.transform.rotation = InventoryParent.transform.rotation;
                    slot.AddStackedItem(item); 
                    UpdateStackItemCountUI(slot);
                    return true;
                }
            }
            return false;
        }

        public T TryRemoveStackedItem<T>(IInventoryItem obj, IInventoryStackable stack, IInventoryItemCallback itemCallback) where T : class, IInventoryItem
        {
            ItemSlot slot = TryFindItemInSlots(obj);

            if (slot != null && slot.StackCount > 0)
            {
                IInventoryItem stackItemSelected = slot.RemoveStackedItem();
                PrepareItemToDropFromInventory(stackItemSelected, false);
                stack.RemoveFromStack(1);
                slot.UpdateItemStackCount();
                OnItemStackDropped?.Invoke(stackItemSelected);
                return stackItemSelected as T;
            }
            return null;
        }

        #endregion

        #region INTERFACES CALLBACKS

        /// <summary>
        /// From IInventoryItem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemCallback"></param>
        private void Item_ForceItemRemove(IInventoryItem item, IInventoryItemCallback itemCallback)
        {
            if (item == null) return;
            PrepareItemToDropFromInventory(item, false);
            RemoveItemFromInventory(TryFindItemInSlots(item), item, itemCallback);
        }

        /// <summary>
        /// From IInventoryUsable interface
        /// </summary>
        /// <param name="item"></param>
        private void InventoryUsableItem_CanBeUsedFromInventoryChanged(IInventoryItem item)
        {
            IsInventoryUsableUI(TryFindItemInSlots(item), item as IInventoryItemUsable);
        }

        #endregion

        #region UI
        public void ShowPickupItemInteractionUI(IInventoryItem inventoryItem)
        {
            _interactionCanvas.ShowItemPickup();
            _interactionCanvas.ItemPickup_TPM.text = inventoryItem.GetPickupName();
        }

        public void SetEnableStackCountText(ItemSlot target, bool value)
        {
            target.SetEnableItemStackCountText(value);
        }

        public void UpdateStackItemCountUI(ItemSlot target)
        {
            target.UpdateItemStackCount();
        }

        public void SetEnablePickupInteractionTextUI(bool value)
        {
            _interactionCanvas.HideItemPickup();
        }

        public void IsInventoryUsableUI(ItemSlot targetSlot, IInventoryItemUsable itemUsable)
        {
            targetSlot.IsInventoryItemUsable(itemUsable);
        }

        public void SetEnableItemNameTextUI(ItemSlot target, bool value)
        {
            target.SetEnableItemNameText(value);
        }

        public void SetItemNameTextUI(ItemSlot target, string text)
        {
            target.SetItemNameText(text);
        }

        public void SetEnableItemUseKeyUI(ItemSlot target, bool value)
        {
            target.ItemUseKey_TMP.gameObject.SetActive(value);
        }

        #endregion
    }
}
