namespace LightHouse.Core.Player.Inventory.Callbacks
{
    public interface IInventoryItemCallback
    {
        void OnItemAddedToInventory();
        void OnItemRemovedFromInventory();
    }

}
