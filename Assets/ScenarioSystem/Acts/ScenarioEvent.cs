using System;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/ScenarioEvent")]
public class ScenarioEvent : ScriptableObject
{
    public string eventName;
    public ScenarioCondition condition;
    public Action eventsAction;

    public void Play()
    {
        eventsAction?.Invoke();
    }
}

[Flags]
public enum ScenarioCondition
{
    PlayerProximity,
    HighTide,
    LowTide,
    Caca
}