using System;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/ScenarioEvent")]
public class ScenarioEvent : ScriptableObject
{
    public string eventName;
    public ScenarioCondition condition;
    public Action eventAction;

    public void Play()
    {
        eventAction?.Invoke();
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