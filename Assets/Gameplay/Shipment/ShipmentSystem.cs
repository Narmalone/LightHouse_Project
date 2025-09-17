using LightHouse.Game.DayNightSystem;
using System;
using UnityEngine;


namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>Etapes du cycle d’un shipment.</summary>
    public enum ShipmentPhase
    {
        Preparing,              // Phase 1 : attente lead initial (+ éventuel retard météo)
        WaitingDispatchWindow,  // Phase 2 : attente du 09:00 du jour suivant
        Completed               // Livraison effectuée
    }

    /// <summary>
    /// Automate en 2 phases. Le décompte avance avec le TEMPS DE JEU (et pas les secondes réelles).
    /// </summary>
    public sealed class ShipmentSystem
    {
        #region Fields / Ctor

        private readonly TimeConfiguration _cfg;
        private readonly float _dispatchHour;

        private readonly float _leadHours;        // ex. 48h in-game
        private readonly bool _shouldBeDelayed;  // météo défavorable ?
        private readonly float _extraDelayHours;  // ex. 24h in-game si delayed

        private bool _firedInitialDelayEvent;

        // --- NEW: mémorise l'horodatage absolu en HEURES DE JEU pour dériver le delta in-game
        private float _lastAbsGameHours; // = CurrentDay*24 + CurrentTime

        public uint TicketNumber { get; set; }

        public ShipmentPhase Phase { get; private set; } = ShipmentPhase.Preparing;

        /// <summary>Temps RÉEL restant dans la phase courante (en secondes réelles).</summary>
        public float RemainingSeconds { get; private set; }

        /// <summary>Temps RESTANT exprimé en HEURES de jeu (dérivé de RemainingSeconds).</summary>
        public float RemainingGameHours
            => _cfg.RealSecondsToGameHours(Mathf.Max(0f, RemainingSeconds));

        public event Action OnInitialShipmentDelayCompleted;
        public event Action OnPrepared;
        public event Action OnArrived;

        public ShipmentSystem(TimeConfiguration cfg,
                              float leadTimeHours,
                              bool shouldBeDelayed,
                              float additionalDelayHoursIfDelayed,
                              float dispatchHour = 9f,
                              uint ticketNumber = 0)
        {
            _cfg = cfg;
            _dispatchHour = dispatchHour;
            _leadHours = Mathf.Max(0f, leadTimeHours);
            _shouldBeDelayed = shouldBeDelayed;
            _extraDelayHours = Mathf.Max(0f, additionalDelayHoursIfDelayed);
            TicketNumber = ticketNumber;

            // Phase 1 : on part sur le lead initial (en secondes réelles équivalentes).
            RemainingSeconds = _cfg.GameHoursToRealSeconds(_leadHours);
            Phase = ShipmentPhase.Preparing;

            // NEW: initialise l’horloge absolue in-game
            _lastAbsGameHours = TimeHandlerData.CurrentDay * 24f + TimeHandlerData.CurrentTime;
        }

        #endregion

        #region Public API

        /// <summary>
        /// A appeler chaque frame (comme avant) : la méthode ignore les secondes réelles
        /// et consomme à la place le DELTA d’heures de jeu (via TimeHandlerData).
        /// </summary>
        public void Tick(float _ /* deltaRealSeconds (ignoré) */)
        {
            if (Phase == ShipmentPhase.Completed)
                return;

            // --- NEW: calcule le delta d'heures IN-GAME écoulées depuis le dernier tick
            float absNow = TimeHandlerData.CurrentDay * 24f + TimeHandlerData.CurrentTime;
            float deltaGameHours = Mathf.Max(0f, absNow - _lastAbsGameHours);
            _lastAbsGameHours = absNow;

            if (deltaGameHours <= 0f)
                return;
            //
            // Convertit ce delta d'heures de jeu en secondes réelles équivalentes
            float deltaRealFromGame = deltaGameHours * _cfg.RealSecondsPerGameHour;

            RemainingSeconds -= deltaRealFromGame;
            if (RemainingSeconds > 0f) return;

            switch (Phase)
            {
                case ShipmentPhase.Preparing:
                    // Fin du lead initial
                    if (_shouldBeDelayed && !_firedInitialDelayEvent)
                    {
                        _firedInitialDelayEvent = true;
                        OnInitialShipmentDelayCompleted?.Invoke();

                        if (_extraDelayHours > 0f)
                        {
                            // Enchaîne le retard (toujours en Preparing)
                            RemainingSeconds = _cfg.GameHoursToRealSeconds(_extraDelayHours);
                            return;
                        }
                    }

                    // Pas de retard ou bien retard terminé → attendre 09:00 du jour SUIVANT
                    RemainingSeconds = ComputeSecondsUntilNextDayDispatch();
                    Phase = ShipmentPhase.WaitingDispatchWindow;
                    OnPrepared?.Invoke();
                    break;

                case ShipmentPhase.WaitingDispatchWindow:
                    // 09:00 du jour suivant atteint → livraison
                    RemainingSeconds = 0f;
                    Phase = ShipmentPhase.Completed;
                    OnArrived?.Invoke();
                    break;
            }
        }

        #endregion

        #region Internals

        /// <summary>Temps réel à attendre jusqu’au 09:00 du JOUR SUIVANT (basé sur l’heure in-game courante).</summary>
        private float ComputeSecondsUntilNextDayDispatch()
        {
            float untilNext9 = _cfg.RealSecondsUntilGameTime(_dispatchHour);
            // Exigence “jour suivant” : si on est avant 09:00, on rajoute +1 jour in-game.
            if (TimeHandlerData.CurrentTime < _dispatchHour)
                untilNext9 += _cfg.RealSecondsPerGameDay;
            return untilNext9;
        }

        #endregion
    }

}
