using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WheaterTimeline_", menuName = "Lighthouse/WeatherSystem/New Timeline")]
public class WeatherTimeline : ScriptableObject
{
    public List<WeatherData> weathers = new List<WeatherData>();

    /*public WeatherData GetWeatherDataAt(float gameTimeSeconds)
    {
        if (weathers == null || weathers.Count == 0)
            return null;

        for (int i = 0; i < weathers.Count; i++)
        {
            var current = weathers[i];
            var next = i + 1 < weathers.Count ? weathers[i + 1] : null;

            if (gameTimeSeconds >= current.StartTimeInSeconds &&
                gameTimeSeconds < current.StartTimeInSeconds + current.DurationInSeconds)
            {
                // On est dans cette météo
                if (next != null && gameTimeSeconds > current.StartTimeInSeconds + current.DurationInSeconds - 60f)
                {
                    // Transition douce vers la prochaine si < 1min restante
                    float t = Mathf.InverseLerp(
                        current.StartTimeInSeconds + current.DurationInSeconds - 60f,
                        current.StartTimeInSeconds + current.DurationInSeconds,
                        gameTimeSeconds
                    );
                    //return WeatherData.Lerp(current, next, t);
                }

                //return WeatherData.FromWeather(current);
            }
        }

        // Si en dehors de la plage → dernière météo connue
        //return WeatherData.FromWeather(weathers[^1]);
    }*/

    public static WeatherData Lerp(WeatherData a, WeatherData b, float t)
    {
        return new WeatherData
        {
            WeatherType = t < 0.5f ? a.WeatherType : b.WeatherType,
            Humidity = Mathf.Lerp(a.Humidity, b.Humidity, t),
            Pressure = Mathf.Lerp(a.Pressure, b.Pressure, t),
            Temperature = Mathf.Lerp(a.Temperature, b.Temperature, t),
            WindSpeed = Mathf.Lerp(a.WindSpeed, b.WindSpeed, t)
        };
    }


}
