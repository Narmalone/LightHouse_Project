using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoatsData
{
    public string BoatName;
}

[CreateAssetMenu(fileName = "BoatDatabase_", menuName = "LightHouse/Databases/New Boat Config")]
public class BoatsDatabase : ScriptableObject
{
    public List<BoatsData> BoatsConfigurations;
    public Sprite BoatFlags;
}
