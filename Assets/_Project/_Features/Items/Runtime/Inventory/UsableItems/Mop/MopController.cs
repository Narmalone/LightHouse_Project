namespace LightHouse.Features.Items.Inventory.Mop
{
    public class MopController : Key
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
                _mopUseCount--;
        }
    }

}
