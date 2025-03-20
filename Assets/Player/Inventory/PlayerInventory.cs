using System.Collections.Generic;
using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;
using LightHouse.Utilities;
using System.Linq;
using System;
using LightHouse.Items.Samples;

namespace LightHouse.KinematicCharacterController
{
    public class PlayerInventory : MonoBehaviour
    {
        #region SERILIAZED FIELDS
        [Header("Inventory Settings")]
        [SerializeField] private bool _enableInventoryRaycast = true;
        [SerializeField] private byte _inventoryCapacity = 4;
        [SerializeField] private int _currentSlotIndex = -1;
        [SerializeField] private byte _currentNumberOfItemsInInventory = 0;
        [SerializeField] private Vector3 _inventoryOffset = new Vector3(0.5f, -0.5f, 0.8f); // (X = décalage horizontal, Y = vertical, Z = profondeur)

        [Header("Inventory References")]
        [SerializeField] private Transform _inventoryParent = null;
        [SerializeField] private Transform _inventoryTarget = null;
        [SerializeField] private Transform _playerCameraTransform = null;

        [Header("Raycast")]
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
        private Collider[] _dropOverlappingCollider = new Collider[0];

        private ItemSlot _lastSelectedSlot = null;
        public ItemSlot _currentSelectedSlot = null;

        //Raycast Info
        private bool _isRaycastingSomething = false;
        private GameObject _lastObjectSeen;
        private IInventoryItem _raycastedInventoryItem;
        //private IInteractable _raycastedInteractableItem;

        //Drops Info
        private float _dropPower = 0f;
        private bool _isChargingDrop = false;

        public static event Action<PlayerInventory> OnInventoryInitialized;

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

        private void Start()
        {
            _itemSlots = _inventoryCanvas.GenerateItemSlot(_inventoryCapacity).ToList();
            _interactionCanvas.HideItemPickup();
            OnInventoryInitialized?.Invoke(this);
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
                {
                    ResetSeenObject();
                }

                HandleInteractItemInInventory();
                HandleDropInput();
                Debug.DrawRay(_playerCamera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
            }
        }

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
            int inversedScroll = -scrollDirection; //inver scroll direction (scroll up to left, scroll down to right)

            if (inversedScroll == 0) return; 

            _currentSlotIndex += inversedScroll;

            ChangeSelectedSlot(_currentSlotIndex);
        }

        private void HandleInteractItemInInventory()
        {
            if (InputManager.InteractInInventory.WasPressedThisFrame() )
            {
                if (_currentSelectedSlot != null && _currentSelectedSlot.InventoryItem != null)
                {
                    _currentSelectedSlot.InventoryItem.UseFromInventory();
                }
            }
            
        }

