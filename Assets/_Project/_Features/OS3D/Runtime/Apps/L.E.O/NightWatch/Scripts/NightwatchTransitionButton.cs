using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch
{
    public class NightwatchTransitionButton : LEOWindowButton
    {
        private NightWatchController controller => App.NightWatch;
        [SerializeField] private E_NightWatchMode _mode;

        public override void OnClick()
        {
            base.OnClick();
            controller.SwitchTo(_mode);
        }
    }

}
