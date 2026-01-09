namespace LightHouse.Inventory
{
    public class InventoryScrollHandler
    {
        #region FIELDS & PROPERTIES
        private ItemDatabase _itemDatabase;
        #endregion

        #region  Constructeur / Init
        public InventoryScrollHandler(ItemDatabase database) => _itemDatabase = database;
        #endregion

        #region SCROLL && MANAGERMENT
        public void Scroll(int direction)
        {
            int nextScrollValue = SlotManager.CurrentSlotIndex + direction;
            if (nextScrollValue < -1)
                nextScrollValue = SlotManager.SlotLength - 1;
            else if (nextScrollValue >= SlotManager.SlotLength)
                nextScrollValue = -1;
            SlotManager.ChangeSelectedSlot(_itemDatabase, (short)nextScrollValue);
        }
        #endregion
    }

}
