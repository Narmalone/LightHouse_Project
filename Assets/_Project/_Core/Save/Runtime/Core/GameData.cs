using LightHouse.Core.Save.Sample;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public string Name;
    public string CurrentLevelName;
    public string DisplayName;        // ex: "Partie 1"
    public string LastSaveTime;       // string ISO pour JsonUtility
    public float PlaytimeSeconds;
    public bool IsNewGame;

    public PlayerData PlayerData;
    public Dictionary<string, string> EntityStates = new();
}