        private void HandlePickupInput()
        {
            if (_raycastedInventoryItem != null && InputManager.PickUp.WasPerformedThisFrame())
            {
                AddItem(_raycastedInventoryItem);
            }
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

        #region Change Current Selected Slot
        /// <summary>
        /// Change the current selected slot based on somme index
        /// <cf _currentSlotIndex> </cf>
        /// </summary>
        public void ChangeSelectedSlot(int index)
        {
            // Cyclic scrolling, -1 is for nothing selected
            if (_currentSlotIndex < -1)
            {
                _currentSlotIndex = _inventoryCapacity - 1;
            }
            else if (_currentSlotIndex >= _inventoryCapacity)
            {
                _currentSlotIndex = -1;
            }

            if(_currentSlotIndex < 0)
            {
                _currentSelectedSlot = null;
            }

            if (_lastSelectedSlot != null)
            {
                if (_lastSelectedSlot.ItemName_TMP.isActiveAndEnabled)
                {
                    _lastSelectedSlot.ItemName_TMP.gameObject.SetActive(false);

                    if(_lastSelectedSlot.InventoryItem != null)
                        _lastSelectedSlot.InventoryItem.GetItem().gameObject.SetActive(false);
                }
                SetInteractableItemUseKeyUI(_lastSelectedSlot, false);
            }

            //Handle case where we have nothing on hands
            if (_currentSlotIndex < 0 || _currentSlotIndex >= _inventoryCapacity) return;

            _currentSelectedSlot = _itemSlots[_currentSlotIndex];
            _lastSelectedSlot = _currentSelectedSlot;

            if(_currentSelectedSlot.InventoryItem != null)
            {
                UpdateInventorySlotUI(_currentSelectedSlot, _currentSelectedSlot.InventoryItem.GetName());
                IsInventoryUsableUI(_currentSelectedSlot, _currentSelectedSlot.InventoryItem);
                _currentSelectedSlot.InventoryItem.GetItem().gameObject.SetActive(true);
                if (!_currentSelectedSlot.ItemName_TMP.isActiveAndEnabled)
                    _currentSelectedSlot.ItemName_TMP.gameObject.SetActive(true);
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

                if (_raycastedInventoryItem != null)
                {
                    ShowInventoryPickupUI(_raycastedInventoryItem);
                }
                //in case we are looking an item and looking an other item whithout seeing nothing (without the ResetSeenObjectMethod())
                else
                {
                    if (!_interactionCanvas.IsHided)
                        _interactionCanvas.HideItemPickup();
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

            if (_interactionCanvas.ItemInventory_TMP.isActiveAndEnabled)
                _interactionCanvas.HideItemPickup();
        }

        #endregion

        #region ADD / DROP / REMOVE ITEMS

        /// <summary>
        /// Called when the player press the pickup key while looking an item
        /// </summary>
        /// <param name="item"> the item we are trying to add in the inventory </param>
        /// <returns> if the items sucessfully been added to inventory</returns>
        public bool AddItem(IInventoryItem item)
        {
            if (_currentNumberOfItemsInInventory >= _inventoryCapacity)
            {
                Debug.LogWarning("Inventaire plein !");
                return false;
            }

            if(item is Key)
            {
                Key key = (Key)item;
                _keyTypesInInventory.Add(key.KeyType);
            }

            _interactionCanvas.ItemInventory_TMP.text = "";
            GameObject obj = item.GetItem().gameObject;
            obj.transform.SetParent(_inventoryParent);
            obj.transform.position = _inventoryTarget.position;
            obj.transform.rotation = _inventoryTarget.rotation;

            item.GetCollider().enabled = false;
            item.GetRigidBody().isKinematic = true;

            item.IsItemInInventory = true;
            _currentNumberOfItemsInInventory++;
            item.OnItemAddedToInventory();

            //Handle in case we had an item, or out of range
            if (_currentSlotIndex < 0 || _currentSlotIndex >= _inventoryCapacity || _currentSelectedSlot.InventoryItem != null)
            {
                ItemSlot targetSlot = GetEmptyPlaceInInventory();
                targetSlot.SetInventoryItem(item);
                obj.SetActive(false);
            }
            //else we presume that the item can be store on our hands bcs it's the current selected slot
            else if (_currentSelectedSlot == _itemSlots[_currentSlotIndex])
            {
                _currentSelectedSlot.SetInventoryItem(item);
                UpdateInventorySlotUI(_currentSelectedSlot, item.GetName());
                _currentSelectedSlot.ItemName_TMP.gameObject.SetActive(true);
                IsInventoryUsableUI(_currentSelectedSlot, item);
            }

            item.ForceRemoveItemInInventory += () => Item_ForceItemRemove(item);
            return true;
        }

        private void Item_ForceItemRemove(IInventoryItem item)
        {
            Debug.Log($"Suppression forcée de l'objet : {item.GetName()}");

            if (item == null) return;

            RemoveItem(item);
        }

        /// <summary>
        /// Try to see if a specific item is inside a slot, and if so we remove it
        /// </summary>
        /// <param name="item"> the item you try to remove </param>
        public void RemoveItem(IInventoryItem item)
        {
            ItemSlot slot = TryFindItemInItemSlot(item);
            if (slot == null) return;

            Debug.Log($"Objet supprimé : {item.GetName()}");

            _currentSelectedSlot.ItemName_TMP.gameObject.SetActive(false);
            _currentSelectedSlot.ItemUseKey_TPM.gameObject.SetActive(false);

            RemoveKeyType(item);
            item.IsItemInInventory = false;
            _currentNumberOfItemsInInventory--;
            item.OnItemRemovedFromInventory();
            slot.ResetSlot();
            
            //Se désinscrire de l’événement après suppression
            item.ForceRemoveItemInInventory -= () => Item_ForceItemRemove(item);
        }


        /// <summary>
        /// Remove the item of the current selected slot
        /// </summary>
        public void RemoveCurrentSelectedItem()
        {
            RemoveKeyType(_currentSelectedSlot.InventoryItem);
            _currentSelectedSlot.ItemName_TMP.gameObject.SetActive(false);
            _currentSelectedSlot.ItemUseKey_TPM.gameObject.SetActive(false);
            _currentNumberOfItemsInInventory--;
            _currentSelectedSlot.InventoryItem.IsItemInInventory = false;
            _currentSelectedSlot.InventoryItem.OnItemRemovedFromInventory();
            _currentSelectedSlot.ResetSlot();
        }

        public void RemoveKeyType(IInventoryItem item)
        {
            if (item is not Key) return;
            Key key = (Key)item;
            if (_keyTypesInInventory.Contains(key.KeyType))
            {
                _keyTypesInInventory.Remove(key.KeyType);  
            }
        }

        /// <summary>
        /// Drop the item inside the current selected slot on a specific position with a hold force
        /// </summary>
        /// <param name="dropPosition"> the position where you want to drop the item </param>
        /// <param name="force"> the force added to the forward of the position if a rigibody is attached to it </param>
        public void DropCurrentSelectedItem(Vector3 dropPosition, float force)
        {
            if (_itemSlots[_currentSlotIndex].InventoryItem == null) return;

            IInventoryItem item = _itemSlots[_currentSlotIndex].InventoryItem;
            GameObject obj = item.GetItem().gameObject;

            obj.transform.SetParent(null);

            Vector3 safeDropPosition = GetAdjustedDropPosition(dropPosition);
            obj.transform.position = safeDropPosition;
            obj.transform.rotation = Quaternion.Euler(0f, _playerCamera.transform.eulerAngles.y, 0f);
            item.GetCollider().enabled = true;
            item.GetRigidBody().isKinematic = false;
            obj.SetActive(true);

            Rigidbody rb = item.GetRigidBody();
            if (rb != null && safeDropPosition == dropPosition)
            {
                rb.AddForce(_inventoryTarget.forward * force, ForceMode.Impulse);
            }

            RemoveCurrentSelectedItem();
        }

        private Vector3 GetAdjustedDropPosition(Vector3 intendedDropPosition)
        {
            Vector3 start = _inventoryTarget.position;
            float sphereRadius = _securityOverlapSphereRadius; // Ajuste la taille en fonction des objets

            // 1. Vérifier si un obstacle est proche avec un OverlapSphere
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            _dropOverlappingCollider = colliders;
            if (colliders.Length > 0)
            {
                Debug.Log("Obstacle trop proche ! Dépose l'objet juste devant le joueur.");
                Vector3 adjustedPosition = start - _inventoryTarget.forward * 0.5f; // Reculer légèrement pour ne pas être dans l'obstacle
                Debug.DrawRay(start, -_inventoryTarget.forward * 0.5f, Color.yellow, 5.0f);
                return adjustedPosition;
            }

            return intendedDropPosition;
        }

        private void OnDrawGizmos()
        {
            Vector3 start = _inventoryTarget.position;
            float sphereRadius = _securityOverlapSphereRadius; // Ajuste la taille en fonction des objets

            // 1. Vérifier si un obstacle est proche avec un OverlapSphere
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            _dropOverlappingCollider = colliders;

            Gizmos.color = _dropOverlappingCollider.Length > 0 ? Color.red : Color.green; 
            Gizmos.DrawSphere(_inventoryTarget.position, 0.3f);
        }

        #endregion

        #region Get / Find / Organisation
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
        public ItemSlot TryFindItemInItemSlot(IInventoryItem item)
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

        #endregion

        #region UI
        public void ShowInventoryPickupUI(IInventoryItem inventoryItem)
        {
            _interactionCanvas.ShowItemPickup();
            _interactionCanvas.ItemInventory_TMP.text = inventoryItem.GetPickupName();
        }

        public void UpdateInventorySlotUI(ItemSlot item, string name)
        {
            item.ItemName_TMP.text = name;
        }

        public void IsInventoryUsableUI(ItemSlot targetSlot, IInventoryItem item)
        {
            if (item.CanBeUsedFromInventory)
            {
                targetSlot.ItemUseKey_TPM.gameObject.SetActive(true);
                
            }
            else
            {
                targetSlot.ItemUseKey_TPM.gameObject.SetActive(false);
            }
        }

        public void SetInteractableItemUseKeyUI(ItemSlot target, bool value)
        {
            target.ItemUseKey_TPM.gameObject.SetActive(value);
        }

        #endregion
    }
}
