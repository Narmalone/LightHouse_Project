using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Ocean Configuration")]
public class OceanConfiguration : ScriptableObject
{
    [Header("Plages météo")]
    public float windMin = 0f;
    public float windMax = 30f;

    public float humidityMin = 0f;
    public float humidityMax = 100f;

    public float temperatureMin = -5f;
    public float temperatureMax = 35f;

    public float pressureMin = 980f;
    public float pressureMax = 1030f;

    public float timeMin = 5f;
    public float timeMax = 20f;

    public float sunIntensityMax = 1.5f;

    [Header("Chaos général")]
    public float chaosBase = 0.4f;
    public float chaosWindFactor = 0.6f;
    public float chaosPressureFactor = 0.8f;

    [Header("Large Bands")]
    public Vector2 band0Range = new Vector2(0.6f, 1.3f);
    public Vector2 band1Range = new Vector2(0.4f, 1.0f);

    [Header("Ripples")]
    public float maxRipplesWind = 8f;

    // Ripples Chaos
    public float ripplesChaosBase = 0.3f;
    public float ripplesChaosWindFactor = 0.4f;
    public float ripplesChaosHumidityFactor = 0.3f;

    // Ripples Fade
    public Vector2 fadeStartRange = new Vector2(30f, 60f);
    public Vector2 fadeDistanceRange = new Vector2(80f, 200f);

    [Header("Scattering Color")]
    public Color scatteringDark = new Color(0.05f, 0.1f, 0.2f);
    public Color scatteringLight = new Color(0.2f, 0.4f, 0.55f);
    public Color scatteringHazy = new Color(0.4f, 0.4f, 0.45f);

    [Header("Refraction Color")]
    public Color refractionCold = new Color(0.2f, 0.3f, 0.35f);
    public Color refractionWarm = new Color(0.4f, 0.6f, 0.7f);

    [Header("Foam Color")]
    public Color foamLowWind = new Color(0.75f, 0.85f, 0.9f);
    public Color foamHighWind = Color.white;

    [Header("Absorption")]
    public Vector2 absorptionRange = new Vector2(5f, 1f);

    [Header("Scattering HDRP")]
    public Vector2 ambientScatteringRange = new Vector2(0.1f, 0.25f);
    public Vector2 heightScatteringRange = new Vector2(0.1f, 0.3f);
    public Vector2 displacementScatteringRange = new Vector2(0.15f, 0.4f);
}
