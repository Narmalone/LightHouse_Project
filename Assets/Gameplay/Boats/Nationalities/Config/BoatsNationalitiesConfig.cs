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
    public Sprite NationalityFlag;
}

[CreateAssetMenu(fileName = "BoatDatabase_", menuName = "LightHouse/Databases/New Boat Config")]
public class BoatsNationalitiesConfig : ScriptableObject
{
    public NationalitiesBoats Country;
    public string NationalityName;
    public Sprite Flag;
    public BoatData[] BoatsDatas;

    private void OnValidate()
    {
        foreach(var boatData in BoatsDatas)
        {
            boatData.NationalityFlag = Flag;
        }
    }
}
