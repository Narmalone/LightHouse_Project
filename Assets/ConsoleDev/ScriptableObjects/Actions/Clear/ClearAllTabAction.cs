using UnityEngine;

[CreateAssetMenu(fileName = "NewClearAllTabAction", menuName = "CustomConsole/Action/ClearAllTabAction")]
public class ClearAllTabAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        instance.ClearAll();
    }
}
