using LightHouse.Inventory;
using UnityEngine;

public class InventoryScrollHandler
{
    private ItemSlot[] _slots;
    private byte _inventoryCapacity;
    private int _currentIndex;
    private ItemDatabase _itemDatabase;

    public int CurrentIndex => _currentIndex;

    public InventoryScrollHandler(ItemSlot[] slots, byte capacity, ItemDatabase database)
    {
        _slots = slots;
        _inventoryCapacity = capacity;
        _currentIndex = -1;
        _itemDatabase = database;
    }

    public void Scroll(int direction)
    {
        int nextScrollValue = _currentIndex + direction;
        ChangeSelectedSlot(nextScrollValue);
    }

    public void ChangeSelectedSlot(int slotID)
    {
        if (!IsIndexInvalid(_currentIndex))
        {
            _slots[_currentIndex].SlotDatas.IsSelected = false;
            if (_slots[_currentIndex].SlotDatas.HasItem)
                _slots[_currentIndex].HideSelectedInfos();
        }

        _currentIndex = slotID;

        if (_currentIndex < -1)
            _currentIndex = _inventoryCapacity - 1;
        else if (_currentIndex >= _inventoryCapacity)
            _currentIndex = -1;

        if (!IsIndexInvalid(_currentIndex))
        {
            _slots[_currentIndex].SlotDatas.IsSelected = true;
            if (_slots[_currentIndex].SlotDatas.HasItem)
                _slots[_currentIndex].Show();

            if (_slots[_currentIndex].SlotDatas.HasItem)
            {
                var item = _itemDatabase.Get(_slots[_currentIndex].SlotDatas.ItemGlobalID);
                InventoryHandlerData.SetSelectedItem(item);
            }
            else
            {
                InventoryHandlerData.ClearSelection();
            }
        }
        else
        {
            InventoryHandlerData.ClearSelection();
        }
    }

    public bool IsIndexInvalid(int index) => index < 0 || index >= _inventoryCapacity;
}
