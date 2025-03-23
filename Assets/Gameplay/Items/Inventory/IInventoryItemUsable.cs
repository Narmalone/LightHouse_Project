namespace LightHouse.Inventory
{
    public interface IInventoryItemUsable 
    {
        public bool CanBeUsedFromInventory { get; set; }
        string UseInInventoryText();
        void UseFromInventory();
    }

}
