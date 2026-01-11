using LightHouse.Features.Weather;
using TMPro;
using UnityEngine;

namespace LightHouse.Features.PlaceHolders.Weather
{
    public class PlaceHolder_AirTempHygroBaroUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _airTemperatureTxt;
        [SerializeField] private TextMeshProUGUI _hygrometerTxt;
        [SerializeField] private TextMeshProUGUI _barometerTxt;

        private void LateUpdate()
        {
            if (WeatherHandlerData.CurrentWeather == null) return;
            _airTemperatureTxt.text = "Air Temp: " + WeatherHandlerData.CurrentWeather.AirTemperature.ToString("#.00") + " °C";
            _barometerTxt.text = "Pressure: " + WeatherHandlerData.CurrentWeather.AtmosphericPressure.ToString("#.00") + " hPa";
            _hygrometerTxt.text = "Humidity: " + WeatherHandlerData.CurrentWeather.Humidity.ToString("#.00") + " %";
        }
    }
}
