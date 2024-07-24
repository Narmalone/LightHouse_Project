using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private List<ActEvents> actScenarios = new List<ActEvents>();
    
    [SerializeField] private ActEvents currentFollowedAct;
    [SerializeField]
    private List<ScenarioEvent> scenariosFromAct = new List<ScenarioEvent>();
    [SerializeField]
    private ScenarioEvent nextEvent;

    private void Awake()
    {
        currentFollowedAct = RandomAct();
        scenariosFromAct = new List<ScenarioEvent>(currentFollowedAct.possibleEvents);
        SelectNextEvent();
    }

    public void SelectNextEvent()
    {
        if(scenariosFromAct.Count <= 0)
        {
            Debug.Log($"il n'y a plus d'events restant dans {currentFollowedAct.name}");
            return;
        }
        nextEvent = GetRandomEventByAct(currentFollowedAct);
        scenariosFromAct.Remove(nextEvent);
    }

    public void PlayNextEvent()
    {
        nextEvent?.Play();
        SelectNextEvent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayNextEvent();
        }
    }

    private ActEvents RandomAct()
    {
        return actScenarios[Random.Range(0, actScenarios.Count)];
    }

    private ScenarioEvent GetRandomEventByAct(ActEvents act)
    {
        if (act.possibleEvents.Count <= 0) return null;
        var rdm = Random.Range(0, act.possibleEvents.Count);
        var evt = act.possibleEvents[rdm];
        return evt;
    }
}
