using LightHouse.Game.Computer.OS;
using UnityEngine;

public class LeaveApplication : ComputerApp
{
    public override void OnClose()
    {
        
    }

    public override void OnMinimize()
    {
        
    }

    public override void OnOpen()
    {
        _os.LeaveOS();
        Destroy(this.gameObject);
    }
}
