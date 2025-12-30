using LightHouse.Weather;
using LightHouse.Game.Computer.LEO.Weather.Pressure;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Weather
{
    public struct WeatherPayoutResult
    {
        public float Payout;   // argent final gagné
        public float InputValue;
        public float Accuracy; // EN POURCENT (0..100)

        public WeatherPayoutResult(float payout, float accuracy, float value)
        {
            Payout = payout;
            Accuracy = accuracy;
            InputValue = value;
        }
    }

    /// <summary>
    /// Calcule les gains météo :
    /// - delta = |input - base|
    /// - delta <= fullMargin     => 100% du gain
    /// - delta >= earningMargin  => 0
    /// - sinon interpolation via PayoutCurve entre 1 et 0 (puis *100 pour accuracy)
    /// </summary>
    public static class WeatherMoneyCalculator
    {
        #region Core

        private const float EPS = 1e-5f;

        private static float Round2(float v) => Mathf.Round(v * 100f) / 100f;

        /// <summary>
        /// Convertit 0..1 -> 0..100 si on détecte une valeur "petite".
        /// Laisse passer 0..100 tel quel. Utile pour l’humidité.
        /// </summary>
        private static float ToPercentIf01(float v)
        {
            return (v > 0f && v <= 1.0001f) ? v * 100f : v;
        }

        /// <summary>
        /// Évalue un payout avec "deadzone" (fullMargin) et coupure (earningMargin).
        /// Retourne aussi la précision (Accuracy en 0..100).
        /// </summary>
        private static WeatherPayoutResult EvaluatePayoutWithDeadzone(
            float baseValue,
            float inputValue,
            float maxMoneyWhenCorrect,
            float fullMargin,
            float earningMargin,
            AnimationCurve payoutCurve)
        {
            if (maxMoneyWhenCorrect <= 0f)
                return new WeatherPayoutResult(0f, 0f, inputValue);

            // S'assure que earningMargin >= fullMargin
            if (earningMargin < fullMargin)
            {
                float tmp = earningMargin;
                earningMargin = fullMargin;
                fullMargin = tmp;
            }

            float delta = Mathf.Abs(inputValue - baseValue);

            // Zone à 100%
            if (delta <= fullMargin)
                return new WeatherPayoutResult(Round2(maxMoneyWhenCorrect), 100f, inputValue);

            // Au-delà de earning => 0
            if (delta >= earningMargin)
                return new WeatherPayoutResult(0f, 0f, inputValue);

            // Entre les deux : interpolation
            float denom = Mathf.Max(EPS, (earningMargin - fullMargin));
            float x = Mathf.Clamp01((delta - fullMargin) / denom); // 0..1
            float mult = payoutCurve != null ? payoutCurve.Evaluate(x) : (1f - x);
            mult = Mathf.Clamp01(mult);

            float payout = Round2(maxMoneyWhenCorrect * mult);
            float accuracyPct = mult * 100f;

            return new WeatherPayoutResult(payout, accuracyPct, inputValue);
        }

        #endregion

        #region Public calculators (tous retournent PayoutResult)

        public static WeatherPayoutResult CalculateWaterTemperature(float baseTemp, float inputTemp, SO_WeatherMoneyResults cfg)
        {
            return EvaluatePayoutWithDeadzone(
                baseTemp, inputTemp,
                cfg.WaterTemp_MaxMoneyWhenCorrect,
                cfg.WaterTemp_FullMargin,
                cfg.WaterTemp_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static WeatherPayoutResult CalculateAirTemperature(float baseTemp, float inputTemp, SO_WeatherMoneyResults cfg)
        {
            return EvaluatePayoutWithDeadzone(
                baseTemp, inputTemp,
                cfg.AirTemp_MaxMoneyWhenCorrect,
                cfg.AirTemp_FullMargin,
                cfg.AirTemp_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static WeatherPayoutResult CalculateHumidity(float baseValue, float inputValue, SO_WeatherMoneyResults cfg)
        {
            // Normalise automatiquement en pourcents si 0..1 est détecté
            float b = ToPercentIf01(baseValue);
            float i = ToPercentIf01(inputValue);

            return EvaluatePayoutWithDeadzone(
                b, i,
                cfg.Humidity_MaxMoneyWhenCorrect,
                cfg.Humidity_FullMargin,
                cfg.Humidity_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static WeatherPayoutResult CalculateWindSpeed(float baseValue, float inputValue, SO_WeatherMoneyResults cfg)
        {
            // ⚠️ Assure-toi que baseValue et inputValue sont dans la même unité (ex: tous deux en kts).
            return EvaluatePayoutWithDeadzone(
                baseValue, inputValue,
                cfg.WindSpeed_MaxMoneyWhenCorrect,
                cfg.WindSpeed_FullMargin,
                cfg.WindSpeed_EarningMargin,
                cfg.PayoutCurve
            );
        }

        // Pressure : stocke une valeur représentative de l’INPUT joueur (ex: centre du range)
        public static WeatherPayoutResult CalculateAirPressure(float baseValue, PressureRange datas, SO_WeatherMoneyResults cfg)
        {
            float inputCenter = 0.5f * (datas.MinPressure + datas.MaxPressure); // <- input
            bool inRange = baseValue >= datas.MinPressure && baseValue <= datas.MaxPressure;

            return inRange
                ? new WeatherPayoutResult(Round2(cfg.AirPressure_MaxMoneyWhenCorrect), 100f, inputCenter)
                : new WeatherPayoutResult(0f, 0f, inputCenter);
        }

        // Wind dir : Value doit être l’orientation choisie par le joueur
        public static WeatherPayoutResult CalculateWindDirection(WindOrientationType baseOrientation, WindOrientationType inputOrientation, SO_WeatherMoneyResults cfg)
        {
            bool ok = (baseOrientation == inputOrientation);
            return ok
                ? new WeatherPayoutResult(Round2(cfg.WindDirection_MaxMoneyWhenCorrect), 100f, (float)inputOrientation)
                : new WeatherPayoutResult(0f, 0f, (float)inputOrientation);
        }

        #endregion

        #region Utils d’affichage

        public static string FormatMoney(float v)
        {
            if (Mathf.Approximately(v, 0f)) return "0";
            return (v > 0f ? $"+ {v:0.##}" : $"- {Mathf.Abs(v):0.##}");
        }

        public static Color ColorForAmount(float v)
            => v > 0f ? Color.green : (Mathf.Approximately(v, 0f) ? Color.gray : Color.red);

        #endregion
    }
}
