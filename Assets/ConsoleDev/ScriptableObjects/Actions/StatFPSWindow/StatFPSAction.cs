using UnityEngine;

[CreateAssetMenu(fileName = "StatAction", menuName = "CustomConsole/Action/StatAction")]
public class StatFPSAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        if (instance.FpsWindow.IsShowed)
        {
            instance.FpsWindow.Disable();
        }
        else
        {
            instance.FpsWindow.Enable();
        }
    }
}
