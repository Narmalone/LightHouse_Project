using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Mop : Key
    {

        public int _mopMaxUseCount = 3;
        public int _mopUseCount = 0;
        private bool _useFromMopTracker = false;
        public bool IsWet
        {
            get => _mopUseCount > 0;
        }

        public void MakeMeWet()
        {
            _mopUseCount = _mopMaxUseCount;
            _useFromMopTracker = true;
        }

        public override void UseFromInventory()
        {
           
            base.UseFromInventory();
            if (_useFromMopTracker)
            {
                _useFromMopTracker = false;
                return;
            }
            if (IsWet)
            {
                _mopUseCount--;

            }
        }
    }

}
