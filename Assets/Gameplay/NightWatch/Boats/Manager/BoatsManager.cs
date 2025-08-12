using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightHouse.Game.Nightwatch;
using LightHouse.Game.DayNightSystem;
using LightHouse.Weather;

namespace LightHouse.Game.Boats
{
    /// <summary>
    /// Gère le cycle de vie des bateaux: planification des spawns, instanciation,
    /// gestion des anomalies selon la météo, et destruction en fin de parcours / changement de segment.
    /// </summary>
    public class BoatsManager : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Boat Settings")]
        [SerializeField] private Boat _boatPrefab;
        [SerializeField] private BoatAnomaliesDatabase _anomaliesDatabase;
        [SerializeField] private AnomalyConfiguration[] _anomalyConfigsByWeather;
        [SerializeField] private BoatAnomalyDefinition[] _anomalyDefinitions;

        [Header("Spawn Settings")]
        [SerializeField] private SO_NightWatchConfiguration _nightWatchConfig;
        [SerializeField, Min(0)] private byte _minBoatsSpawnDuringNight = 2;
        [SerializeField, Min(0)] private byte _maxBoatsSpawnDuringNight = 4;

        [Header("Time Settings")]
        [SerializeField] private bool _despawnAllBoatsOnMorning = false;

        [Header("Anomalies")]
        [Tooltip("Probabilité (0..1) qu'un bateau spawné reçoive une anomalie.")]
        [Range(0f, 1f)]
        [SerializeField] private float _anomalyChance = 0.3f;

        #endregion

        #region Private State

        // Bateaux instanciés par ce manager
        private readonly List<Boat> _spawnedBoats = new();

        // Planning des heures de spawn (heures "game time")
        private readonly List<float> _scheduledSpawnTimes = new();

        // Index du prochain spawn prévu
        private int _nextSpawnIndex = 0;

        // Indique si la planification a été faite pour la nuit en cours
        private bool _hasScheduledToday = false;

