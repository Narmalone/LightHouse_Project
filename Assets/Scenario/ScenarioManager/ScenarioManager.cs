using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScenarioManager : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private float _scenarioSpeedMultiplier = 1.0f;
    [SerializeField] private List<ThemeScenario> _availableThemes = new List<ThemeScenario>();
    [SerializeField] private GameSettings _gameSettings;
    [SerializeField] private ScenarioSettings _scenarioSettings;

    [Header("AUTO LINKED REFS")]
    [SerializeField] private ThemeScenario _currentFollowedTheme;
    [SerializeField] private List<ActEvents> _actsFromTheme = new List<ActEvents>();    
    [SerializeField] public List<ScenarioEvent> _scenarioEventsFromAct = new List<ScenarioEvent>();

    [Header("ACT INFOS")]
    [SerializeField] private int _currentActIndex = -1;
    [SerializeField] private ActEvents _currentAct;

    [Header("EVT INFOS")]
    [SerializeField] private ScenarioEvent _nextEvent;

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
    [SerializeField] private List<float> _actsTargetSeconds = new List<float>();
    [SerializeField] private float _currentChronoToNextAct = 0f;
    [SerializeField] private int _currentActTimeIndex = 0;
    private bool _eventsChronometerEnabled = true;
    private bool _actChronometerEnabled = true;
    [SerializeField] private float _currentChronoToNextEvent = 0f;
    [SerializeField] private int _currentEventTimeIndex = 0;
    [SerializeField] private int _numberOfEventsPlayedInAct = 0;

    public ScenarioCondition CurrentConditions;

    #region MONO CALLBACKS
    private void Awake()
    {
        _currentActIndex = -1;
        SubscribeToEvents();
        _currentFollowedTheme = GetRandomTheme();
        _actsFromTheme = GetActsByTheme(_currentFollowedTheme);
        _targetSeconds = DivideEvents(_gameSettings, _actsFromTheme, _scenarioSettings.MinForkEventTime, _scenarioSettings.MaxForkEventTime, _scenarioSettings.LastEventMaxTimePercent);
        _actsTargetSeconds = DivideActsByEvents(_gameSettings, _actsFromTheme, _targetSeconds);
    }

    private void Update()
    {
        if (_eventsChronometerEnabled)
        {
            _currentChronoToNextEvent += _scenarioSpeedMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime;

            if (_currentChronoToNextEvent >= _targetSeconds[_currentEventTimeIndex])
            {
                _currentEventTimeIndex += 1;
                if (_currentEventTimeIndex >= _targetSeconds.Count)
                {
                    _eventsChronometerEnabled = false;
                }
                PlayNextEvent();
            }
        }

        if (_actChronometerEnabled)
        {
            _currentChronoToNextAct += _scenarioSpeedMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime;
            if (_currentChronoToNextAct >= _actsTargetSeconds[_currentActTimeIndex])
            {
                _currentActTimeIndex += 1;
                if (_currentActTimeIndex >= _actsTargetSeconds.Count)
                {
                    _actChronometerEnabled = false;
                }
                PlayNextAct();
            }
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

    #region THEMES
    private ThemeScenario GetRandomTheme()
    {
        return _availableThemes[Random.Range(0, _availableThemes.Count)];
    }

    #endregion

    #region ACTS
    private List<ActEvents> GetActsByTheme(ThemeScenario theme)
    {
        return theme.actEvents;
    }

    private ActEvents SelectNextAct(List<ActEvents> actsFromTheme)
    {
        if (_currentActIndex >= actsFromTheme.Count - 1)
        {
            Debug.Log("pas de prochain act");
            return null;
        }
        _currentActIndex++;
        ActEvents currAct = actsFromTheme[_currentActIndex];
        _scenarioEventsFromAct = new List<ScenarioEvent>(currAct.possibleEvents);
        return currAct;
    }

    private List<float> DivideActsByEvents(GameSettings gameSettings, List<ActEvents> actsEvents, List<float> targetSeconds)
    {
        List<float> list = new List<float>();

        float totalGameDuration = gameSettings.GetTotalGameDurationInSeconds(); // Durée totale du jeu en secondes
        int totalSteps = 0;
        float playActTransitionAtTime = 0f;
        float lastEventInCurrentAct = 0f;
        float nextEventInAct = 0f;
        float differenceBetweenLastAndNext = 0f;

        for (int i = 0; i < actsEvents.Count; i++)
        {
            if(i == 0)
            {
                totalSteps = 0;
                playActTransitionAtTime = targetSeconds[0] / 2;
                //Debug.Log($"Temps depuis le départ du jeu 0s et temps du premier évènement" +
                //$" du prochain acte {targetSeconds[i]} la transition de l'acte {i} à {i + 1} se joue à: {playActTransitionAtTime}");
            }
            else
            {
                totalSteps += actsEvents[i].NumberOfEventsToPlayInAct; //on enlève 1 car on veut un index et pas un count
                lastEventInCurrentAct = targetSeconds[totalSteps - 1]; //dernier event de son acte
                nextEventInAct = targetSeconds[totalSteps]; //premier event de l'acte suivant
                differenceBetweenLastAndNext = nextEventInAct - lastEventInCurrentAct;
                playActTransitionAtTime = lastEventInCurrentAct + (differenceBetweenLastAndNext / 2);
                //Debug.Log($"Temps du dernier évènement de l'acte {i} à {lastEventInCurrentAct}, temps du premier évènement" +
                //$" du prochain acteà  {nextEventInAct} la transition de l'acte {i} à {i + 1} se joue à: {playActTransitionAtTime}");
            }

            list.Add(playActTransitionAtTime);

            
        }

        return list;
    }

    public void PlayNextAct()
    {
        _currentAct = SelectNextAct(_actsFromTheme);
        _currentAct?.Play();
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"ActItem_{_currentActIndex}";
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

    public ScenarioEvent GetRandomEventByConditions(ScenarioCondition conditionToCompare)
    {
        if (_scenarioEventsFromAct.Count <= 0)
        {
            Debug.Log($"Il n'y a plus d'événements restants dans {_currentAct.name}");
            return null;
        }

        // Filtrer les événements selon les conditions
        List<ScenarioEvent> eligibleEvents = _scenarioEventsFromAct.FindAll(e => e.AreConditionsValid(conditionToCompare));
        ScenarioEvent nextEvent = null;

        Debug.Log("Nombre d'évènements éligibles " + eligibleEvents.Count);
        if (eligibleEvents.Count > 0)
        {
            // Sélectionner un événement au hasard parmi ceux qui remplissent les conditions
            nextEvent = GetRandomEventByEvents(eligibleEvents);
        }
        else
        {
            Debug.Log("Aucun événement ne correspond aux conditions actuelles, prise d'un évènement aléatoire sans prendre en" +
                "compte les conditions.");
            nextEvent = GetRandomEventByEvents(_scenarioEventsFromAct);
        }
        _scenarioEventsFromAct.Remove(nextEvent);
        return nextEvent;
    }

    /// <summary>
    /// Doit êtres appelé limite juste avant d'être jouer pour vérifier les condition
    /// par ex: ne pas faire spawn d'event sur la même zone que le joueur
    /// </summary>
    public ScenarioEvent SelectNextEvent(ActEvents currentAct, ScenarioCondition conditions)
    {
        if (_numberOfEventsPlayedInAct >= currentAct.NumberOfEventsToPlayInAct)
        {
            _numberOfEventsPlayedInAct = 0;
        }

        if (_scenarioEventsFromAct.Count <= 0)
        {
            Debug.Log($"il n'y a plus d'events restant dans {currentAct.name}");
            return null;
        }

        ScenarioEvent nextEvent = GetRandomEventByConditions(conditions);
        _scenarioEventsFromAct.Remove(nextEvent);
        return nextEvent;
    }

    public void PlayNextEvent()
    {
        _nextEvent = SelectNextEvent(_currentAct, CurrentConditions);
        _nextEvent?.Play();
        _numberOfEventsPlayedInAct++;
    }

    /// <summary>
    /// On va diviser de manière égale le temps de jeu total  pour pouvoir y placer tous les évènements
    /// par la suite on va ajouter une fourchette aléatoire, pour ne pas que ce soit trop répétitif par rapport
    /// à ce temps égal pour obtenir quelque chose de cohérent
    /// </summary>
    private List<float> DivideEvents(GameSettings gameSettings, List<ActEvents> actsFromTheme, float minPercentage = 20f, float maxPercentage = 20f, float maxLastEventAtGameTimePercent = 90f)
    {
        List<float> result = new List<float>();
        int totalEventsToPlay = 0;

        // Calcul du nombre total d'événements à jouer
        for (int i = 0; i < actsFromTheme.Count; i++)
        {
            totalEventsToPlay += actsFromTheme[i].NumberOfEventsToPlayInAct;
        }

        if (totalEventsToPlay > 0)
        {
            float totalGameDuration = gameSettings.GetTotalGameDurationInSeconds(); // Durée totale du jeu en secondes
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
