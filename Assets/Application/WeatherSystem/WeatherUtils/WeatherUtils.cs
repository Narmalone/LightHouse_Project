using UnityEngine;

public static class WeatherUtils 
{

    public static WeatherData GetWeatherAt(byte day, float hour, WeatherTimeline timeline, TimeConfiguration config)
    {
        float secondsPerDay = config.GetTotalSecondsPerDay();
        float targetTime = day * secondsPerDay + (hour / 24f) * secondsPerDay;

        var weathers = timeline.weathers;
        for (int i = 0; i < weathers.Count - 1; i++)
        {
            var current = weathers[i];
            var next = weathers[i + 1];

            if (targetTime >= current.StartTimeInSeconds && targetTime <= current.StartTimeInSeconds + current.DurationInSeconds)
            {
                float localTime = targetTime - current.StartTimeInSeconds;
                float t = Mathf.Clamp01(localTime / current.DurationInSeconds);
                return LerpWeatherData(current, next, t);
            }
        }

        // Par défaut : dernier état météo
        return weathers.Count > 0 ? weathers[weathers.Count - 1] : null;
    }


    public static WeatherData LerpWeatherData(WeatherData from, WeatherData to, float t)
    {
        float rawOrientation = Mathf.LerpAngle(from.WindOrientation, to.WindOrientation, t);
        float normalizedOrientation = (rawOrientation % 360f + 360f) % 360f;

        return new WeatherData
        {
            WeatherType = t < 0.5f ? from.WeatherType : to.WeatherType,
            StartTimeInSeconds = Mathf.Lerp(from.StartTimeInSeconds, to.StartTimeInSeconds, t),
            DurationInSeconds = Mathf.Lerp(from.DurationInSeconds, to.DurationInSeconds, t),
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
        angle = angle % 360f;
        int sector = Mathf.RoundToInt(angle / 45f) % 8;
        return (WindOrientationType)sector;
    }

    public static WeatherType DetermineWeatherType(
        float humidity,
        float pressure,
        float airTemp,
        float windSpeed,
        float waterTemp,
        WeatherDefinitionDatabase definitionDatabase)
    {
        WeatherType bestMatch = WeatherType.Sunny;
        float bestScore = float.MinValue;

        foreach (var def in definitionDatabase.Definitions)
        {
            float score = 0f;

            // On compare à chaque plage et on ajoute des points si la valeur est dans l’intervalle
            score += IsInRange(humidity, def.HumidityRange) ? 1f : -1f;
            score += IsInRange(pressure, def.PressureRange) ? 1f : -1f;
            score += IsInRange(windSpeed, def.WindSpeedRange) ? 1f : -1f;
            score += IsInRange(airTemp, def.AirTemperatureRange) ? 1f : -1f;
            score += IsInRange(waterTemp, def.WaterTemperatureRange) ? 0.5f : -0.5f; // moins important

            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = def.Type;
            }
        }

        return bestMatch;
    }

    private static bool IsInRange(float value, Vector2 range)
    {
        return value >= range.x && value <= range.y;
    }

}
