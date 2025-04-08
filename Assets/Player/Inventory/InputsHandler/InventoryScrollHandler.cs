using LightHouse.Inventory;

public class InventoryScrollHandler
{
    private ItemSlot[] _slots;
    private byte _inventoryCapacity;
    private int _currentIndex;

    public int CurrentIndex => _currentIndex;

    public InventoryScrollHandler(ItemSlot[] slots, byte capacity)
    {
        _slots = slots;
        _inventoryCapacity = capacity;
        _currentIndex = -1;
    }

    public void Scroll(int direction)
    {
        if (!IsIndexInvalid(_currentIndex))
        {
            _slots[_currentIndex].SlotDatas.IsSelected = false;
            if (_slots[_currentIndex].SlotDatas.HasItem)
                _slots[_currentIndex].HideSelectedInfos();
        }

        _currentIndex += direction;

        if (_currentIndex < -1)
            _currentIndex = _inventoryCapacity - 1;
        else if (_currentIndex >= _inventoryCapacity)
            _currentIndex = -1;

        if (!IsIndexInvalid(_currentIndex))
        {
            _slots[_currentIndex].SlotDatas.IsSelected = true;
            if (_slots[_currentIndex].SlotDatas.HasItem)
                _slots[_currentIndex].Show();
        }
    }

    public bool IsIndexInvalid(int index) => index < 0 || index >= _inventoryCapacity;
}
