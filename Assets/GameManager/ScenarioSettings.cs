using UnityEngine;

[CreateAssetMenu(menuName = "GameSettings/Scenario")]
public class ScenarioSettings : ScriptableObject
{
    [Range(0, 5)] public int minEventPerWeek;
    [Range(0, 8)] public int maxEventPerWeek;

    public int GetRandomFromEvent()
    {
        return Random.Range(minEventPerWeek, maxEventPerWeek);
    }
}