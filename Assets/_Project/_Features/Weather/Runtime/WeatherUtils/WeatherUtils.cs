using LightHouse.Features.TimeOfDay.TimeCore;
using UnityEngine;

namespace LightHouse.Features.Weather.Utils
{
    /// <summary>
    /// Utilitaires météo :
    ///  - Interpolations de WeatherData (y compris orientation vent).
    ///  - Conversion angle ↔ orientation cardinale.
    ///  - Dérivation d’un WeatherType à partir de seuils (base de définitions).
    ///  - Accès "timeline" (récupération de la météo au temps T ou jour/heure).
    ///  - Aides d’affichage (cardinal letters, état de mer simplifié).
    /// </summary>
    public static class WeatherUtils
    {
        #region ======================= Config / Tuning =======================

        // Poids du scoring "DetermineWeatherType". Ajustez selon vos définitions.
        private const float WEIGHT_IN_RANGE_MAIN = 1f;   // humidité / pression / vent / air
        private const float WEIGHT_IN_RANGE_WATER = 0.5f; // eau un peu moins discriminant

        // Tolérance numérique.
        private const float EPS = 1e-5f;

        #endregion

        #region ======================= Public API (Interpolation & Type) =======================

        /// <summary>
        /// Interpole deux <see cref="WeatherData"/> pour t∈[0..1].
        /// - Les méta temporelles (Start/Duration) sont gardées depuis <paramref name="from"/>.
        /// - L’orientation du vent est interpolée circulairement (LerpAngle) et normalisée [0..360).
        /// - WeatherType hérite de from (t&lt;0.5) ou to (t≥0.5) – simple "step" visuel.
        /// </summary>
        public static WeatherData LerpWeatherData(WeatherData from, WeatherData to, float t)
        {
            t = Mathf.Clamp01(t);

            // Interpolation angulaire + normalisation
            float rawOrientation = Mathf.LerpAngle(from.WindOrientation, to.WindOrientation, t);
            float normalizedOrientation = NormalizeAngle360(rawOrientation);

            return new WeatherData
            {
                // On ne lerp pas les métadonnées temporelles
                WeatherType = (t < 0.5f) ? from.WeatherType : to.WeatherType,
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

        /// <summary>
        /// Convertit un angle en orientation cardinale discrète.
        /// 0=N, 45=NE, 90=E, …, 315=NW. L’angle est d’abord normalisé en [0..360).
        /// </summary>
        public static WindOrientationType AngleToOrientationType(float angle)
        {
            angle = NormalizeAngle360(angle);
            int sector = Mathf.RoundToInt(angle / 45f) % 8; // 8 secteurs de 45°
            return (WindOrientationType)sector;
        }

        /// <summary>
        /// Choisit le <see cref="WeatherType"/> le plus cohérent en scorant chaque définition :
        /// pour chaque range respecté → +poids ; sinon → -poids.
        /// </summary>
        public static WeatherType DetermineWeatherType(
            float humidity, float pressure, float airTemp, float windSpeed, float waterTemp,
            WeatherConfigDatabase definitionDatabase)
        {
            WeatherType bestMatch = WeatherType.Sunny;
            float bestScore = float.MinValue;

            foreach (var def in definitionDatabase.Definitions)
            {
                float score = 0f;
                score += ScoreRange(humidity, def.HumidityRange, WEIGHT_IN_RANGE_MAIN);
                score += ScoreRange(pressure, def.PressureRange, WEIGHT_IN_RANGE_MAIN);
                score += ScoreRange(windSpeed, def.WindSpeedRange, WEIGHT_IN_RANGE_MAIN);
                score += ScoreRange(airTemp, def.AirTemperatureRange, WEIGHT_IN_RANGE_MAIN);
                score += ScoreRange(waterTemp, def.WaterTemperatureRange, WEIGHT_IN_RANGE_WATER);

                if (score > bestScore) { bestScore = score; bestMatch = def.Type; }
            }
            return bestMatch;
        }

        #endregion

        #region ======================= Timeline Query (GetWeatherAt*) =======================

        /// <summary>
        /// Renvoie la météo interpolée pour un <paramref name="day"/> (0-based) et une <paramref name="hour"/> (0..24).
        /// Si <paramref name="wrap"/> est true, boucle dans la timeline ; sinon clamp au début/fin.
        /// </summary>
        public static WeatherData GetWeatherAt(
            byte day, float hour,
            WeatherTimeline timeline, TimeConfiguration config,
            bool wrap = false)
        {
            float secondsPerDay = config.RealSecondsPerGameDay;

            // Clamp l’heure pour éviter des surprises
            float clampedHour = Mathf.Clamp(hour, 0f, 24f);

            // Conversion jour/heure -> secondes de jeu absolues
            float targetTime = day * secondsPerDay + (clampedHour / 24f) * secondsPerDay;

            return GetWeatherAtAbsoluteSeconds(targetTime, timeline, wrap);
        }

        /// <summary>
        /// Variante en secondes de jeu absolues (jour + heure déjà convertis).
        /// </summary>
        public static WeatherData GetWeatherAtAbsoluteSeconds(
            float gameSeconds, WeatherTimeline timeline, bool wrap = false)
        {
            var weathers = timeline.Weathers;
            if (weathers == null || weathers.Count == 0) return null;

            int last = weathers.Count - 1;
            float timelineEnd = weathers[last].StartTimeInSeconds + weathers[last].DurationInSeconds;

            // Boucle ou clamp dans la timeline
            if (wrap && timelineEnd > 0f)
                gameSeconds = Mathf.Repeat(gameSeconds, timelineEnd);
            else
                gameSeconds = Mathf.Clamp(gameSeconds, 0f, Mathf.Max(0f, timelineEnd - Mathf.Epsilon));

            // Recherche linéaire (suffit largement; passer en binaire si besoin)
            for (int i = 0; i < weathers.Count; i++)
            {
                var cur = weathers[i];
                float start = cur.StartTimeInSeconds;
                float end = start + cur.DurationInSeconds;

                // Inclut la frontière de fin avec >= sur le segment suivant via la boucle
                bool onLastExactEnd = (i == last && Mathf.Approximately(gameSeconds, end));
                if (gameSeconds < end || onLastExactEnd)
                {
                    // Segment suivant : réel, wrap vers le premier, ou cur lui-même (constant) si fin
                    var next = (i < last) ? weathers[i + 1] : (wrap ? weathers[0] : cur);

                    float local = gameSeconds - start;
                    float dur = Mathf.Max(EPS, cur.DurationInSeconds);
                    float t = Mathf.Clamp01(local / dur);

                    return LerpWeatherData(cur, next, t);
                }
            }

            // Fallback robuste
            return weathers[last];
        }

        #endregion

        #region ======================= Affichage / Libellés =======================

        /// <summary>
        /// Retourne la lettre cardinale courte pour une orientation (N, NE, E, …).
        /// </summary>
        public static string GetCardinalLetter(WindOrientationType t)
        {
            switch (t)
            {
                case WindOrientationType.North: return "N";
                case WindOrientationType.North_East: return "NE";
                case WindOrientationType.East: return "E";
                case WindOrientationType.South_East: return "SE";
                case WindOrientationType.South: return "S";
                case WindOrientationType.South_West: return "SW";
                case WindOrientationType.West: return "W";
                case WindOrientationType.North_West: return "NW";
                default: return "";
            }
        }

        #endregion

        #region ======================= Helpers num / scoring =======================

        /// <summary>
        /// Normalise un angle en degrés dans [0..360).
        /// </summary>
        public static float NormalizeAngle360(float angle)
        {
            float a = angle % 360f;
            return (a < 0f) ? a + 360f : a;
        }

        // 0°=Nord(+Z), 90°=Est(+X)  -> direction monde (TOWARDS)
        public static Vector3 HeadingToDirXZ(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r));
        }

        // Convertit un angle météo “FROM” (d’où vient) en “TOWARDS” (vers où va)
        public static float FromToTowards(float fromDeg) => NormalizeAngle360(fromDeg + 180f);


        /// <summary>
        /// Test d’appartenance à un intervalle [x..y] (Vector2.x = min, Vector2.y = max).
        /// </summary>
        private static bool IsInRange(float value, Vector2 range) =>
            value >= range.x && value <= range.y;

        /// <summary>
        /// Rend +w si la valeur est dans la plage, -w sinon (pour le scoring).
        /// </summary>
        private static float ScoreRange(float v, Vector2 range, float weight) =>
            IsInRange(v, range) ? +weight : -weight;

        #endregion
    }
}
