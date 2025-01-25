using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/ActEvents")]
public class ActEvents : ScriptableObject
{
    public List<ScenarioEvent> possibleEvents;
    public int NumberOfEventsToPlayInAct = 3;
    public event Action OnEvent;

    public virtual void Play()
    {
        OnEvent?.Invoke();
    }
}
