using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class OceanController : MonoBehaviour
{
    [SerializeField] private WaterSurface _water;

    private OceanSettings _oldOceanSettings;
    [SerializeField] private OceanSettings _calmSettings;
    [SerializeField] private OceanSettings _sunnySettings;
    [SerializeField] private OceanSettings _rainySettings;
    [SerializeField] private OceanSettings _stormySettings;
    [SerializeField] private OceanSettings _windySettings;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;

    private WeatherManager _weatherManager;

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

    IEnumerator LerpOceanSettingsCoroutine(OceanSettings startSettings, OceanSettings endSettings, float duration)
    {
        //bools
        _water.ripples = endSettings.EnableRipples;

        _water.caustics = endSettings.EnableCaustic;
        _water.causticsBand = endSettings.SimulationBand;
        _water.causticsResolution = endSettings.CausticsResolution;

        _water.underWater = endSettings.EnableUnderWater;
        _water.volumePrority = endSettings.VolumePriority;

        while (_lerpTime < duration)
        {
            //EGALEMENT TESTER CERTAINES VARIABLES QUON PEUT VIA CODE ET PAS VIA EDITEUR ?
            float t = _lerpTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep function

            _water.timeMultiplier = Mathf.Lerp(startSettings.TimeMultiplier, endSettings.TimeMultiplier, t);
            _water.repetitionSize = Mathf.Lerp(startSettings.RepetitionSize, endSettings.RepetitionSize, t);
            _water.largeWindSpeed = Mathf.Lerp(startSettings.DistantWindSpeed, endSettings.DistantWindSpeed, t);
            _water.largeWindOrientationValue = Mathf.Lerp(startSettings.DistantWindOrientation, endSettings.DistantWindOrientation, t);

            _water.largeChaos = Mathf.Lerp(startSettings.SwellChaos, endSettings.SwellChaos, t);
            
            //CURRENT NON TROUVE DONC CHAOSSPEED & CHOS ORIENTATION

            //BAND 1 et BAND 2
            _water.largeBand0Multiplier = Mathf.Lerp(startSettings.AmplitudeAttenuationFirst, endSettings.AmplitudeAttenuationFirst, t);
            _water.largeBand1Multiplier = Mathf.Lerp(startSettings.AmplitudeAttenuationSecond, endSettings.AmplitudeAttenuationSecond, t);

            //ripples
            if (endSettings.EnableRipples)
            {
                _water.ripplesWindSpeed = Mathf.Lerp(startSettings.LocalWindSpeed, endSettings.LocalWindSpeed, t);
                _water.ripplesWindOrientationValue = Mathf.Lerp(startSettings.LocalWindOrientation, endSettings.LocalWindOrientation, t);
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
            if (endSettings.EnableRipples)
            {
                _water.causticsPlaneBlendDistance = Mathf.Lerp(startSettings.VirtualPlaneDistance, endSettings.VirtualPlaneDistance, t);
            }

            //UNDERWATER
            if (endSettings.EnableUnderWater)
            {
                _water.volumeDepth = Mathf.Lerp(startSettings.VolumeDepth, endSettings.VolumeDepth, t);
                _water.transitionSize = Mathf.Lerp(startSettings.TransitionSize, endSettings.TransitionSize, t);
                _water.absorbtionDistanceMultiplier = Mathf.Lerp(startSettings.AbsorbtionDistanceMultiplier, endSettings.AbsorbtionDistanceMultiplier, t);
            }

            _lerpTime += Time.deltaTime;
            yield return null;
        }
    }

}
