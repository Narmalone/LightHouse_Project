using System;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/Event/BaseScenarioEvent")]
public class ScenarioEvent : ScriptableObject
{
    [Header("BASE SCENARIO")]
    public string EventName;
    public int DifficultyLevel = 0;
    public ScenarioCondition Condition;
    public event Action EventAction;
    public bool IsMandatory = false; //si l'event est obligatoire

    public virtual void Play()
    {
        EventAction?.Invoke();
        Debug.Log($"l'évènement {this.name} est lancé.");
    }

    public bool IsEqual(ScenarioCondition condition)
    {
        return Condition == condition;
    }

    //il faut vérifier si le / les flags correspondants sont présents et non pas si 
    //c'est exactement la même chose
    public bool AreConditionsValid(ScenarioCondition condition)
    {
        // Vérifie si tous les flags de l'événement sont présents dans la condition active
        //ne pas inverser Condition et condition car sinon ça veut dire qu'on estime
        //qu'il faille plusieurs flags à chaque events, là ça marche pour les 2
        return (condition & Condition) == Condition;
    }

}

[Flags]
//on peut aussi faire PlayerSleeping, PlayerLowHunger,
public enum ScenarioCondition
{
    None = 1 << 1,
    PlayerProximity = 1 << 2,
    PlayerOutside = 1 << 3,
    PlayerInside = 1 << 4,
    DuringDay = 1 << 5,
    DuringNight = 1 << 6,
    HighTide = 1 << 7,
    LowTide = 1 << 8,
    DuringDayAndInside = PlayerInside | DuringDay,
    DuringNightAndInside = PlayerInside | DuringNight,
    DuringDayAndOutside = PlayerOutside | DuringDay,
    DuringNightAndOutside = PlayerOutside | DuringNight,
}