using UnityEngine;

[CreateAssetMenu(fileName = "StatResetAction", menuName = "CustomConsole/Action/StatResetAction")]
public class StatFPSResetAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        instance.FpsWindow.ResetWindow();
    }
}
