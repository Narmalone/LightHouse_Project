using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    [System.Serializable]
    public class SupplyItemDatas
    {
        public int UniqueID;
        public string Name;
        public int Cost = 15;
        [System.NonSerialized] public int CurrentStockCount;
        [System.NonSerialized] public int SelectedAmountToBuy; //non serialized because stored in a scriptable object
                                                               //so it will be stored while in runtime
    }
}
