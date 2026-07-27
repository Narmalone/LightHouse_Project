using UnityEngine;

[CreateAssetMenu(fileName = "RainConfiguration", menuName = "ScriptableObjects/Weather/Rain Configuration")]
public class RainConfigurationSO : ScriptableObject
{
    [Header("Wind")]
    public float minWindSpeed = 0f;
    public float maxWindSpeed = 200f;

    public float minWindAngle = 0f;
    public float maxWindAngle = 60f;

    [Header("Rain Velocity")]
    public Vector3 rainMinVelocity = new Vector3(0f, -45f, 0f);
    public Vector3 rainMaxVelocity = new Vector3(0f, -60f, 0f);

    [Header("Humidity")]
    [Range(0, 100)] public float humidityRainStart = 70f;
    [Range(0, 100)] public float humidityRainFull = 95f;

    [Header("Emission")]
    [Range(0, 10000)] public float maxRainOverTime = 2500f;

    [Header("Noise")]
    [Range(0, 10)] public float noiseInitialStrength = 0f;
    [Range(0, 10)] public float noiseMaxStrength = 0.5f;

    [Range(0, 2)] public float noiseInitialFrequency = 0.2f;
    [Range(0, 2)] public float noiseMaxFrequency = 0.8f;

    [Range(0, 2)] public float noiseInitialScrollSpeed = 0f;
    [Range(0, 2)] public float noiseMaxScrollSpeed = 0.8f;
}