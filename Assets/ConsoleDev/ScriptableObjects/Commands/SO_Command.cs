using UnityEngine;

[CreateAssetMenu(fileName = "NewSuggestionCommand", menuName = "CustomConsole/NewCommand")]
public class SO_Command : ScriptableObject
{
    public string keyName;
    public string command;
    public string commandUsage = "/set {variable} {value}";
    [field: SerializeField]
    public SuggestionSource suggestionSource { get; set; } 
    public SO_CommandAction action;

    public int GetSubstringValue()
    {
        return command.Length;
    }

    public enum SuggestionSource
    {
        VariableNames,
        SceneNames,
        ObjectNames,
        //ajouter d'autres choses
    }

}

public abstract class SO_CommandAction : ScriptableObject
{
    public abstract void Execute(string[] args, ChatController instance);
}