using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameZoneType_", menuName = "GameSettings/GameZoneTypeInfo")]
public class GameZoneTypeInfo : ScriptableObject
{
    public GameZone[] OutsideZones;
    public GameZone[] InsideZones;
}

public enum GMTypeInfo
{
    Inside,
    Outside
}
