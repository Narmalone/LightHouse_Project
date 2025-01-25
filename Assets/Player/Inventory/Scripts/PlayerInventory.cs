using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    private PlayerManager _manager;

    [SerializeField] private byte slots = 4;
    [SerializeField] private byte currentUsedSlots = 0;
    [SerializeField] public CustomEvent_2Float _eventStartDropItem;
    [SerializeField] public CustomEvent _eventDropItem;
    [SerializeField] public CustomEvent _eventEndDropItem;
    [SerializeField] private CustomEvent_ItemBase _fromStorageToInventory;
    [SerializeField] public GameObject previewObjectParent; // Reference to the 3D preview object
    [SerializeField] public List<InventorySlot> listInventorySlots = new List<InventorySlot>();

    [SerializeField] private CanvasGroup _inventoryGroup;
    [SerializeField] private CustomEvent _inventoryShow; 
    [SerializeField] private CustomEvent _inventoryHide; 

    [SerializeField]
    private List<ItemBase> listPreviewObject = new List<ItemBase> { null, null, null, null};

    public ItemBase CurrentItemSelected
    {
        get => listPreviewObject[selectedSlot];
        private set
        {
            OnCurrentItemSelectedChanged?.Invoke(listPreviewObject[selectedSlot], value);
            listPreviewObject[selectedSlot] = value;
        }
    }

    private int selectedSlot;
    private PIA _inputs;
    private float _startDropTime;
    private float _maxStrengthThrowItem;
    private float _timeToAchieveMaxStrength;
    private AnimationCurve _curveStrengthGrow;

    public bool IsInventoryFull => HasEmptySlot();

    //EVENTS
    public static Action<ItemBase> TakeItemAction;
    public static event Action<ItemBase, ItemBase> OnCurrentItemSelectedChanged; //old, new value
    
    private void Awake()
    {
        listPreviewObject = new List<ItemBase> { null, null, null, null };

        _inputs = new PIA();
        _inputs.Enable();
        _inputs.Game.UseInInventory.performed += OnUseSelectedItem;
        _inputs.Game.DropInventoryItem.performed += OnDropItem;
        _inputs.Game.DropInventoryItem.canceled += OnDropItem;

        _inputs.Inventory.Slot1.performed += SelectSlot0;
        _inputs.Inventory.Slot2.performed += SelectSlot1;
        _inputs.Inventory.Slot3.performed += SelectSlot2;
        _inputs.Inventory.Slot4.performed += SelectSlot3;
        _inputs.Inventory.Scroll.performed += ScrollWheel;

        TakeItemAction += TakeItem;
        _eventDropItem.handle += OnDropItem;

        _fromStorageToInventory.handle += _fromStorageToInventory_handle;

        _inventoryShow.handle += _inventoryShow_handle;
        _inventoryHide.handle += _inventoryHide_handle;
    }
    private void Start()
    {
        SelectSlot(0);

        _manager._eventUpdate -= OnUpdate;
    }

    private void OnDestroy()
    {
        TakeItemAction -= TakeItem;
        _inputs.Game.UseInInventory.performed -= OnUseSelectedItem;
        _inputs.Game.DropInventoryItem.performed -= OnDropItem;
        _inputs.Game.DropInventoryItem.canceled -= OnDropItem;

        _inputs.Inventory.Slot1.performed -= SelectSlot0;
        _inputs.Inventory.Slot2.performed -= SelectSlot1;
        _inputs.Inventory.Slot3.performed -= SelectSlot2;
        _inputs.Inventory.Slot4.performed -= SelectSlot3;
        _inputs.Inventory.Scroll.performed -= ScrollWheel;

        _eventDropItem.handle -= OnDropItem;

        _fromStorageToInventory.handle -= _fromStorageToInventory_handle;

        _manager._eventUpdate -= OnUpdate;

        _inventoryShow.handle -= _inventoryShow_handle;
        _inventoryHide.handle -= _inventoryHide_handle;
    }

    private void _inventoryHide_handle()
    {
        _inventoryGroup.alpha = 0f;
        _inventoryGroup.interactable = false;
        _inventoryGroup.blocksRaycasts = false;
    }

    private void _inventoryShow_handle()
    {
        _inventoryGroup.alpha = 1f;
        _inventoryGroup.interactable = true;
        _inventoryGroup.blocksRaycasts = true;
    }

    void OnUpdate() { }

    private void ScrollWheel(InputAction.CallbackContext context)
    {
        if (context.ReadValue<Vector2>() != Vector2.zero)
        {
            int scrollDirection = context.ReadValue<Vector2>().y < 0? -1 : 1;
            SelectSlot(selectedSlot + scrollDirection);
        }
    }
    private void SelectSlot0(InputAction.CallbackContext context) => SelectSlot(0);
    private void SelectSlot1(InputAction.CallbackContext context) => SelectSlot(1);
    private void SelectSlot2(InputAction.CallbackContext context) => SelectSlot(2);
    private void SelectSlot3(InputAction.CallbackContext context) => SelectSlot(3);

    public void EnableUseInInventory()
    {
        _inputs.Game.UseInInventory.performed += OnUseSelectedItem;
    }

    public void DisableUseInInventory()
    {
        _inputs.Game.UseInInventory.performed -= OnUseSelectedItem;
    }

    #region OTHER FUNCTIONS

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
        _maxStrengthThrowItem = _manager._data._maxStrengthThrowItem;
        _timeToAchieveMaxStrength = _manager._data._timeToAchieveMaxStrength;
        _curveStrengthGrow = _manager._data._curveStrengthGrow;
    }

    private void TakeItem(ItemBase item)
    {
        if (_manager.Freeze) return;

        // Ajouter dans le slot (Choix slot, Nom, Icon)
        var slotIndex = GetEmptySlot();

        // If no empty slot
        if (slotIndex == -1) return;

        var previewItem = AddPreviewObject(slotIndex, item, item.ItemDatas.prefab);

        listInventorySlots[slotIndex].SetItem(previewItem);

        // Enlever l'item du jeux
        item.gameObject.SetActive(false);
        Destroy(item.gameObject);
    }

    private void OnDropItem()
    {
        if (_manager.Freeze) return;

        DropItem(false);
    }

    public ItemBase DropItem(bool respawnObject = true, InventorySlot slot = null)
    {
        if (slot == null) slot = listInventorySlots[selectedSlot];

        if (slot.item == null) return null;

        ItemBase itm = null;
        if (respawnObject)
        {
            // Drop prefab
            var go = Instantiate(slot.item.ItemDatas.prefab, _manager.transform.position + _manager._data.playerCamera.transform.forward * 2, Quaternion.identity);
            itm = go.GetComponent<ItemBase>();
            itm?.SetStateObject(slot.previewItem);
            // Propulse Object
            itm.TryGetComponent(out Rigidbody rb);
            if (rb != null) rb.AddForce(_manager._data.playerCamera.transform.forward * GetForceToDropItem() * rb.mass, ForceMode.Impulse);
        }

        // Empty the item slot
        slot.SetItem(null);

        if (slot != null)
        {
            Destroy(listPreviewObject[listInventorySlots.FindIndex(x => x == slot)].gameObject);
        }
        return itm;
    }

    #region STORAGE FUNCS

    private void _fromStorageToInventory_handle(ItemBase obj)
    {
        //ajouter l'obj à l'inventaire
        TakeItem(obj);
    }
    #endregion

    public void RemoveItemFromInventory(ItemBase target)
    {
        if (listPreviewObject.Contains(target))
        {
            listInventorySlots[listPreviewObject.FindIndex(x => x == target)].SetItem(null);
            Destroy(listPreviewObject.Find(x => x == target).gameObject);
        }
    }

    private float GetForceToDropItem()
    {
        return Mathf.Min(Time.time - _startDropTime, _timeToAchieveMaxStrength) / _timeToAchieveMaxStrength * _maxStrengthThrowItem;
    }

    private bool HasEmptySlot()
    {
        int count = 0;
        for(int i = 0; i < listInventorySlots.Count; i++)
        {
            if (listInventorySlots[i].previewItem != null)
                count++;
        }
        return count >= listInventorySlots.Count;
    }

    private int GetEmptySlot()
    {
        if (currentUsedSlots >= slots) return -1;

        if (listInventorySlots[selectedSlot].isEmpty) return selectedSlot;

        for (int i = 0; i < listInventorySlots.Count; i++)
        {
            if (listInventorySlots[i].isEmpty == false) continue;
            return i;
        }
        return -1;
    }

    void SelectSlot(int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, slots - 1);

        UpdatePreviewObject(slotIndex);
        listInventorySlots[selectedSlot].OnDeselect();
        selectedSlot = slotIndex;
        listInventorySlots[selectedSlot].OnSelect();
        CurrentItemSelected = listPreviewObject[selectedSlot]; 
    }

    private ItemBase AddPreviewObject(int index, ItemBase item, GameObject mesh)
    {
        if (listPreviewObject[index] != null) Destroy(listPreviewObject[index]);
        var go = Instantiate(mesh, previewObjectParent.transform.position, previewObjectParent.transform.rotation, previewObjectParent.transform);
        var itemPreview = go.GetComponent<ItemBase>();
        listPreviewObject[index] = itemPreview;

        itemPreview.SetStateObject(item);
        listInventorySlots[index].SetPreviewItem(itemPreview);
        if (itemPreview != CurrentItemSelected) itemPreview.gameObject.SetActive(false);
        return itemPreview;
    }

    private void UpdatePreviewObject(int nextIndex)
    {
        if (listPreviewObject[selectedSlot] != null)
            listPreviewObject[selectedSlot].gameObject.SetActive(false);

        if (listPreviewObject[nextIndex] != null)
            listPreviewObject[nextIndex].gameObject.SetActive(true);
    }

    #endregion

    #region INPUT CALBACK
    private void OnUseSelectedItem(InputAction.CallbackContext context)
    {
        if (_manager.Freeze) return;

        HandleUseSelectedItem();
    }

    private void OnDropItem(InputAction.CallbackContext context)
    {
        if (_manager.Freeze) return;

        if (context.performed) HandleDropItem_Start();
        if (context.canceled) HandleDropItem_End();
    }
    private void HandleDropItem_Start()
    {
        _startDropTime = Time.time;
        _eventStartDropItem.Raise(PlayerManager.Instance._data._timeToAchieveMaxStrength,0);
    }

    private void HandleDropItem_End()
    {
        if (_manager.Freeze) return;

        DropItem();
        _eventEndDropItem.Raise();
    }

    #endregion

    #region HANDLE
    private void HandleUseSelectedItem()
    {
        if (listInventorySlots[selectedSlot].isEmpty || !listInventorySlots[selectedSlot].previewItem.IsUsable) return;

        listInventorySlots[selectedSlot].RaiseUseItem();
    }

    #endregion

}