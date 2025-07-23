using UnityEngine;

public class NightwatchTransitionButton : LEOWindowButton
{
    [SerializeField] private NightWatchController controller => App.NightWatch;
    [SerializeField] private E_NightWatchMode _mode;

    public override void OnClick()
    {
        base.OnClick();
        controller.SwitchTo(_mode);
    }
}
