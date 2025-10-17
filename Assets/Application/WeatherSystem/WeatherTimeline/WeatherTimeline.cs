using System;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Weather.Utils;

namespace LightHouse.Weather
{
    /// <summary>
    /// Ligne de temps météo pour toute la durée du jeu (enchaînement de segments).
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherTimeline_", menuName = "LightHouse/WeatherSystem/New Timeline")]
    public class WeatherTimeline : ScriptableObject
    {
        #region Data

        public List<WeatherData> Weathers = new();

        public WeatherForecast Forecast = null;

        [Header("Custom start (optionnel)")]
        public bool EnableCustomWeathers = false;
        [Tooltip("Types imposés pour les premiers segments de la timeline, dans l’ordre.")]
        public WeatherType[] CustomWeathers;

        /// <summary>Appelé après régénération complète de la timeline.</summary>
        public static Action OnWeatherGenerated { get; set; }

        #endregion

        #region Generation

        /// <param name="totalTime">Durée totale du jeu (secondes).</param>
        public void GenerateTimeline(WeatherConfigDatabase database, TimeConfiguration timeConfig)
        {
            Weathers.Clear();

            float totalTime = timeConfig.GetTotalGameTimeInSeconds();

            float t = 0f;
            while (t < totalTime)
            {
                var type = (WeatherType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(WeatherType)).Length);
                var def = database.GetDefinition(type);

                float duration = UnityEngine.Random.Range(def.MinWeatherDuration, def.MaxWeatherDuration);
                if (duration <= 0f) duration = 1f; // garde-fou

                if (t + duration > totalTime)
                    duration = totalTime - t;

                float orientation = UnityEngine.Random.Range(0f, 360f);

                Weathers.Add(new WeatherData
                {
                    WeatherType = type,
                    StartTimeInSeconds = t,
                    DurationInSeconds = duration,
                    Humidity = UnityEngine.Random.Range(def.HumidityRange.x, def.HumidityRange.y),
                    AtmosphericPressure = UnityEngine.Random.Range(def.PressureRange.x, def.PressureRange.y),
                    WindSpeed = UnityEngine.Random.Range(def.WindSpeedRange.x, def.WindSpeedRange.y),
                    AirTemperature = UnityEngine.Random.Range(def.AirTemperatureRange.x, def.AirTemperatureRange.y),
                    WaterTemperature = UnityEngine.Random.Range(def.WaterTemperatureRange.x, def.WaterTemperatureRange.y),
                    WindOrientation = WeatherUtils.NormalizeAngle360(orientation),
                    WindOrientationType = WeatherUtils.AngleToOrientationType(orientation)
                });

                t += duration;
            }

            // ★ Forçage des premiers segments si demandé
            if (EnableCustomWeathers)
                ApplyCustomOverrides(database);

            Forecast = new WeatherForecast(timeConfig, this);
            OnWeatherGenerated?.Invoke();
        }

        /// <summary>
        /// Force les premiers segments avec CustomWeathers.
        /// Préserve StartTime/Duration, rééchantillonne les paramètres depuis la DB du type imposé.
        /// </summary>
        private void ApplyCustomOverrides(WeatherConfigDatabase database)
        {
            if (CustomWeathers == null || CustomWeathers.Length == 0) return;
            if (Weathers == null || Weathers.Count == 0) return;

            int count = Mathf.Min(CustomWeathers.Length, Weathers.Count);
            for (int i = 0; i < count; i++)
            {
                var forcedType = CustomWeathers[i];
                var seg = Weathers[i];
                if (seg == null) continue;

                var def = database.GetDefinition(forcedType);
                if (def == null)
                {
                    Debug.LogWarning($"[WeatherTimeline] Aucun def pour {forcedType}, skip override index {i}");
                    continue;
                }

                // On garde le timing existant
                float start = seg.StartTimeInSeconds;
                float dur = seg.DurationInSeconds;

                // Re-roll des paramètres selon le type imposé
                float orientation = UnityEngine.Random.Range(0f, 360f);

                seg.WeatherType = forcedType;
                seg.Humidity = UnityEngine.Random.Range(def.HumidityRange.x, def.HumidityRange.y);
                seg.AtmosphericPressure = UnityEngine.Random.Range(def.PressureRange.x, def.PressureRange.y);
                seg.WindSpeed = UnityEngine.Random.Range(def.WindSpeedRange.x, def.WindSpeedRange.y);
                seg.AirTemperature = UnityEngine.Random.Range(def.AirTemperatureRange.x, def.AirTemperatureRange.y);
                seg.WaterTemperature = UnityEngine.Random.Range(def.WaterTemperatureRange.x, def.WaterTemperatureRange.y);
                seg.WindOrientation = WeatherUtils.NormalizeAngle360(orientation);
                seg.WindOrientationType = WeatherUtils.AngleToOrientationType(orientation);

                seg.StartTimeInSeconds = start;
                seg.DurationInSeconds = dur;

                // Si des champs dérivés existent, rebake ici si nécessaire.
                // WeatherUtils.BakeDerived(seg, database);

                // (Optionnel) Debug
                // Debug.Log($"[Timeline] Override[{i}] => {forcedType} (t={start:0.0}s, dur={dur:0.0}s)");
            }
        }

        #endregion
    }
}
