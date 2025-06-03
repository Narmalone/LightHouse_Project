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

        var timeline = generator.GenerateRandomTimeline(
            generator.MinWeathersDuration,
            generator.MaxWeathersDuration
        );

        // Sauvegarde dans un chemin dynamique si besoin
        string folderPath = "Assets/Application/WeatherSystem";
        string assetPath = $"{folderPath}/GeneratedTimeline.asset";

        AssetDatabase.CreateAsset(timeline, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Timeline météo générée dans : {assetPath}");
        EditorGUIUtility.PingObject(timeline);
    }
}
