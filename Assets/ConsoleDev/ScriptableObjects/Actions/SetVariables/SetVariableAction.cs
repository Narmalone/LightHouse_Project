using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewSetVarAction", menuName = "CustomConsole/Action/SetVarAction")]
public class SetVariableAction : SO_CommandAction
{
    private ChatController _chatController;
    public override void Execute(string[] args, ChatController i)
    {
        _chatController = i;
        if (args.Length != 2)
        {
            i.SendChatMessage("Usage: /set {variable} {value}", ChatTabs.Dev, "Console", logLevel: LogLevel.Warning);
            return;
        }
        string variableName = args[0];
        string valueString = args[1];
        SetVariable(variableName, valueString);
    }

    public void SetVariable(string variableName, string valueString)
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        bool variableFound = false;

        foreach (MonoBehaviour mb in allMonoBehaviours)
        {
            IEnumerable<FieldInfo> fields = mb.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.IsDefined(typeof(ConsoleVariableAttribute), false));

            foreach (FieldInfo field in fields)
            {
                ConsoleVariableAttribute attribute = (ConsoleVariableAttribute)field.GetCustomAttribute(typeof(ConsoleVariableAttribute));
                if (field.Name == variableName)
                {
                    try
                    {
                        Type valueType = field.FieldType;
                        object value = Convert.ChangeType(valueString, valueType);
                        field.SetValue(mb, value);
                        _chatController.SendChatMessage($"Variable {variableName} set to {value}", ChatTabs.Dev);
                        variableFound = true;
                    }
                    catch (Exception ex)
                    {
                        _chatController.SendChatMessage($"Error setting variable {variableName}: {ex.Message}", ChatTabs.Dev, logLevel: LogLevel.Error);
                    }
                }
            }
        }

        if (!variableFound)
        {
            _chatController.SendChatMessage($"Variable {variableName} not found!", ChatTabs.Dev, logLevel: LogLevel.Error);
        }
    }
}
