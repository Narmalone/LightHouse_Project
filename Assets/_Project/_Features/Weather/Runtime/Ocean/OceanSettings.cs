using LightHouse.EditorTools.SuperGameManager;
using UnityEngine;

namespace LightHouse.Features.Weather.Ocean
{
    [CreateAssetMenu(menuName = "Environment/Ocean Configuration")]
    public class OceanConfiguration : ScriptableObject
    {
        [Header("Plages météo")]
        [SgmExpose(label: "Wind Min when weather is at 0 kmph")]
        public float windMin = 0f;
        [SgmExpose(label: "Wind Max when weather is at 0 kmph")]
        public float windMax = 30f;

        [SgmExpose(label: "Min Humidity")]
        public float humidityMin = 0f;
        [SgmExpose(label: "Max Humidity")]
        public float humidityMax = 100f;

        [SgmExpose(label: "Min Water Temp")]
        public float temperatureMin = -5f;
        [SgmExpose(label: "Max Water Temp")]
        public float temperatureMax = 35f;

        [SgmExpose(label: "Min Pressure")]
        public float pressureMin = 980f;
        [SgmExpose(label: "Max Pressure")]
        public float pressureMax = 1030f;

        public float timeMin = 5f;
        public float timeMax = 20f;

        [SgmExpose(label: "Sun Max Intensity")]
        public float sunIntensityMax = 1.5f;

        [Header("Chaos général")]
        [SgmExpose(label: "Chaos Base")]
        public float chaosBase = 0.4f;
        [SgmExpose(label: "Chaos Wind Factor")]
        public float chaosWindFactor = 0.6f;
        [SgmExpose(label: "Chaos Pressure Factor")]
        public float chaosPressureFactor = 0.8f;

        [Header("Large Bands")]
        [SgmExpose(label: "Band0 Range")]
        public Vector2 band0Range = new Vector2(0.6f, 1.3f);
        [SgmExpose(label: "Band1 Range")]
        public Vector2 band1Range = new Vector2(0.4f, 1.0f);

        [Header("Ripples")]
        [SgmExpose(label: "Max Ripples Wind")]
        public float maxRipplesWind = 8f;

        // Ripples Chaos
        [SgmExpose(label: "Ripples Chaos Base")]
        public float ripplesChaosBase = 0.3f;

        [SgmExpose(label: "Ripples Chaos Wind Factor")]
        public float ripplesChaosWindFactor = 0.4f;
        [SgmExpose(label: "Ripples Chaos Humidity Factor")]
        public float ripplesChaosHumidityFactor = 0.3f;

        // Ripples Fade
        [SgmExpose(label: "Fade Start Range")]
        public Vector2 fadeStartRange = new Vector2(30f, 60f);
        [SgmExpose(label: "Fade Distance Range")]
        public Vector2 fadeDistanceRange = new Vector2(80f, 200f);

        [Header("Scattering Color")]
        [SgmExpose(label: "Scattering Dark Color")]
        public Color scatteringDark = new Color(0.05f, 0.1f, 0.2f);
        [SgmExpose(label: "Scattering Light Color")]
        public Color scatteringLight = new Color(0.2f, 0.4f, 0.55f);
        [SgmExpose(label: "Scattering Hazy Color")]
        public Color scatteringHazy = new Color(0.4f, 0.4f, 0.45f);

        [Header("Refraction Color")]
        [SgmExpose(label: "Refraction Cold Color")]
        public Color refractionCold = new Color(0.2f, 0.3f, 0.35f);
        [SgmExpose(label: "Refraction Warm Color")]
        public Color refractionWarm = new Color(0.4f, 0.6f, 0.7f);

        [Header("Foam Color")]
        [SgmExpose(label: "Foam Low Wind Color")]
        public Color foamLowWind = new Color(0.75f, 0.85f, 0.9f);
        [SgmExpose(label: "Foam High Wind Color")]
        public Color foamHighWind = Color.white;

        [Header("Absorbtion")]
        [SgmExpose(label: "Absorbtion Range")]
        public Vector2 absorptionRange = new Vector2(5f, 1f);

        [Header("Scattering HDRP")]
        [SgmExpose(label: "Ambient Scattering Range")]
        public Vector2 ambientScatteringRange = new Vector2(0.1f, 0.25f);
        [SgmExpose(label: "Height Scattering Range")]
        public Vector2 heightScatteringRange = new Vector2(0.1f, 0.3f);
        [SgmExpose(label: "Displacement Scattering Range")]
        public Vector2 displacementScatteringRange = new Vector2(0.15f, 0.4f);
    }

}
