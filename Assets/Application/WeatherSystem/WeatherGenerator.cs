using UnityEngine;

[CreateAssetMenu(fileName = "Generator_Default", menuName = "Lighthouse/Weather/New Generator")]
public class WeatherGenerator : ScriptableObject
{
    public TimeConfiguration TimeConfig;
    public float MinWeathersDuration;
    public float MaxWeathersDuration;

    public WeatherTimeline GenerateRandomTimeline(float minDuration, float maxDuration)
    {
        var timeline = ScriptableObject.CreateInstance<WeatherTimeline>();
        float current = 0f;
        float totalTime = TimeConfig.GetTotalGameTimeInSeconds();

        while (current < totalTime)
        {
            float duration = Random.Range(minDuration, maxDuration);

            // Clamp la fin pour éviter de dépasser légèrement
            if (current + duration > totalTime)
            {
                duration = totalTime - current;
            }

            var evt = new WeatherData
            {
                WeatherType = (WeatherType)Random.Range(0, System.Enum.GetValues(typeof(WeatherType)).Length),
                Humidity = Random.Range(0, 100),
                StartTimeInSeconds = current,
                DurationInSeconds = duration
            };

            timeline.weathers.Add(evt);
            current += duration;
        }

        return timeline;
    }
}
