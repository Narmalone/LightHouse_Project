using LightHouse.Weather;
using TMPro;
using UnityEngine;

public class PlaceHolder_WaterTempUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _waterTempTxt;

    private void LateUpdate()
    {
        if (WeatherHandlerData.CurrentWeather == null) return;
        _waterTempTxt.text = "Water Temp: " + WeatherHandlerData.CurrentWeather.WaterTemperature.ToString("#.00") + " °C";
    }
}
