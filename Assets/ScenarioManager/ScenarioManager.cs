using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScenarioManager : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private List<ThemeScenario> themes = new List<ThemeScenario>();
    [SerializeField] private GameSettings _gameSettings;
    [SerializeField] private ScenarioSettings _scenarioSettings;

    [Header("AUTO LINKED REFS")]
    [SerializeField] private ThemeScenario currentFollowedTheme;
    [SerializeField] private List<ActEvents> actScenarios = new List<ActEvents>();    
    [SerializeField] public List<ScenarioEvent> scenariosFromAct = new List<ScenarioEvent>();

    [Header("ACT INFOS")]
    [SerializeField] private int currentActIndex = 0;
    [SerializeField] private ActEvents currentAct;

    [Header("EVT INFOS")]
    [SerializeField] private ScenarioEvent nextEvent;

    private float _currentTimerToNextEvent;
    private int _currentEventTimeIndex = 0;

    private void Awake()
    {
        currentFollowedTheme = RandomTheme();
        actScenarios = GetActsByTheme(currentFollowedTheme);
        SelectNextAct();
        DivideEvents(_scenarioSettings.MinForkEventTime, _scenarioSettings.MaxForkEventTime, _scenarioSettings.LastEventMaxTimePercent);
    }

    /// <summary>
    /// On va diviser de manière égale le temps de jeu total  pour pouvoir y placer tous les évènements
    /// par la suite on va ajouter une fourchette aléatoire, pour ne pas que ce soit trop répétitif par rapport
    /// à ce temps égal pour obtenir quelque chose de cohérent
    /// </summary>
    private List<float> DivideEvents(float minPercentage = 20f, float maxPercentage = 20f, float maxLastEventAtGameTimePercent = 90f)
    {
        List<float> result = new List<float>();
        int totalEventsToPlay = 0;

        // Calcul du nombre total d'événements à jouer
        for (int i = 0; i < actScenarios.Count; i++)
        {
            totalEventsToPlay += actScenarios[i].NumberOfEventsToPlayInAct;
        }

        if (totalEventsToPlay > 0)
        {
            float totalGameDuration = _gameSettings.GetTotalGameDurationInSeconds(); // Durée totale du jeu en secondes
            float maxEventTime = totalGameDuration * maxLastEventAtGameTimePercent / 100f; // Le dernier événement doit se déclencher avant 90 % de la durée totale
            float intervalBetweenEvents = maxEventTime / totalEventsToPlay; // Intervalle entre chaque événement
            // Convertir les pourcentages en facteurs multiplicateurs (par exemple, 20f devient 0.2f)
            float minFactor = 1f - (minPercentage / 100f);
            float maxFactor = 1f + (maxPercentage / 100f);
            Debug.Log(totalGameDuration);

            // Planification des événements
            for (int i = 0; i < totalEventsToPlay; i++)
            {
                // Temps prévu pour cet événement
                float baseTime = intervalBetweenEvents * (i + 1);

                // Calculer la fourchette en fonction des pourcentages paramétrables
                float minTime = baseTime * minFactor;
                float maxTime = baseTime * maxFactor;

                // Limiter le dernier événement pour qu'il ne soit pas trop tard
                if (i == totalEventsToPlay - 1 && maxTime > maxEventTime)
                {
                    maxTime = maxEventTime;
                }

                //Debug.Log(baseTime);

                // Choisir un temps aléatoire dans la fourchette
                float eventTime = Random.Range(minTime, maxTime);

                result.Add(eventTime);
                Debug.Log($"L'événement {i + 1} devrait démarrer entre {minTime:F2} et {maxTime:F2} secondes. Temps choisi : {eventTime:F2} secondes");

                // Si tu veux réellement démarrer l'événement, tu peux appeler une fonction ici :
                // StartEventAtTime(eventTime);
            }
            SelectNextEvent();
        }
        else
        {
            Debug.LogWarning("Aucun événement à jouer !");
        }
        return result;
    }

    private void StartEventAtTime(float eventTime)
    {
        // Logique pour déclencher l'événement à `eventTime` secondes
        Debug.Log("Déclenchement de l'événement à " + eventTime + " secondes");
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
