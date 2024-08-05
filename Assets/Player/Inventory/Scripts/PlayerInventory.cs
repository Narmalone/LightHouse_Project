using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    private PlayerManager _manager;

    [SerializeField] private byte slots = 4;
    [SerializeField] private byte currentUsedSlots = 0;
    [SerializeField] public CustomEvent _eventStartDropItem;
    [SerializeField] public CustomEvent _eventEndDropItem;
    [SerializeField] public GameObject previewObjectParent; // Reference to the 3D preview object
    [SerializeField] public List<InventorySlot> listInventorySlots = new List<InventorySlot>();

    private List<ItemBase> listPreviewObject = new List<ItemBase> { null, null, null, null};

    public ItemBase CurrentItemSelected => listPreviewObject[selectedSlot];

    private int selectedSlot;
    private PIA playerInputAction;
    private float _startDropTime;
    private float _maxStrengthThrowItem;
    private float _timeToAchieveMaxStrength;
    private AnimationCurve _curveStrengthGrow;

    public static bool IsInventoryFull = false;
    public static Action<ItemBase> TakeItemAction;
    
    private void Awake()
    {
        listPreviewObject = new List<ItemBase> { null, null, null, null };

        playerInputAction = new PIA();
        playerInputAction.Enable();
        playerInputAction.Game.UseInInventory.performed += OnUseSelectedItem;
        playerInputAction.Game.DropInventoryItem.performed += OnDropItem;
        playerInputAction.Game.DropInventoryItem.canceled += OnDropItem;

        TakeItemAction += TakeItem;
    }

    private void Start()
    {
        SelectSlot(0);
    }

    private void OnDestroy()
    {
        TakeItemAction -= TakeItem;
        playerInputAction.Game.UseInInventory.performed -= OnUseSelectedItem;
        playerInputAction.Game.DropInventoryItem.performed -= OnDropItem;
        playerInputAction.Game.DropInventoryItem.canceled -= OnDropItem;
    }

    void Update()
    {
        // Check for keyboard input to select a slot
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSlot(3);
        }

        // Check for mouse wheel input to select a slot
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            int scrollDirection = Input.GetAxis("Mouse ScrollWheel") > 0 ? -1 : 1;
            SelectSlot(selectedSlot + scrollDirection);
        }
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
        // Ajouter dans le slot (Choix slot, Nom, Icon)
        var slotIndex = GetEmptySlot();

        // If no empty slot
        if (slotIndex == -1) return;

        listInventorySlots[slotIndex].SetItem(item.ItemDatas);

        AddPreviewObject(slotIndex, item, item.ItemDatas.prefab);

        // Enlever l'item du jeux
        item.gameObject.SetActive(false);

        Destroy(item.gameObject);
    }

    private void DropItem()
    {
        if (listInventorySlots[selectedSlot].item == null) return;

        // Drop prefab
        var go = Instantiate(listInventorySlots[selectedSlot].item.prefab, _manager.transform.position + _manager._data.playerCamera.transform.forward * 2, Quaternion.identity);
        var itemDroped = go.GetComponent<ItemBase>();
        itemDroped?.SetStateObject(listInventorySlots[selectedSlot].previewItem);

        // Propulse Object
        itemDroped.TryGetComponent(out Rigidbody rb);
        if (rb != null) rb.AddForce(_manager._data.playerCamera.transform.forward * GetForceToDropItem(), ForceMode.Impulse);

        // Empty the item slot
        listInventorySlots[selectedSlot].SetItem(null);

        if (listPreviewObject[selectedSlot] != null)
        {
            Destroy(listPreviewObject[selectedSlot].gameObject);
        }
    }

    private float GetForceToDropItem()
    {
        return Mathf.Min(Time.time - _startDropTime, _timeToAchieveMaxStrength) / _timeToAchieveMaxStrength * _maxStrengthThrowItem;
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

    }

    private void AddPreviewObject(int index, ItemBase item, GameObject mesh)
    {
        if (listPreviewObject[index] != null) Destroy(listPreviewObject[index]);
        var go = Instantiate(mesh, previewObjectParent.transform.position, previewObjectParent.transform.rotation, previewObjectParent.transform);
        var itemPreview = go.GetComponent<ItemBase>();
        listPreviewObject[index] = itemPreview;

        itemPreview?.SetStateObject(item);
        listInventorySlots[index].SetPreviewItem(itemPreview);
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
        HandleUseSelectedItem();
    }

    private void OnDropItem(InputAction.CallbackContext context)
    {
        if (context.performed) HandleDropItem_Start();
        if (context.canceled) HandleDropItem_End();
    }
    private void HandleDropItem_Start()
    {
        _startDropTime = Time.time;
        _eventStartDropItem.Raise();
    }

    private void HandleDropItem_End()
    {
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