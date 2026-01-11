using LightHouse.Features.Computer.LEO.Weather;
using LightHouse.Features.TimeOfDay;
using LightHouse.Features.TimeOfDay.TimeCore;
using LightHouse.Features.Weather.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Weather
{
    /// <summary>
    /// Génère et expose des prévisions discrètes (matin / midi / soir / nuit) pour N jours,
    /// puis permet de construire une WeatherData "mélangée" (player-informed) en fonction :
    ///   - des valeurs du MONDE (timeline réelle),
    ///   - des INPUTS JOUEUR,
    ///   - des ACCURACIES calculées (0..100%).
    ///
    /// Règle de mélange (par variable) :
    ///   accuracy = 0%   -> on garde 100% la valeur JOUEUR
    ///   accuracy = 100% -> on garde 100% la valeur MONDE
    ///   sinon           -> interpolation linéaire entre les deux.
    /// 
    /// Remarque : l’humidité est automatiquement remise à l’échelle (0..1 <-> 0..100) si besoin.
    /// </summary>
    public class WeatherForecast
    {

        #region ===== Données : listes par segment =====

        /// <summary> Prévisions "monde" échantillonnées vers 9h pour chaque jour. </summary>
        public List<WeatherData> MorningsDatas = new List<WeatherData>();

        /// <summary> Prévisions "monde" échantillonnées vers 12h pour chaque jour. </summary>
        public List<WeatherData> MiddaysDatas = new List<WeatherData>();

        /// <summary> Prévisions "monde" échantillonnées vers 18h pour chaque jour. </summary>
        public List<WeatherData> EveningDatas = new List<WeatherData>();

        /// <summary> Prévisions "monde" échantillonnées vers 0h pour chaque jour. </summary>
        public List<WeatherData> MiddnightDatas = new List<WeatherData>();

        #endregion

        #region ===== Construction & génération initiale =====

        /// <summary>
        /// Construit les tableaux de prévisions "monde" (matin/midi/soir/nuit) pour tous les jours.
        /// </summary>
        public WeatherForecast(TimeConfiguration timeConfig, WeatherTimeline weatherTimeline)
        {
            GenerateForecast(timeConfig, weatherTimeline);
        }

        /// <summary>
        /// (Ré)génère les listes de prévisions "monde" pour chaque segment et chaque jour.
        /// </summary>
        private void GenerateForecast(TimeConfiguration timeConfig, WeatherTimeline weatherTimeline)
        {
            MorningsDatas.Clear();
            MiddaysDatas.Clear();
            EveningDatas.Clear();
            MiddnightDatas.Clear();

            int totalDays = timeConfig.TotalDays;

            for (byte day = 0; day < totalDays; day++)
            {
                // Heures d’échantillonnage par segment
                MorningsDatas.Add(WeatherUtils.GetWeatherAt(day, 9f, weatherTimeline, timeConfig));
                MiddaysDatas.Add(WeatherUtils.GetWeatherAt(day, 12f, weatherTimeline, timeConfig));
                EveningDatas.Add(WeatherUtils.GetWeatherAt(day, 18f, weatherTimeline, timeConfig));
                MiddnightDatas.Add(WeatherUtils.GetWeatherAt(day, 0f, weatherTimeline, timeConfig));
            }
        }

        #endregion

        #region ===== API publique : construction d’une WeatherData “player-informed” =====

        /// <summary>
        /// Construit la météo "player-informed" pour un <paramref name="targetDay"/> et un <paramref name="targetSegment"/>.
        /// Mélange MONDE/JOUEUR pondéré par les accuracies (0..100%).
        /// </summary>
        /// <param name="targetDay"> Jour cible (0..TotalDays-1). </param>
        /// <param name="datas"> Résultats d’argent/accuracy contenant les INPUTS joueur. </param>
        /// <param name="targetSegment"> Segment temporel (matin/midi/soir/nuit). </param>
        /// <returns> WeatherData blendée (ou null si out of range). </returns>
        public WeatherData GetForecastBasedOnPlayerInput(int targetDay, MoneyWeatherData datas, TimeOfDaySegment targetSegment)
        {
            // 1) Récupère la météo "monde" du jour/segment
            var world = GetWorldSegmentData(targetDay, targetSegment);
            if (world == null) return null; // pas de données pour ce slot

            // 2) Inputs du joueur (issus de MoneyWeatherData.XXX.InputValue)
            float pAir = datas.AirTemperatureResult.InputValue;
            float pWater = datas.WaterTemperatureResult.InputValue;
            float pHum = HumidityMatchScale(datas.HumidityRateResult.InputValue, world.Humidity);
            float pPress = datas.AtmosphericPressureResult.InputValue; // centre du range tel que calculé côté payout
            float pWind = datas.WindSpeedResult.InputValue;
            var pDir = (WindOrientationType)datas.WindOrientationResult.InputValue; // stocké en float → cast

            // 3) Accuracies en %
            float aAir = datas.AirTemperatureResult.Accuracy;
            float aWater = datas.WaterTemperatureResult.Accuracy;
            float aHum = datas.HumidityRateResult.Accuracy;
            float aPress = datas.AtmosphericPressureResult.Accuracy;
            float aWind = datas.WindSpeedResult.Accuracy;
            float aDir = datas.WindOrientationResult.Accuracy;

            // 4) Mélange scalaire (0% => joueur, 100% => monde)
            float outAir = BlendAcc(world.AirTemperature, pAir, aAir);
            float outWater = BlendAcc(world.WaterTemperature, pWater, aWater);
            float outHum = BlendAcc(world.Humidity, pHum, aHum);
            float outPress = BlendAcc(world.AtmosphericPressure, pPress, aPress);
            float outWind = BlendAcc(world.WindSpeed, pWind, aWind);

            // 5) Direction du vent : interpolation angulaire (0% => joueur, 100% => monde)
            float worldAng = world.WindOrientation;                 // 0..360
            float playerAng = OrientationTypeToAngle(pDir);          // 0,45,...315
            float tDirToPlayer = 1f - Mathf.Clamp01(aDir / 100f);    // 0% acc -> 1 (joueur), 100% -> 0 (monde)
            float outAngle = Mathf.LerpAngle(worldAng, playerAng, tDirToPlayer);
            outAngle = WeatherUtils.NormalizeAngle360(outAngle);
            var outDirType = WeatherUtils.AngleToOrientationType(outAngle);

          /*  Debug.Log(
                   $"[ForecastBlend] seg={targetSegment} day={targetDay} | " +
                   $"WORLD: Air={world.AirTemperature}, Water={world.WaterTemperature}, Hum={world.Humidity}, " +
                   $"WindSpd={world.WindSpeed}, WindDir={world.WindOrientationType} | " +
                   $"PLAYER: Air={pAir} ({aAir}%), Water={pWater} ({aWater}%), Hum={datas.HumidityRateResult.InputValue} ({aHum}%), " +
                   $"WindSpd={pWind} ({aWind}%), WindDir={pDir} ({aDir}%), Press={pPress} ({aPress}%) | " +
                   $"OUT: Air={outAir}, Water={outWater}, Hum={outHum}, WindSpd={outWind}, WindDir={outDirType}, Press={outPress}"
               );*/

            // 6) Retourne une WeatherData “player-informed”, en conservant la fenêtre temporelle du segment
            return new WeatherData
            {
                WeatherType = world.WeatherType,          // Option : recalculer plus tard via DetermineWeatherType(...)
                StartTimeInSeconds = world.StartTimeInSeconds,   // on garde la plage temporelle du segment
                DurationInSeconds = world.DurationInSeconds,

                Humidity = outHum,
                AtmosphericPressure = outPress,
                WindSpeed = outWind,
                WindOrientation = outAngle,
                WindOrientationType = outDirType,
                WaterTemperature = outWater,
                AirTemperature = outAir
            };
        }

        /// <summary>
        /// Moyenne simple des 6 accuracies du jour (0..100).
        /// </summary>
        public float AverageAccuracy(MoneyWeatherData d)
        {
            return (d.AirTemperatureResult.Accuracy
                  + d.WaterTemperatureResult.Accuracy
                  + d.HumidityRateResult.Accuracy
                  + d.AtmosphericPressureResult.Accuracy
                  + d.WindSpeedResult.Accuracy
                  + d.WindOrientationResult.Accuracy) / 6f;
        }

        #endregion

        #region ===== Sélection & accès aux données “monde” =====

        /// <summary>
        /// Retourne la liste correspondant à un segment (matin/midi/soir/nuit).
        /// </summary>
        private List<WeatherData> GetListForSegment(TimeOfDaySegment seg)
        {
            switch (seg)
            {
                case TimeOfDaySegment.Morning: return MorningsDatas;
                case TimeOfDaySegment.Midday: return MiddaysDatas;
                case TimeOfDaySegment.Evening: return EveningDatas;
                case TimeOfDaySegment.Night: return MiddnightDatas;
                default: return MorningsDatas;
            }
        }

        /// <summary>
        /// Récupère la météo "monde" d'un jour/segment (null si out of range).
        /// </summary>
        private WeatherData GetWorldSegmentData(int day, TimeOfDaySegment seg)
        {
            var list = GetListForSegment(seg);
            if (list == null || day < 0 || day >= list.Count) return null;
            return list[day];
        }

        #endregion

        #region ===== Helpers de mélange & conversions =====

        /// <summary>
        /// Met l’humidité joueur sur la même échelle que le monde :
        ///  - si le monde est en % (ex: 0..100) et le joueur en 0..1 → *100
        ///  - si le monde est en 0..1 et le joueur en % → /100
        ///  - sinon : inchangé.
        /// </summary>
        private float HumidityMatchScale(float playerValue, float worldValue)
        {
            bool worldIsPct = worldValue > 1.0001f;
            bool playerIs01 = playerValue >= 0f && playerValue <= 1.0001f;
            if (worldIsPct && playerIs01) return playerValue * 100f;
            if (!worldIsPct && playerValue > 1f) return playerValue / 100f;
            return playerValue;
        }

        /// <summary>
        /// Interpolation inversée par accuracy :
        ///   accuracy = 0%   → 100% joueur,
        ///   accuracy = 100% → 100% monde,
        ///   sinon           → LERP(world → player, t=1-acc).
        /// </summary>
        private float BlendAcc(float world, float player, float accuracyPct)
        {
            float tToPlayer = 1f - Mathf.Clamp01(accuracyPct / 100f);
            return Mathf.Lerp(world, player, tToPlayer);
        }

        /// <summary>
        /// Convertit une orientation discrète (N, NE, E, …) en angle (0, 45, 90, …).
        /// </summary>
        private float OrientationTypeToAngle(WindOrientationType t) => ((int)t) * 45f;

        #endregion
    }
}
