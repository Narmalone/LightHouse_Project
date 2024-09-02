using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class OceanController : MonoBehaviour
{
    [SerializeField] private WaterSurface _water;
    public bool RealisticMode = true;

    [SerializeField] private float _transitionDuration = 40f;
    private OceanSettings _oldOceanSettings;
    [SerializeField] private OceanSettings _calmSettings;
    [SerializeField] private OceanSettings _sunnySettings;
    [SerializeField] private OceanSettings _rainySettings;
    [SerializeField] private OceanSettings _stormySettings;
    [SerializeField] private OceanSettings _windySettings;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;

    private WeatherManager _weatherManager;

    public WaterSurface WaterSurface => _water;
    public OceanSettings CalmSettings => _calmSettings;
    public OceanSettings SunnySettings => _sunnySettings;
    public OceanSettings RainySettings => _rainySettings;
    public OceanSettings WindySettings => _windySettings;
    public OceanSettings StormySettings => _stormySettings;

    private float _lerpTime = 0f;

    private void Awake()
    {
        _onWeatherChanged.handle += _onWeatherChanged_handle;
    }

    private void OnDestroy()
    {
        _onWeatherChanged.handle -= _onWeatherChanged_handle;
    }

    private void _onWeatherChanged_handle(WeatherType obj)
    {
        if (RealisticMode) return;
        OceanSettings newOceanSettings = null;
        switch (obj)
        {
            case WeatherType.Calm:
                newOceanSettings = _calmSettings;
                break;
            case WeatherType.Storm:
                newOceanSettings = _stormySettings;
                break;
            case WeatherType.Windy:
                newOceanSettings = _windySettings;
                break;
            case WeatherType.Rainy:
                newOceanSettings = _rainySettings;
                break;
            case WeatherType.Sunny:
                newOceanSettings = _sunnySettings;
                break;
        }
        if (_oldOceanSettings == null)
        {
            _oldOceanSettings = _calmSettings;
        }

        //Lancer coroutine
        LerpOceanSettings(_oldOceanSettings, newOceanSettings, _transitionDuration);
        _oldOceanSettings = newOceanSettings;
    }
    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
    }

    private void LerpOceanSettings(OceanSettings startSettings, OceanSettings endSettings, float duration)
    {
        _lerpTime = 0f;

        StartCoroutine(LerpOceanSettingsCoroutine(startSettings, endSettings, duration));
    }

    private void Update()
    {
        if (!RealisticMode || _weatherManager == null) return;

        // Récupérer les paramètres météorologiques
        float maxWindSpeed = _weatherManager.MaxWindSpeed;
        float windSpeed = _weatherManager.WindSpeed;
        float humidity = _weatherManager.Humidity;
        float airTemperature = _weatherManager.AirTemperature;
        float waterTemperature = _weatherManager.WaterTemperature;

        // Simuler des changements progressifs plutôt que des changements brutaux
        float timeFactor = Time.deltaTime * 0.1f;

        // Ajuster les paramètres de l'eau en fonction de la vitesse du vent et d'autres paramètres météorologiques
        _water.timeMultiplier = Mathf.Lerp(_water.timeMultiplier, Mathf.Lerp(1f, 3.5f, windSpeed / maxWindSpeed), timeFactor);
        _water.largeBand0Multiplier = Mathf.Lerp(_water.largeBand0Multiplier, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * (1 - humidity / 100f)), timeFactor);
        _water.largeBand1Multiplier = Mathf.Lerp(_water.largeBand1Multiplier, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * (1 - humidity / 100f)), timeFactor);

        // L'orientation des vagues peut être influencée par le vent et la température de l'air
        _water.largeOrientationValue = Mathf.Lerp(_water.largeOrientationValue, Mathf.Lerp(0f, 360f, windSpeed / maxWindSpeed * airTemperature / 30f), timeFactor);

        // Le chaos dans les vagues est lié à la vitesse du vent et à la température de l'eau
        _water.largeChaos = Mathf.Lerp(_water.largeChaos, Mathf.Lerp(0f, 1f, windSpeed / maxWindSpeed * waterTemperature / 20f), timeFactor);

        // Ajustement de la vitesse du vent appliquée à la surface de l'eau
        _water.largeWindSpeed = Mathf.Lerp(_water.largeWindSpeed, windSpeed, timeFactor);

        // Ajustements des paramètres de scintillement ou autres effets
        AdjustAdditionalWaterParameters(timeFactor);
    }

    // Méthode pour ajuster d'autres paramètres comme les ondulations, caustiques, etc.
    private void AdjustAdditionalWaterParameters(float timeFactor)
    {
        // Exemples d'ajustements supplémentaires
        _water.smoothnessFadeStart = Mathf.Lerp(_water.smoothnessFadeStart, 0.1f, timeFactor);
        _water.smoothnessFadeDistance = Mathf.Lerp(_water.smoothnessFadeDistance, 0.5f, timeFactor);

        // Ajuster la réfraction en fonction de la température de l'eau
        _water.refractionColor = Color.Lerp(_water.refractionColor, Color.Lerp(Color.blue, Color.cyan, _weatherManager.WaterTemperature / 25f), timeFactor);

        // Ajustements pour les caustiques et l'absorption de la lumière sous-marine
        if (_water.caustics)
        {
            _water.causticsIntensity = Mathf.Lerp(_water.causticsIntensity, Mathf.Clamp(_weatherManager.AirTemperature / 35f, 0f, 1f), timeFactor);
            _water.causticsPlaneBlendDistance = Mathf.Lerp(_water.causticsPlaneBlendDistance, Mathf.Lerp(0.5f, 2f, _weatherManager.Humidity / 100f), timeFactor);
        }
    }


    IEnumerator LerpOceanSettingsCoroutine(OceanSettings startSettings, OceanSettings endSettings, float duration)
    {
        //bools
        _water.ripples = endSettings.EnableRipples;

        _water.caustics = endSettings.EnableCaustic;
        _water.causticsBand = endSettings.SimulationBand;
        _water.causticsResolution = endSettings.CausticsResolution;

        _water.underWater = endSettings.EnableUnderWater;
        _water.volumePrority = endSettings.VolumePriority;

        _water.repetitionSize = endSettings.RepetitionSize;

        while (_lerpTime < duration)
        {
            //EGALEMENT TESTER CERTAINES VARIABLES QUON PEUT VIA CODE ET PAS VIA EDITEUR ?
            float t = _lerpTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep function

            _water.largeOrientationValue = Mathf.Lerp(startSettings.DistantWindOrientation, endSettings.DistantWindOrientation, t);
            _water.largeChaos = Mathf.Lerp(startSettings.SwellChaos, endSettings.SwellChaos, t);
            _water.timeMultiplier = Mathf.Lerp(startSettings.TimeMultiplier, endSettings.TimeMultiplier, t);
            //BAND 1 et BAND 2
            _water.largeBand0Multiplier = Mathf.Lerp(startSettings.AmplitudeAttenuationFirst, endSettings.AmplitudeAttenuationFirst, t);
            _water.largeBand1Multiplier = Mathf.Lerp(startSettings.AmplitudeAttenuationSecond, endSettings.AmplitudeAttenuationSecond, t);

            _water.largeWindSpeed = Mathf.Lerp(startSettings.DistantWindSpeed, endSettings.DistantWindSpeed, t);
            //CURRENT NON TROUVE DONC CHAOSSPEED & CHOS ORIENTATION

            //ripples
            if (endSettings.EnableRipples)
            {
                _water.ripplesWindSpeed = Mathf.Lerp(startSettings.LocalWindSpeed, endSettings.LocalWindSpeed, t);
                _water.ripplesOrientationValue = Mathf.Lerp(startSettings.LocalWindOrientation, endSettings.LocalWindOrientation, t);
                _water.ripplesChaos = Mathf.Lerp(startSettings.RippleChaos, endSettings.RippleChaos, t);
            }

            //Smoothness
            _water.smoothnessFadeStart = Mathf.Lerp(startSettings.FadeRangeStart, endSettings.FadeRangeStart, t);
            _water.smoothnessFadeDistance = Mathf.Lerp(startSettings.FadeRangeStart, endSettings.FadeRangeStart, t);

            //Refraction
            _water.refractionColor = Color.Lerp(startSettings.RefractionColor, endSettings.RefractionColor, t);
            _water.maxRefractionDistance = Mathf.Lerp(startSettings.MaximumDistance, endSettings.MaximumDistance, t);
            _water.absorptionDistance = Mathf.Lerp(startSettings.AbsorbtionDistance, endSettings.AbsorbtionDistance, t);

            //SCAT
            _water.scatteringColor = Color.Lerp(startSettings.ScatteringColor, endSettings.ScatteringColor, t);
            _water.ambientScattering = Mathf.Lerp(startSettings.AmbientTerm, endSettings.AmbientTerm, t);
            _water.heightScattering = Mathf.Lerp(startSettings.HeightTerm, endSettings.HeightTerm, t);
            _water.displacementScattering = Mathf.Lerp(startSettings.DisplacementTerm, endSettings.DisplacementTerm, t);
            _water.directLightTipScattering = Mathf.Lerp(startSettings.DirectLightTipTerm, endSettings.DirectLightTipTerm, t);
            _water.directLightBodyScattering = Mathf.Lerp(startSettings.DirectLightBodyTerm, endSettings.DirectLightBodyTerm, t);

            //CAUSTIC
            if (endSettings.EnableCaustic)
            {
                _water.causticsIntensity = Mathf.Lerp(startSettings.CausticIntensity, endSettings.CausticIntensity, t);
                _water.causticsPlaneBlendDistance = Mathf.Lerp(startSettings.VirtualPlaneDistance, endSettings.VirtualPlaneDistance, t);
            }

            //UNDERWATER
            if (endSettings.EnableUnderWater)
            {
                _water.volumeDepth = Mathf.Lerp(startSettings.VolumeDepth, endSettings.VolumeDepth, t);
                _water.absorptionDistanceMultiplier = Mathf.Lerp(startSettings.AbsorbtionDistanceMultiplier, endSettings.AbsorbtionDistanceMultiplier, t);
            }

            _lerpTime += Time.deltaTime;
            yield return null;
        }
    }

    public void OverrideSettings(OceanSettings settings)
    {

    }

}
