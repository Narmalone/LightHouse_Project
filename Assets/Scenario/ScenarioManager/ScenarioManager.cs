using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScenarioManager : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private float _scenarioSpeedMultiplier = 1.0f;
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

    [Header(" --- LISTENERS --- ")]
    [SerializeField] private CustomEvent_GameZone _onGameZoneChanged;
    [SerializeField] private GameZoneTypeInfo _gameZoneTypeInfo;
    [SerializeField] private CustomEvent_Int _onGlobalDayStateChanged;

    //il nous faut récup un bon paquet d'events, comme lorsque la zone auquel le joueur se situe à changé,
    //lorsque on considère que c'est la nuit / le jour
    //quand il y'a marée haute / marée basse / marée normale ?
    //quand le joueur est proche d'un objet ou alors 

    [Header(" - DEBUG ONLY -")]
    [SerializeField] private List<float> _targetSeconds = new List<float>();
    private bool _chronometerEnabled = true;
    [SerializeField] private float _currentChronoToNextEvent = 0f;
    [SerializeField] private int _currentEventTimeIndex = 0;
    [SerializeField] private int _numberOfEventsPlayedInAct = 0;

    public ScenarioCondition CurrentConditions;

    #region MONO CALLBACKS
    private void Awake()
    {
        SubscribeToEvents();
        currentFollowedTheme = GetRandomTheme();
        actScenarios = GetActsByTheme(currentFollowedTheme);
        SelectNextAct();
        _targetSeconds = DivideEvents(_scenarioSettings.MinForkEventTime, _scenarioSettings.MaxForkEventTime, _scenarioSettings.LastEventMaxTimePercent);
    }

    private void Update()
    {
        if (!_chronometerEnabled) return;

        _currentChronoToNextEvent += _scenarioSpeedMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime;

        if (_currentChronoToNextEvent >= _targetSeconds[_currentEventTimeIndex])
        {
            _currentEventTimeIndex += 1;
            if (_currentEventTimeIndex >= _targetSeconds.Count)
            {
                _currentChronoToNextEvent = 0f;
                _chronometerEnabled = false;
            }
            PlayNextEvent();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeToEvents();
    }
    #endregion

    #region DELEGATES

    private void SubscribeToEvents()
    {
        _onGameZoneChanged.handle += OnGameZoneChanged;
        _onGlobalDayStateChanged.handle += OnGlobalDayStateChanged;
    }

    private void UnsubscribeToEvents()
    {
        _onGameZoneChanged.handle -= OnGameZoneChanged;
        _onGlobalDayStateChanged.handle -= OnGlobalDayStateChanged;
    }
    private void OnGlobalDayStateChanged(int obj)
    {
        switch ((DayNightManager.GlobalDayState)obj)
        {
            case DayNightManager.GlobalDayState.DAY:

                if (CurrentConditions.HasFlag(ScenarioCondition.DuringNight))
                {
                    CurrentConditions &= ~ScenarioCondition.DuringNight;
                }

                if (!CurrentConditions.HasFlag(ScenarioCondition.DuringDay))
                {
                    CurrentConditions |= ScenarioCondition.DuringDay;
                }

                return;
            case DayNightManager.GlobalDayState.NIGHT:

                if (CurrentConditions.HasFlag(ScenarioCondition.DuringDay))
                {
                    CurrentConditions &= ~ScenarioCondition.DuringDay;
                }

                if (!CurrentConditions.HasFlag(ScenarioCondition.DuringNight))
                {
                    CurrentConditions |= ScenarioCondition.DuringNight;
                }

                return;
        }
    }

    private void OnGameZoneChanged(GameZone zone)
    {
        if (_gameZoneTypeInfo.OutsideZones.Contains(zone))
        {
            if (CurrentConditions.HasFlag(ScenarioCondition.PlayerInside))
            {
                CurrentConditions &= ~ScenarioCondition.PlayerInside;
            }

            if (!CurrentConditions.HasFlag(ScenarioCondition.PlayerOutside))
            {
                CurrentConditions |= ScenarioCondition.PlayerOutside;
            }
        }
        else if (_gameZoneTypeInfo.InsideZones.Contains(zone))
        {
            if (CurrentConditions.HasFlag(ScenarioCondition.PlayerOutside))
            {
                CurrentConditions &= ~ScenarioCondition.PlayerOutside;
            }

            if (!CurrentConditions.HasFlag(ScenarioCondition.PlayerInside))
            {
                CurrentConditions |= ScenarioCondition.PlayerInside;
            }
        }
    }

    #endregion

    public ScenarioEvent GetRandomEventByConditions(ScenarioCondition conditionToCompare)
    {
        if (_numberOfEventsPlayedInAct >= currentAct.NumberOfEventsToPlayInAct)
        {
            // Passer à l'acte suivant
            SelectNextAct();
            _numberOfEventsPlayedInAct = 0;
        }

        if (scenariosFromAct.Count <= 0)
        {
            Debug.Log($"Il n'y a plus d'événements restants dans {currentAct.name}");
            return null;
        }

        // Filtrer les événements selon les conditions
        List<ScenarioEvent> eligibleEvents = scenariosFromAct.FindAll(e => e.AreConditionsValid(conditionToCompare));
        ScenarioEvent nextEvent = null;

        Debug.Log("Nombre d'évènements éligibles " + eligibleEvents.Count);
        if (eligibleEvents.Count > 0)
        {
            // Sélectionner un événement au hasard parmi ceux qui remplissent les conditions
            nextEvent = GetRandomEventByEvents(eligibleEvents);
            scenariosFromAct.Remove(nextEvent);
        }
        else
        {
            Debug.Log("Aucun événement ne correspond aux conditions actuelles, prise d'un évènement aléatoire sans prendre en" +
                "compte les conditions.");
            nextEvent = GetRandomEventByEvents(scenariosFromAct);
        }
        return nextEvent;
    }

    #region THEMES
    private ThemeScenario GetRandomTheme()
    {
        return themes[Random.Range(0, themes.Count)];
    }

    #endregion

    #region ACTS
    private List<ActEvents> GetActsByTheme(ThemeScenario theme)
    {
        return theme.actEvents;
    }

    private void SelectNextAct()
    {
        if (currentActIndex >= actScenarios.Count - 1)
        {
            Debug.Log("pas de prochain act");
            return;
        }
        currentActIndex++;
        currentAct = actScenarios[currentActIndex];
        scenariosFromAct = new List<ScenarioEvent>(currentAct.possibleEvents);
    }

    #endregion

    #region EVENTS
    private ScenarioEvent GetRandomEventByEvents(List<ScenarioEvent> mainEvents)
    {
        if (mainEvents.Count <= 0) return null;
        var rdm = Random.Range(0, mainEvents.Count);
        var evt = mainEvents[rdm];
        return evt;
    }
    /// <summary>
    /// Doit êtres appelé limite juste avant d'être jouer pour vérifier les condition
    /// par ex: ne pas faire spawn d'event sur la même zone que le joueur
    /// </summary>
    public void SelectNextEvent()
    {
        if (_numberOfEventsPlayedInAct >= currentAct.NumberOfEventsToPlayInAct)
        {
            //passer à l'acte suivant
            SelectNextAct();
            _numberOfEventsPlayedInAct = 0;
        }
        if (scenariosFromAct.Count <= 0)
        {
            Debug.Log($"il n'y a plus d'events restant dans {currentAct.name}");
            return;
        }
        //old
        //nextEvent = GetRandomEventByEvents(scenariosFromAct);
        nextEvent = GetRandomEventByConditions(CurrentConditions);
        scenariosFromAct.Remove(nextEvent);
    }

    public void PlayNextEvent()
    {
        SelectNextEvent();
        nextEvent?.Play();
        _numberOfEventsPlayedInAct++;
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

            // Calcul d'un écart fixe en fonction du pourcentage appliqué à l'intervalle
            float minOffset = intervalBetweenEvents * (minPercentage / 100f);
            float maxOffset = intervalBetweenEvents * (maxPercentage / 100f);

            float previousMaxTime = 0f; // Pour suivre le maxTime du précédent événement

            // Planification des événements
            for (int i = 0; i < totalEventsToPlay; i++)
            {
                // Temps prévu pour cet événement
                float baseTime = intervalBetweenEvents * (i + 1);

                // Calcul des bornes de la fourchette
                float minTime = Mathf.Max(baseTime - minOffset, previousMaxTime + 1f); // Respecter un espacement d'au moins 1 seconde
                float maxTime = Mathf.Min(baseTime + maxOffset, maxEventTime); // Ne pas dépasser le temps maximum autorisé

                // Validation des bornes
                if (maxTime <= minTime)
                {
                    Debug.LogWarning($"La fourchette pour l'événement {i + 1} est invalide. Ajustement en cours.");
                    maxTime = minTime + 1f; // Ajouter un espacement minimal
                }

                // Choisir un temps aléatoire dans la fourchette
                float eventTime = Random.Range(minTime, maxTime);

                result.Add(eventTime);
                //Debug.Log($"L'événement {i + 1} devrait démarrer entre {minTime:F2} et {maxTime:F2} secondes. Temps choisi : {eventTime:F2} secondes");

                // Mettre à jour le maxTime précédent
                previousMaxTime = maxTime;
            }
        }
        else
        {
            Debug.LogWarning("Aucun événement à jouer !");
        }
        return result;
    }
    #endregion
}
