using UnityEngine;

public enum NationalitiesBoats
{
    EN,
    FR,
    RU,
    DE,
    SPA,
    CA
}

[System.Serializable]
public class BoatData
{
    public string Name;
}

[CreateAssetMenu(fileName = "BoatDatabase_", menuName = "LightHouse/Databases/New Boat Config")]
public class BoatsNationalitiesConfig : ScriptableObject
{
    public NationalitiesBoats Country;
    public Sprite Flag;
    public BoatData[] BoatsDatas;
}
