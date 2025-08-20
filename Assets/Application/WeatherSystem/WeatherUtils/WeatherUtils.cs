using UnityEngine;

namespace LightHouse.Weather.Utils
{
    public static class WeatherUtils
    {
        #region Public API (déjà ok)

        public static WeatherData LerpWeatherData(WeatherData from, WeatherData to, float t)
        {
            t = Mathf.Clamp01(t);

            float rawOrientation = Mathf.LerpAngle(from.WindOrientation, to.WindOrientation, t);
            float normalizedOrientation = NormalizeAngle360(rawOrientation);

            return new WeatherData
            {
                // on ne lerp pas les métadonnées temporelles
                WeatherType = t < 0.5f ? from.WeatherType : to.WeatherType,
                StartTimeInSeconds = from.StartTimeInSeconds,
                DurationInSeconds = from.DurationInSeconds,

                Humidity = Mathf.Lerp(from.Humidity, to.Humidity, t),
                AtmosphericPressure = Mathf.Lerp(from.AtmosphericPressure, to.AtmosphericPressure, t),
                WindSpeed = Mathf.Lerp(from.WindSpeed, to.WindSpeed, t),
                WindOrientation = normalizedOrientation,
                WaterTemperature = Mathf.Lerp(from.WaterTemperature, to.WaterTemperature, t),
                AirTemperature = Mathf.Lerp(from.AirTemperature, to.AirTemperature, t),
                WindOrientationType = AngleToOrientationType(normalizedOrientation)
            };
        }

        public static WindOrientationType AngleToOrientationType(float angle)
        {
            angle = NormalizeAngle360(angle);
            int sector = Mathf.RoundToInt(angle / 45f) % 8;
            return (WindOrientationType)sector;
        }

        public static WeatherType DetermineWeatherType(
            float humidity, float pressure, float airTemp, float windSpeed, float waterTemp,
            WeatherConfigDatabase definitionDatabase)
        {
            WeatherType bestMatch = WeatherType.Sunny;
            float bestScore = float.MinValue;

            foreach (var def in definitionDatabase.Definitions)
            {
                float score = 0f;
                score += IsInRange(humidity, def.HumidityRange) ? 1f : -1f;
                score += IsInRange(pressure, def.PressureRange) ? 1f : -1f;
                score += IsInRange(windSpeed, def.WindSpeedRange) ? 1f : -1f;
                score += IsInRange(airTemp, def.AirTemperatureRange) ? 1f : -1f;
                score += IsInRange(waterTemp, def.WaterTemperatureRange) ? 0.5f : -0.5f;

                if (score > bestScore) { bestScore = score; bestMatch = def.Type; }
            }
            return bestMatch;
        }

        #endregion

        #region Query helpers (GetWeatherAt)

        /// <summary>
        /// Renvoie la météo interpolée pour un <paramref name="day"/> (0-based) et une <paramref name="hour"/> (0..24).
        /// </summary>
        /// <param name="wrap">
        /// Si true, l’instant demandé boucle dans la timeline (utile pour une timeline cyclique).
        /// </param>
        public static WeatherData GetWeatherAt(byte day, float hour, WeatherTimeline timeline, TimeConfiguration config, bool wrap = false)
        {
            float secondsPerDay = config.GetTotalSecondsPerDay();
            // clamp l’heure dans [0,24] pour éviter les surprises
            float clampedHour = Mathf.Clamp(hour, 0f, 24f);
            float targetTime = day * secondsPerDay + (clampedHour / 24f) * secondsPerDay;

            return GetWeatherAtAbsoluteSeconds(targetTime, timeline, wrap);
        }

        /// <summary>
        /// Variante en secondes de jeu absolues (jour + heure déjà convertis).
        /// </summary>
        public static WeatherData GetWeatherAtAbsoluteSeconds(float gameSeconds, WeatherTimeline timeline, bool wrap = false)
        {
            var weathers = timeline.Weathers;
            if (weathers == null || weathers.Count == 0) return null;

            int last = weathers.Count - 1;
            float timelineEnd = weathers[last].StartTimeInSeconds + weathers[last].DurationInSeconds;

            // boucle ou clamp dans la timeline
            if (wrap && timelineEnd > 0f) gameSeconds = Mathf.Repeat(gameSeconds, timelineEnd);
            else gameSeconds = Mathf.Clamp(gameSeconds, 0f, Mathf.Max(0f, timelineEnd - Mathf.Epsilon));

            // Recherche linéaire (suffit largement; passer en binaire si besoin)
            for (int i = 0; i < weathers.Count; i++)
            {
                var cur = weathers[i];
                float start = cur.StartTimeInSeconds;
                float end = start + cur.DurationInSeconds;

                // inclut la frontière de fin avec >= sur le segment suivant via la boucle
                if (gameSeconds < end || (i == last && Mathf.Approximately(gameSeconds, end)))
                {
                    // segment suivant : soit le suivant réel, soit wrap vers le premier, soit cur lui-même (constant) en fin de timeline
                    var next = (i < last) ? weathers[i + 1] : (wrap ? weathers[0] : cur);

                    float local = gameSeconds - start;
                    float dur = Mathf.Max(1e-5f, cur.DurationInSeconds);
                    float t = Mathf.Clamp01(local / dur);

                    return LerpWeatherData(cur, next, t);
                }
            }

            // fallback robuste
            return weathers[last];
        }

        #endregion

        #region Helpers

        public static float NormalizeAngle360(float angle)
        {
            float a = angle % 360f;
            return (a < 0f) ? a + 360f : a;
        }

        private static bool IsInRange(float value, Vector2 range) =>
            value >= range.x && value <= range.y;

        #endregion
    }
}
