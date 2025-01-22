using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private List<ThemeScenario> themes = new List<ThemeScenario>();
    [SerializeField] private GameSettings _gameSettings;

    [Header("AUTO LINKED REFS")]
    [SerializeField] private ThemeScenario currentFollowedTheme;
    [SerializeField] private List<ActEvents> actScenarios = new List<ActEvents>();    
    [SerializeField] private List<ScenarioEvent> scenariosFromAct = new List<ScenarioEvent>();

    [Header("ACT INFOS")]
    [SerializeField] private int currentActIndex = 0;
    [SerializeField] private ActEvents currentAct;

    [Header("EVT INFOS")]
    [SerializeField] private ScenarioEvent nextEvent;

    private void Awake()
    {
        currentFollowedTheme = RandomTheme();
        actScenarios = GetActsByTheme(currentFollowedTheme);
        SelectNextAct();
    }

    private void SelectNextAct()
    {
        if(currentActIndex >= actScenarios.Count - 1)
        {
            Debug.Log("pas de prochain act");
            return;
        }
        currentActIndex++;
        currentAct = actScenarios[currentActIndex];
        scenariosFromAct = new List<ScenarioEvent>(currentAct.possibleEvents);
    }

    /// <summary>
    /// Doit êtres appelé limite juste avant d'être jouer pour vérifier les condition
    /// par ex: ne pas faire spawn d'event sur la même zone que le joueur
    /// </summary>
    public void SelectNextEvent()
    {
        if(scenariosFromAct.Count <= 0)
        {
            Debug.Log($"il n'y a plus d'events restant dans {currentAct.name}");
            return;
        }
        nextEvent = GetRandomEventByEvents(scenariosFromAct);
        scenariosFromAct.Remove(nextEvent);
    }

    public void PlayNextEvent()
    {
        nextEvent?.Play();
        SelectNextEvent();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.V))
        {
            SelectNextAct();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SelectNextEvent();
        }*/
    }

    private ThemeScenario RandomTheme()
    {
        return themes[Random.Range(0, themes.Count)];
    }

    private List<ActEvents> GetActsByTheme(ThemeScenario theme)
    {
        return theme.actEvents;
    }

    private ScenarioEvent GetRandomEventByEvents(List<ScenarioEvent> mainEvents)
    {
        if (mainEvents.Count <= 0) return null;
        var rdm = Random.Range(0, mainEvents.Count);
        var evt = mainEvents[rdm];
        return evt;
    }
}
