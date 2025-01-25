using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResetMeteoDebuggerAction", menuName = "CustomConsole/Action/Windows/ResetMeteoDebuggerAction")]
public class ResetMeteoDebuggerAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        instance.MeteoWindow.ResetWindow();
    }
}
