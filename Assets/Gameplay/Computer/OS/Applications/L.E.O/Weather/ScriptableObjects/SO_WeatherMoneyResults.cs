using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Weather
{
    [CreateAssetMenu(fileName = "SO_Money_Money_Config_Default", menuName = "LightHouse/LEO/Weather/New Money Config")]
    public class SO_WeatherMoneyResults : ScriptableObject
    {
        #region Smoothing

        [Header("Smoothing")]
        [Tooltip("x = ((|input-base|-FullMargin)/(EarningMargin-FullMargin)) dans [0..1], y = multiplicateur [1..0].")]
        public AnimationCurve PayoutCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        #endregion

        #region Water Temperature

        [Header("Water Temperature")]
        public float WaterTemp_MaxMoneyWhenCorrect = 25f;

        [Tooltip("Zone à 100% de gain autour de la valeur cible.")]
        public float WaterTemp_FullMargin = 2.5f;

        [Tooltip("Au-delà de cette marge, gain = 0.")]
        public float WaterTemp_EarningMargin = 7.0f;

        #endregion

        #region Air Temperature

        [Header("Air Temperature")]
        public float AirTemp_MaxMoneyWhenCorrect = 25f;
        public float AirTemp_FullMargin = 2.5f;
        public float AirTemp_EarningMargin = 7.0f;

        #endregion

        #region Air Pressure

        [Header("Air Pressure")]
        public float AirPressure_MaxMoneyWhenCorrect = 25f;

        #endregion

        #region Wind Speed

        [Header("Wind Speed")]
        public float WindSpeed_MaxMoneyWhenCorrect = 25f;
        public float WindSpeed_FullMargin = 2.5f;
        public float WindSpeed_EarningMargin = 7.0f;

        #endregion

        #region Wind Direction (géré séparément)

        [Header("Wind Direction")]
        public float WindDirection_MaxMoneyWhenCorrect = 25f;

        #endregion

        #region Humidity

        [Header("Humidity Rate")]
        public float Humidity_MaxMoneyWhenCorrect = 25f;
        public float Humidity_FullMargin = 2.5f;
        public float Humidity_EarningMargin = 7.0f;

        #endregion
    }
}
