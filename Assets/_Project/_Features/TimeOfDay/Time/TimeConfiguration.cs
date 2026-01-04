using LightHouse.EditorTools.SuperGameManager;
using LightHouse.Game.DayNightSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeConfig_Default", menuName = "LightHouse/Time/New Config")]
public class TimeConfiguration : ScriptableObject
{
    #region ========= Serialized data =========

    [Header("Calendar")]
    [SgmExpose("General", "Total Days", order: 0)]
    public ushort TotalDays = 31;

    [Header("Game Day Length")]
    [Tooltip("Durée d’UN jour de jeu (24h in-game) en minutes réelles.")]
    [SgmExpose("General", "Day Length (minutes)", order: 0)]
    [Min(0.01f)]
    public float DayLengthInMinits = 30.0f;

    #endregion

    #region ========= Derived constants (réel par unité de temps de jeu) =========
    /// <summary>Secondes RÉELLES contenues dans 1 jour de jeu (24h in-game).</summary>
    public float RealSecondsPerGameDay => DayLengthInMinits * 60f;

    /// <summary>Secondes RÉELLES contenues dans 1 heure de jeu.</summary>
    public float RealSecondsPerGameHour => RealSecondsPerGameDay / 24f;

    /// <summary>Secondes RÉELLES contenues dans 1 minute de jeu.</summary>
    public float RealSecondsPerGameMinute => RealSecondsPerGameHour / 60f;
    #endregion

    #region ========= Conversions (jeu → réel) =========
    /// <summary>Convertit des HEURES de jeu en SECONDES réelles.</summary>
    public float GameHoursToRealSeconds(float gameHours)
        => Mathf.Max(0f, gameHours) * RealSecondsPerGameHour;

    /// <summary>Convertit des MINUTES de jeu en SECONDES réelles.</summary>
    public float GameMinutesToRealSeconds(float gameMinutes)
        => Mathf.Max(0f, gameMinutes) * RealSecondsPerGameMinute;

    /// <summary>Convertit une durée de jeu (jours+heures+minutes) en SECONDES réelles.</summary>
    public float GameDurationToRealSeconds(int gameDays, float gameHours, float gameMinutes)
    {
        float totalGameHours =
            Mathf.Max(0, gameDays) * 24f
          + Mathf.Max(0f, gameHours)
          + Mathf.Max(0f, gameMinutes) / 60f;

        return GameHoursToRealSeconds(totalGameHours);
    }
    #endregion

    #region ========= Conversions (réel → jeu) =========
    /// <summary>Convertit des SECONDES réelles en HEURES de jeu.</summary>
    public float RealSecondsToGameHours(float realSeconds)
    {
        if (RealSecondsPerGameHour <= 0f) return 0f;
        return Mathf.Max(0f, realSeconds) / RealSecondsPerGameHour;
    }

    /// <summary>Convertit des SECONDES réelles en JOURS de jeu (fractionnaire possible).</summary>
    public float RealSecondsToGameDays(float realSeconds)
    {
        if (RealSecondsPerGameDay <= 0f) return 0f;
        return Mathf.Max(0f, realSeconds) / RealSecondsPerGameDay;
    }
    #endregion

    #region ========= Totaux utiles =========
    /// <summary>Temps RÉEL total pour parcourir tous les jours de jeu configurés (en secondes réelles).</summary>
    public float GetTotalGameTimeInSeconds()
        => TotalDays * RealSecondsPerGameDay;

    /// <summary>
    /// [Obsolète] Ancien nom ambigu. Utiliser plutôt RealSecondsPerGameDay.
    /// Gardé pour compatibilité temporaire.
    /// </summary>
    [System.Obsolete("Use RealSecondsPerGameDay instead.")]
    public float GetTotalSecondsPerDay() => RealSecondsPerGameDay;
    #endregion

    #region ========= Scheduling helpers (next in-game time → real seconds) =========
    /// <summary>
    /// Renvoie, en SECONDES RÉELLES, le temps restant avant la prochaine occurrence
    /// de <paramref name="targetGameHour"/> (0..24) en partant de <paramref name="currentGameHour"/>.
    /// Si <paramref name="includeIfNow"/> == false et que current == target, on considère la PROCHAINE
    /// occurrence (dans 24h in-game). Si true, on renvoie 0 dans ce cas.
    /// </summary>
    public float RealSecondsUntilGameTime(float currentGameHour, float targetGameHour, bool includeIfNow = false)
    {
        // Différence d'heures in-game en [0,24)
        float deltaHours = Mathf.Repeat(targetGameHour - currentGameHour, 24f);

        // Si on est exactement à l'heure cible
        if (!includeIfNow && Mathf.Approximately(deltaHours, 0f))
            deltaHours = 24f; // prochaine occurrence : dans 24h in-game

        return deltaHours * RealSecondsPerGameHour;
    }

    /// <summary>
    /// Surcharge pratique : part de l'heure in-game courante (TimeHandlerData.CurrentTime).
    /// </summary>
    public float RealSecondsUntilGameTime(float targetGameHour, bool includeIfNow = false)
    {
        return RealSecondsUntilGameTime(TimeHandlerData.CurrentTime, targetGameHour, includeIfNow);
    }

    /// <summary>
    /// Raccourci : temps réel (en secondes) avant le prochain 9:00 (in-game).
    /// </summary>
    public float RealSecondsUntilNextNineAM(bool includeIfNow = false)
    {
        return RealSecondsUntilGameTime(9f, includeIfNow);
    }
    #endregion


    #region ========= Validation =========
    private void OnValidate()
    {
        if (DayLengthInMinits < 0.01f) DayLengthInMinits = 0.01f;
        if (TotalDays < 1) TotalDays = 1;
    }
    #endregion
}