        // Délégués stockés pour désabonnements propres
        private readonly Dictionary<Boat, Action> _onAnomalyResolvedHandlers = new();
        private readonly Dictionary<Boat, Action> _onProgressEndedHandlers = new();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            BoatsHandlerData.Boats.Clear();
            TimeHandlerData.OnTimeSegmentChanged += OnTimeSegmentChanged;
        }

        private void OnDestroy()
        {
            TimeHandlerData.OnTimeSegmentChanged -= OnTimeSegmentChanged;
            UnsubscribeAndClearAllBoats();
            _anomaliesDatabase.ResetAnomalies();
            BoatsHandlerData.Boats.Clear();
        }

        private void Start()
        {
            // Si on démarre déjà dans la fenêtre de spawn, planifie tout de suite
            if (TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime,
                                          _nightWatchConfig.BoatsSpawnStartHour,
                                          _nightWatchConfig.BoatsSpawnEndHour))
            {
                ScheduleNightSpawns();
                _hasScheduledToday = true;
            }
        }

        private void Update()
        {
            float now = TimeHandlerData.CurrentTime;

            // Fenêtre ouverte mais pas encore planifiée : planifier
            if (!_hasScheduledToday &&
                TimeUtility.IsTimeInRange(now,
                                          _nightWatchConfig.BoatsSpawnStartHour,
                                          _nightWatchConfig.BoatsSpawnEndHour))
            {
                ScheduleNightSpawns();
                _hasScheduledToday = true;
            }

            // Rien à faire ?
            if (_scheduledSpawnTimes.Count == 0 || _nextSpawnIndex >= _scheduledSpawnTimes.Count)
                return;

            // A-t-on atteint l'heure du prochain spawn ?
            if (HasReachedHour(now, _scheduledSpawnTimes[_nextSpawnIndex]))
            {
                SpawnBoat();
                _nextSpawnIndex++;
            }
        }

        #endregion

        #region Scheduling

        /// <summary>
        /// Calcule la liste des heures (triées) auxquelles faire apparaître des bateaux pour la nuit courante.
        /// </summary>
        private void ScheduleNightSpawns()
        {
            _scheduledSpawnTimes.Clear();
            _nextSpawnIndex = 0;

            int count = UnityEngine.Random.Range(_minBoatsSpawnDuringNight, _maxBoatsSpawnDuringNight + 1);

            for (int i = 0; i < count; i++)
            {
                float h = RandomHourInWindow(_nightWatchConfig.BoatsSpawnStartHour,
                                             _nightWatchConfig.BoatsSpawnEndHour);
                _scheduledSpawnTimes.Add(h);
            }

            _scheduledSpawnTimes.Sort(CompareTimeOfDay);
        }

        #endregion

        #region Spawning & Despawning

        /// <summary>
        /// Instancie un bateau, configure ses événements et tente d'y ajouter une anomalie.
        /// </summary>
        private void SpawnBoat()
        {
            var boat = Instantiate(_boatPrefab);

            // Essai d'ajout d'anomalie selon météo/paramètres
            TryAttachRandomAnomaly(boat);

            // Référencement
            _spawnedBoats.Add(boat);
            BoatsHandlerData.Boats.Add(boat);

            // Abonnements (on stocke les délégués pour bien se désabonner plus tard)
            Action resolvedHandler = () => OnBoatAnomalyResolved(boat);
            Action progressEndedHandler = () => OnBoatProgressEnded(boat);

            boat.AnomalyController.OnAnomalyResolved += resolvedHandler;
            boat.OnBoatProgressEnded += progressEndedHandler;

            _onAnomalyResolvedHandlers[boat] = resolvedHandler;
            _onProgressEndedHandlers[boat] = progressEndedHandler;
        }

        /// <summary>
        /// Désabonne tous les bateaux et détruit leurs GameObjects.
        /// </summary>
        private void DespawnAllBoats()
        {
            foreach (var boat in _spawnedBoats)
            {
                if (boat == null) continue;
                UnsubscribeBoat(boat);
                Destroy(boat.gameObject);
            }

            _spawnedBoats.Clear();
            BoatsHandlerData.Boats.Clear();
        }

        /// <summary>
        /// Nettoie correctement tous les bateaux (désabonnements + listes).
        /// </summary>
        private void UnsubscribeAndClearAllBoats()
        {
            foreach (var boat in _spawnedBoats)
            {
                if (boat == null) continue;
                UnsubscribeBoat(boat);
            }

            _spawnedBoats.Clear();
            _onAnomalyResolvedHandlers.Clear();
            _onProgressEndedHandlers.Clear();
        }

        /// <summary>
        /// Désabonne proprement les handlers enregistrés pour ce bateau.
        /// </summary>
        private void UnsubscribeBoat(Boat boat)
        {
            if (boat == null) return;

            if (_onAnomalyResolvedHandlers.TryGetValue(boat, out var resolvedHandler))
            {
                boat.AnomalyController.OnAnomalyResolved -= resolvedHandler;
                _onAnomalyResolvedHandlers.Remove(boat);
            }

            if (_onProgressEndedHandlers.TryGetValue(boat, out var progressHandler))
            {
                boat.OnBoatProgressEnded -= progressHandler;
                _onProgressEndedHandlers.Remove(boat);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Appelé quand le segment temporel change (matin, etc.).
        /// </summary>
        private void OnTimeSegmentChanged(TimeOfDaySegment segment)
        {
            if (segment == TimeOfDaySegment.Morning)
            {
                _hasScheduledToday = false;
                _scheduledSpawnTimes.Clear();
                _nextSpawnIndex = 0;

                if (_despawnAllBoatsOnMorning)
                    DespawnAllBoats();
            }
        }

        /// <summary>
        /// Appelé quand l'anomalie d'un bateau est résolue.
        /// </summary>
        private void OnBoatAnomalyResolved(Boat boat)
        {
            // Important : se désabonner via les délégués stockés
            UnsubscribeBoat(boat);

            // Côté "monde de données" (base anomalies bateaux)
            _anomaliesDatabase.RemoveAnomaly(boat.Data.Name);
        }

        /// <summary>
        /// Appelé quand un bateau termine son parcours.
        /// </summary>
        private void OnBoatProgressEnded(Boat boat)
        {
            UnsubscribeBoat(boat);

            BoatsHandlerData.Boats.Remove(boat);
            _spawnedBoats.Remove(boat);

            if (boat != null)
                Destroy(boat.gameObject);
        }

        #endregion

        #region Time Helpers

        private float RandomHourInWindow(float start, float end)
        {
            if (end > start)
            {
                return UnityEngine.Random.Range(start, end);
            }
            else
            {
                // Fenêtre qui traverse minuit
                float span1 = 24f - start;
                float span2 = end;
                float total = span1 + span2;
                float pick = UnityEngine.Random.Range(0f, total);
                return (pick < span1) ? (start + pick) : (pick - span1);
            }
        }

        private bool HasReachedHour(float now, float target) =>
            TimeUtility.HasReachedHour(now, target, _nightWatchConfig.BoatsSpawnEndHour);

        private int CompareTimeOfDay(float a, float b) =>
            TimeUtility.CompareTimeOfDay(a, b, _nightWatchConfig.BoatsSpawnStartHour, _nightWatchConfig.BoatsSpawnEndHour);

        #endregion

        #region Anomaly Management

        /// <summary>
        /// Attache éventuellement une anomalie aléatoire au bateau selon la météo et la probabilité configurée.
        /// </summary>
        private void TryAttachRandomAnomaly(Boat boat)
        {
            if (UnityEngine.Random.value > _anomalyChance) return;
            if (WeatherHandlerData.CurrentWeather == null) return;

            // Trouver la configuration correspondant à la météo courante
            var config = _anomalyConfigsByWeather.FirstOrDefault(c =>
                c.weatherType == WeatherHandlerData.CurrentWeather.WeatherType);

            if (config == null) return;

            var picked = config.PickRandomAnomaly();
            if (!picked.HasValue)
                return;

            var def = GetAnomalyDefinition(picked.Value);
            if (def == null)
            {
                Debug.LogWarning($"[BoatsManager] Aucun prefab défini pour l'anomalie: {picked.Value}");
                return;
            }

            boat.AnomalyController.AddAnomaly(def);

            // Enregistrement côté base (affichage, report, etc.)
            _anomaliesDatabase.SetAnomaly(boat.Data.Name, def.Type, $"{boat.RadioFrequency} MHz");
        }

        /// <summary>
        /// Retourne la définition d'anomalie correspondant au type donné.
        /// </summary>
        private BoatAnomalyDefinition GetAnomalyDefinition(AnomalyType type) =>
            _anomalyDefinitions.FirstOrDefault(d => d.Type == type);

        #endregion
    }
}
