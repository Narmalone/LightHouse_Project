using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class OceanController : MonoBehaviour
{
    [SerializeField] private WaterSurface _water;
    public bool RealisticMode = true;

    [SerializeField] private float _transitionDuration = 40f;
    [SerializeField] private float _oceanTimeMultiplier = 1.0f;
    private OceanSettings _currentOceanSettings;
    [SerializeField] private OceanSettings _calmSettings;
    [SerializeField] private OceanSettings _sunnySettings;
    [SerializeField] private OceanSettings _rainySettings;
    [SerializeField] private OceanSettings _stormySettings;
    [SerializeField] private OceanSettings _windySettings;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;
    [SerializeField] private CustomEvent_WeatherType _onWeatherOverrideStart;

    private WeatherManager _weatherManager;

    public WaterSurface WaterSurface => _water;

    private void Awake()
    {
        _onWeatherChanged.handle += OnWeatherChangedHandle;
        _onWeatherOverrideStart.handle += OnWeatherOverrideStartHandle;
    }

    private void OnDestroy()
    {
        _onWeatherChanged.handle -= OnWeatherChangedHandle;
        _onWeatherOverrideStart.handle -= OnWeatherOverrideStartHandle;
    }

    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
    }

    private void Update()
    {
        if (RealisticMode)
        {
            ApplyRealisticWaterSettings();
        }
    }

    private void ApplyRealisticWaterSettings()
    {
        float windSpeed = _weatherManager.WindSpeed;
        float maxWindSpeed = _weatherManager.MaxWindSpeed;
        float humidity = _weatherManager.Humidity;
        float airTemperature = _weatherManager.AirTemperature;
        float waterTemperature = _weatherManager.WaterTemperature;
        float windOrientation = _weatherManager.WindOrientationValue;

        // Ajustements progressifs des paramètres
        float timeFactor = _oceanTimeMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime * 0.1f;

        // Dynamique des vagues et du vent
        _water.timeMultiplier = Mathf.Lerp(_water.timeMultiplier, Mathf.Lerp(1f, 3.5f, windSpeed / maxWindSpeed), timeFactor);
        _water.largeBand0Multiplier = Mathf.Lerp(_water.largeBand0Multiplier, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * (1 - humidity / 100f)), timeFactor);
        _water.largeBand1Multiplier = Mathf.Lerp(_water.largeBand1Multiplier, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * (1 - humidity / 100f)), timeFactor);
        _water.largeOrientationValue = Mathf.Lerp(_water.largeOrientationValue, windOrientation, timeFactor);
        _water.largeChaos = Mathf.Lerp(_water.largeChaos, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * waterTemperature / 20f), timeFactor);
        _water.largeWindSpeed = Mathf.Lerp(_water.largeWindSpeed, windSpeed, timeFactor);

        // Gestion des "ripples" (vagues fines) en fonction du vent local
        _water.ripplesWindSpeed = Mathf.Lerp(_water.ripplesWindSpeed, Mathf.Lerp(0f, 15f, (windSpeed / maxWindSpeed) * 2), timeFactor);
        _water.ripplesChaos = Mathf.Lerp(_water.ripplesChaos, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * humidity / 100f), timeFactor);
        _water.ripplesOrientationValue = Mathf.Lerp(_water.ripplesOrientationValue, windOrientation, timeFactor);

        // Réfraction en fonction de la température de l'eau
        Color realisticWaterColor = GetRealisticWaterColor(airTemperature, waterTemperature, humidity);
        _water.refractionColor = Color.Lerp(_water.refractionColor, realisticWaterColor, timeFactor);

        // Scattering (diffusion de la lumière) ajusté selon les conditions
        //AdjustScatteringSettings(windSpeed, airTemperature, humidity, timeFactor);

        // Ajustements supplémentaires pour des effets comme les caustiques et la réfraction
        AdjustAdditionalWaterParameters(timeFactor);
    }

    private Color GetRealisticWaterColor(float airTemp, float waterTemp, float humidity)
    {
        // Réalisme de la couleur de l'eau : plus bleue en eaux profondes, plus claire en eaux peu profondes
        float depthFactor = Mathf.Clamp01(waterTemp / 30f); // Approche simplifiée pour simuler profondeur/température
        float clarity = Mathf.Clamp01(1f - humidity / 100f); // Plus d'humidité, moins l'eau est claire

        // Simuler une couleur réaliste : entre bleu profond et turquoise
        return Color.Lerp(Color.blue, Color.cyan, depthFactor * clarity);
    }

    private void AdjustScatteringSettings(float windSpeed, float airTemperature, float humidity, float timeFactor)
    {
        // Ajuster la diffusion de la lumière en fonction de la vitesse du vent, la température, et l'humidité
        _water.scatteringColor = Color.Lerp(_water.scatteringColor, Color.Lerp(Color.white, Color.blue, windSpeed / _weatherManager.MaxWindSpeed), timeFactor);
        _water.ambientScattering = Mathf.Lerp(_water.ambientScattering, Mathf.Clamp01(airTemperature / 30f), timeFactor);
        _water.displacementScattering = Mathf.Lerp(_water.displacementScattering, Mathf.Clamp01(humidity / 100f), timeFactor);
        _water.directLightTipScattering = Mathf.Lerp(_water.directLightTipScattering, Mathf.Lerp(0f, 1f, windSpeed / _weatherManager.MaxWindSpeed), timeFactor);
        _water.directLightBodyScattering = Mathf.Lerp(_water.directLightBodyScattering, Mathf.Lerp(0f, 1f, airTemperature / 35f), timeFactor);
    }

    private void AdjustAdditionalWaterParameters(float timeFactor)
    {
        // Exemples d'ajustements supplémentaires
        _water.smoothnessFadeStart = Mathf.Lerp(_water.smoothnessFadeStart, 0.1f, timeFactor);
        _water.smoothnessFadeDistance = Mathf.Lerp(_water.smoothnessFadeDistance, 0.5f, timeFactor);

        if (_water.caustics)
        {
            _water.causticsIntensity = Mathf.Lerp(_water.causticsIntensity, Mathf.Clamp(_weatherManager.AirTemperature / 35f, 0f, 1f), timeFactor);
            _water.causticsPlaneBlendDistance = Mathf.Lerp(_water.causticsPlaneBlendDistance, Mathf.Lerp(0.5f, 2f, _weatherManager.Humidity / 100f), timeFactor);
        }
    }

    private void OnWeatherOverrideStartHandle(WeatherType obj)
    {
        _water.largeOrientationValue = _weatherManager.currentWeather.windOrientationValue;
    }

    private void OnWeatherChangedHandle(WeatherType obj)
    {
        if (RealisticMode) return;

        OceanSettings newSettings = GetSettingsForWeatherType(obj);
        if (_currentOceanSettings == null)
        {
            _currentOceanSettings = _calmSettings;
        }

        StartCoroutine(LerpOceanSettings(_currentOceanSettings, newSettings, _transitionDuration));
        _currentOceanSettings = newSettings;
    }

    private OceanSettings GetSettingsForWeatherType(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Storm: return _stormySettings;
            case WeatherType.Windy: return _windySettings;
            case WeatherType.Rainy: return _rainySettings;
            case WeatherType.Sunny: return _sunnySettings;
            default: return _calmSettings;
        }
    }

    private IEnumerator LerpOceanSettings(OceanSettings startSettings, OceanSettings endSettings, float duration)
    {
        float lerpTime = 0f;

        while (lerpTime < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, lerpTime / duration);

            // Interpoler les paramètres de simulation et d'apparence
            _water.largeOrientationValue = Mathf.Lerp(startSettings.DistantWindOrientation, endSettings.DistantWindOrientation, t);
            _water.largeChaos = Mathf.Lerp(startSettings.SwellChaos, endSettings.SwellChaos, t);
            _water.timeMultiplier = Mathf.Lerp(startSettings.TimeMultiplier, endSettings.TimeMultiplier, t);
            _water.largeBand0Multiplier = Mathf.Lerp(startSettings.AmplitudeAttenuationFirst, endSettings.AmplitudeAttenuationFirst, t);
            _water.largeBand1Multiplier = Mathf.Lerp(startSettings.AmplitudeAttenuationSecond, endSettings.AmplitudeAttenuationSecond, t);
            _water.largeWindSpeed = Mathf.Lerp(startSettings.DistantWindSpeed, endSettings.DistantWindSpeed, t);
            _water.refractionColor = Color.Lerp(startSettings.RefractionColor, endSettings.RefractionColor, t);

            // Interpolation de la réfraction
            _water.maxRefractionDistance = Mathf.Lerp(startSettings.MaximumDistance, endSettings.MaximumDistance, t);
            _water.absorptionDistance = Mathf.Lerp(startSettings.AbsorbtionDistance, endSettings.AbsorbtionDistance, t);

            // Scattering et autres effets sous-marins
            _water.scatteringColor = Color.Lerp(startSettings.ScatteringColor, endSettings.ScatteringColor, t);
            _water.ambientScattering = Mathf.Lerp(startSettings.AmbientTerm, endSettings.AmbientTerm, t);
            _water.displacementScattering = Mathf.Lerp(startSettings.DisplacementTerm, endSettings.DisplacementTerm, t);
            _water.directLightTipScattering = Mathf.Lerp(startSettings.DirectLightTipTerm, endSettings.DirectLightTipTerm, t);
            _water.directLightBodyScattering = Mathf.Lerp(startSettings.DirectLightBodyTerm, endSettings.DirectLightBodyTerm, t);

            lerpTime += _oceanTimeMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime;
            yield return null;
        }
    }
}
