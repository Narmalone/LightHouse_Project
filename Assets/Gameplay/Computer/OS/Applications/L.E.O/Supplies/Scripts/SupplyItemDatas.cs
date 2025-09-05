using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    [System.Serializable]
    public class SupplyItemDatas
    {
        public int UniqueID;
        public string Name;
        public float Cost;
        [System.NonSerialized] public float CurrentStockCount;
        [System.NonSerialized] public int SelectedAmountToBuy; //non serialized because stored in a scriptable object
                                                               //so it will be stored while in runtime
    }
}
