using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Mop : Key
    {

        public int _mopMaxUseCount = 3;
        public int _mopUseCount = 0;
        public bool IsWet
        {
            get => _mopUseCount > 0;
        }

        public void MakeMeWet()
        {
            _mopUseCount = _mopMaxUseCount;
        }

        public override void UseFromInventory()
        {
            base.UseFromInventory();
            if (IsWet)
            {
                _mopUseCount--;

            }
        }
    }

}
