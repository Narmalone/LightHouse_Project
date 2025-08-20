using LightHouse.Weather;
using LightHouse.Game.Computer.LEO.Weather.Pressure;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Weather
{
    /// <summary>
    /// Calcule les gains météo :
    /// - delta = |input - base|
    /// - delta <= fullMargin     => 100% du gain
    /// - delta >= earningMargin  => 0
    /// - sinon interpolation via PayoutCurve entre 1 et 0
    /// </summary>
    public static class WeatherMoneyCalculator
    {
        #region Core

        private const float EPS = 1e-5f;

        /// <summary>
        /// Évalue un payout avec "deadzone" (fullMargin) et coupure (earningMargin).
        /// </summary>
        private static float EvaluatePayoutWithDeadzone(
            float baseValue,
            float inputValue,
            float maxMoneyWhenCorrect,
            float fullMargin,
            float earningMargin,
            AnimationCurve payoutCurve)
        {
            if (maxMoneyWhenCorrect <= 0f) return 0f;

            // Corrige une config incohérente : earning doit être >= full
            if (earningMargin < fullMargin)
            {
                // swap soft
                float tmp = earningMargin;
                earningMargin = fullMargin;
                fullMargin = tmp;
            }

            float delta = Mathf.Abs(inputValue - baseValue);

            // Zone à 100%
            if (delta <= fullMargin) return maxMoneyWhenCorrect;

            // Au-delà : 0
            if (delta >= earningMargin) return 0f;

            // Entre les deux : normalise et applique la courbe
            float denom = Mathf.Max(EPS, (earningMargin - fullMargin));
            float x = Mathf.Clamp01((delta - fullMargin) / denom); // 0..1
            float mult = payoutCurve != null ? payoutCurve.Evaluate(x) : (1f - x);

            // Clamp par sécurité
            mult = Mathf.Clamp01(mult);

            float payout = maxMoneyWhenCorrect * mult;

            // Arrondi au centime (optionnel)
            return Mathf.Round(payout * 100f) / 100f;
        }

        #endregion

        #region Public calculators

        public static float CalculateWaterTemperature(float baseTemp, float inputTemp, SO_WeatherMoneyResults cfg)
        {
            return EvaluatePayoutWithDeadzone(
                baseTemp, inputTemp,
                cfg.WaterTemp_MaxMoneyWhenCorrect,
                cfg.WaterTemp_FullMargin,
                cfg.WaterTemp_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static float CalculateAirTemperature(float baseTemp, float inputTemp, SO_WeatherMoneyResults cfg)
        {
            return EvaluatePayoutWithDeadzone(
                baseTemp, inputTemp,
                cfg.AirTemp_MaxMoneyWhenCorrect,
                cfg.AirTemp_FullMargin,
                cfg.AirTemp_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static float CalculateHumidity(float baseValue, float inputValue, SO_WeatherMoneyResults cfg)
        {
            Debug.Log(inputValue);
            return EvaluatePayoutWithDeadzone(
                baseValue, inputValue,
                cfg.Humidity_MaxMoneyWhenCorrect,
                cfg.Humidity_FullMargin,
                cfg.Humidity_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static float CalculateWindSpeed(float baseValue, float inputValue, SO_WeatherMoneyResults cfg)
        {
            return EvaluatePayoutWithDeadzone(
                baseValue, inputValue,
                cfg.WindSpeed_MaxMoneyWhenCorrect,
                cfg.WindSpeed_FullMargin,
                cfg.WindSpeed_EarningMargin,
                cfg.PayoutCurve
            );
        }

        public static float CalculateAirPressure(float baseValue, PressureRange datas, SO_WeatherMoneyResults cfg)
        {
            return baseValue >= datas.MinPressure && baseValue <= datas.MaxPressure ? cfg.AirPressure_MaxMoneyWhenCorrect : 0f;
        }

        // Wind direction : à brancher plus tard avec DeltaAngle (écart circulaire)
/*        public static float CalculateWindDirection(float baseDeg, float inputDeg, SO_WeatherMoneyResults cfg)
        {
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(baseDeg, inputDeg)); // [0..180]
                                                                             // baseValue est 0 (écart), inputValue = deltaAbs
            return EvaluatePayoutWithDeadzone(
                0f, deltaAbs,
                cfg.WindDirection_MaxMoneyWhenCorrect,
                cfg.WindDirection_FullMargin,
                cfg.WindDirection_EarningMargin,
                cfg.PayoutCurve
            );
        }*/

        public static float CalculateWindDirection(WindOrientationType baseOrientation, WindOrientationType windOrientation, SO_WeatherMoneyResults cfg)
        {
            return baseOrientation == windOrientation ? cfg.WindDirection_MaxMoneyWhenCorrect : 0.0f;
        }

        #endregion

        public static string FormatMoney(float v)
        {
            if (Mathf.Approximately(v, 0f)) return "0";
            // 0.## pour éviter trop de décimales
            return (v > 0f ? $"+ {v:0.##}" : $"- {Mathf.Abs(v):0.##}");
        }

        public static Color ColorForAmount(float v) => v > 0f ? Color.green : (Mathf.Approximately(v, 0f) ? Color.gray : Color.red);
    }
}
