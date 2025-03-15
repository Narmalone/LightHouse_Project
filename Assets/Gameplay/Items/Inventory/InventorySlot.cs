using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public uint StackSize;

    public InventorySlot()
    {

    }

    public void AddToStack(uint numberToAdd)
    {
        StackSize += numberToAdd;   
    }
}
