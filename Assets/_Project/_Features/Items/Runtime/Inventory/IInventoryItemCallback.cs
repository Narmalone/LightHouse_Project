namespace LightHouse.Inventory
{
    public interface IInventoryItemCallback
    {
        void OnItemAddedToInventory();
        void OnItemRemovedFromInventory();
    }

}
