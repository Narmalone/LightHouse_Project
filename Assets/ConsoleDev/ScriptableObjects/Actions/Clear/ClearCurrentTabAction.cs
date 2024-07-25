using UnityEngine;

[CreateAssetMenu(fileName = "ClearCurrentTabAction", menuName = "CustomConsole/Action/ClearCurrentTabAction")]
public class ClearCurrentTabAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        instance.ClearChat(ChatTabs.CurrentSelectedTab);
    }
}
