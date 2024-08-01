using UnityEngine;


[CreateAssetMenu(fileName = "StatWorldWindow", menuName = "CustomConsole/Action/StatWorldWindow")]
public class WorldWindowStatAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        if (instance.WorldWindow.IsShowed)
        {
            instance.WorldWindow.Disable();
        }
        else
        {
            instance.WorldWindow.Enable();
        }
    }
}
