using UnityEngine;

[CreateAssetMenu(fileName = "ResetWorldStatWindowAction", menuName = "CustomConsole/Action/Windows/ResetWorldStatWindowAction")]
public class ResetWorldStatWindowAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        instance.WorldWindow.ResetWindow();
    }
}
