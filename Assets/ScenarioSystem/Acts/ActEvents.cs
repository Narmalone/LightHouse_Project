using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/ActEvents")]
public class ActEvents
    : ScriptableObject
{
    //Une liste d'évènements
    public List<ScenarioEvent> possibleEvents;
}
