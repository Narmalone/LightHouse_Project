using LightHouse.Game.Computer.OS;

public class LeaveShortCut : ShortCutController
{
    public override void OnExecute()
    {
        _os.LeaveOS();
    }
}
