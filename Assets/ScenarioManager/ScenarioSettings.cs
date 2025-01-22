using UnityEngine;

[CreateAssetMenu(menuName = "Scenario/Settings")]
public class ScenarioSettings : ScriptableObject
{
    [Range(0, 100), Tooltip("Le % du jeu à laquelle le dernier évènement peut se lancer, 90 = à 90% du temps de jeu total")]
    public float LastEventMaxTimePercent = 90f;

    [Range(0, 100), Tooltip("Le % auquel le premier élément doit se lancer au minimum par rapport au temps de base")]
    public float MinForkEventTime = 20f;  
    
    [Range(0, 100), Tooltip("Le % auquel le premier élément doit se lancer au maximum par rapport au temps de base")]
    public float MaxForkEventTime = 20f;
}