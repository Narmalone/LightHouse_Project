using LightHouse.Game.Computer.OS;

public class LeaveShortCut : ShortCutController
{
    public override void OnExecute(bool playSound = true)
    {
        _os.LeaveOS();
    }
}
