using LightHouse.Game.DayNightSystem;
using System;
using UnityEngine;

/// <summary>Etapes du cycle d’un shipment.</summary>
public enum ShipmentPhase
{
    Preparing,              // Phase 1 : attente lead initial (+ éventuel retard météo)
    WaitingDispatchWindow,  // Phase 2 : attente du 09:00 du jour suivant
    Completed               // Livraison effectuée
}

/// <summary>
/// Automate minimaliste en 2 phases :
///  - Phase 1 : attendre LEAD (ex: 48h in-game). Si retard météo → on enchaîne un second countdown (ex: +24h).
///    → Juste AVANT d'ajouter ce retard, on invoke OnInitialShipmentDelayCompleted.
///  - Phase 2 : quand la phase 1 est finie, on attend le 09:00 du JOUR SUIVANT, puis on livre.
/// </summary>
public sealed class ShipmentSystem
{
    #region Fields / Ctor

    private readonly TimeConfiguration _cfg;
    private readonly float _dispatchHour;

    private readonly float _leadHours;          // ex. 48h in-game
    private readonly bool _shouldBeDelayed;    // météo défavorable ?
    private readonly float _extraDelayHours;    // ex. 24h in-game si delayed

    private bool _firedInitialDelayEvent;       // pour ne pas ré-invoquer l’event

    public uint TicketNumber { get; set; }

    public ShipmentPhase Phase { get; private set; } = ShipmentPhase.Preparing;

    /// <summary>Temps RÉEL restant dans la phase courante (en secondes réelles).</summary>
    public float RemainingSeconds { get; private set; }

    /// <summary>
    /// Temps RESTANT exprimé en HEURES **de jeu** (conversion depuis RemainingSeconds).
    /// Utile pour l’UI (“< 24 h” vs “> 1 j”), les tooltips, etc.
    /// </summary>
    public float RemainingGameHours
        => _cfg.RealSecondsToGameHours(Mathf.Max(0f, RemainingSeconds));

    /// <summary>
    /// Event déclenché à la FIN du lead initial SI le shipment est delayed (avant d’ajouter le retard).
    /// Idéal pour prévenir l’UI / envoyer un mail “retard confirmé”.
    /// </summary>
    public event Action OnInitialShipmentDelayCompleted;

    /// <summary>Event déclenché quand on passe en attente du 09:00 (fin de la phase 1).</summary>
    public event Action OnPrepared;

    /// <summary>Event déclenché quand la livraison est effective (09:00 du jour suivant atteint).</summary>
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

        // On démarre par le lead initial uniquement.
        RemainingSeconds = _cfg.GameHoursToRealSeconds(_leadHours);
        Phase = ShipmentPhase.Preparing;
        TicketNumber = ticketNumber;
    }

    #endregion

    #region Public API

    /// <summary>A appeler chaque frame avec Time.deltaTime (secondes réelles).</summary>
    public void Tick(float deltaRealSeconds)
    {
        if (Phase == ShipmentPhase.Completed || deltaRealSeconds <= 0f)
            return;

        RemainingSeconds -= deltaRealSeconds;
        if (RemainingSeconds > 0f) return;

        switch (Phase)
        {
            case ShipmentPhase.Preparing:
                // 1) Le lead initial vient de s’achever.
                //    Si le shipment est delayed, prévenir AVANT d’ajouter le retard.
                if (_shouldBeDelayed && !_firedInitialDelayEvent)
                {
                    _firedInitialDelayEvent = true;
                    OnInitialShipmentDelayCompleted?.Invoke();

                    // Démarre le compte à rebours du retard, puis on reste en Preparing.
                    if (_extraDelayHours > 0f)
                    {
                        RemainingSeconds = _cfg.GameHoursToRealSeconds(_extraDelayHours);
                        // Rester en Preparing pour finir ce délai additionnel
                        return;
                    }
                }

                // 2) Ici : soit pas de delay, soit le retard vient de se terminer.
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

    /// <summary>
    /// Calcule (en secondes réelles) le temps à attendre jusqu’au 09:00 **du jour suivant**.
    /// </summary>
    private float ComputeSecondsUntilNextDayDispatch()
    {
        // Delta jusqu’au prochain 09:00 (peut être aujourd’hui si on est avant 09:00).
        float untilNext9 = _cfg.RealSecondsUntilGameTime(_dispatchHour);

        // Exigence “jour suivant” : si on est avant 09:00, on force +1 jour.
        if (TimeHandlerData.CurrentTime < _dispatchHour)
            untilNext9 += _cfg.RealSecondsPerGameDay;

        return untilNext9;
    }

    #endregion
}
