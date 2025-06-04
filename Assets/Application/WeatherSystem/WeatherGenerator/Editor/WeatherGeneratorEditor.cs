using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeatherGenerator))]
public class WeatherGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var weatherGenerator = (WeatherGenerator)target;

        if (GUILayout.Button("Generate Weathers Data with selected params"))
        {
            GenerateTimelineAsset(weatherGenerator);
        }
    }

    private void GenerateTimelineAsset(WeatherGenerator generator)
    {
        if (generator == null)
        {
            Debug.LogError("Le WeatherGenerator est null.");
            return;
        }

        generator.FillTimeline(
            generator.MinWeathersDuration,
            generator.MaxWeathersDuration
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
