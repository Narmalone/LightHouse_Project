using UnityEngine;

[CreateAssetMenu(fileName = "TimeConfig_Default", menuName = "LightHouse/Time/New Config")]
public class TimeConfiguration : ScriptableObject
{
    public ushort TotalDays = 31;
    public float dayLengthInMinits = 10.0f;

    public float GetTotalGameTimeInSeconds()
    {
        Debug.Log(TotalDays * dayLengthInMinits);
        return TotalDays * dayLengthInMinits;
    }
}
