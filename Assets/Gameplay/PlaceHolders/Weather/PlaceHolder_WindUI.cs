using LightHouse.Features.Weather;
using LightHouse.Features.Weather.Utils;
using TMPro;
using UnityEngine;

namespace LightHouse.Features.PlaceHolders.Weather
{
    public class PlaceHolder_WindUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _windSpeedTxt;
        [SerializeField] private TextMeshProUGUI _windDirectionTxt;

        private void LateUpdate()
        {
            if (WeatherHandlerData.CurrentWeather == null) return;
            _windSpeedTxt.text = "Wind Speed: " + WeatherHandlerData.CurrentWeather.WindSpeed.ToString("#.00") + " km/h";
            _windDirectionTxt.text = "Direction: " + WeatherUtils.AngleToOrientationType(WeatherHandlerData.CurrentWeather.WindOrientation).ToString();
        }
    }

}
