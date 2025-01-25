using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeteoDebuggerAction", menuName = "CustomConsole/Action/Windows/MeteoDebuggerAction")]
public class MeteoDebuggerAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        if (instance.MeteoWindow.IsShowed)
        {
            instance.MeteoWindow.Disable();
        }
        else
        {
            instance.MeteoWindow.Enable();
        }
    }
}
