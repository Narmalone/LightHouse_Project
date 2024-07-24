using System;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/ScenarioEvent")]
public class ScenarioEvent : ScriptableObject
{
    public string eventName;
    public Action eventsAction;

    public void Play()
    {
        eventsAction?.Invoke();
    }
}
