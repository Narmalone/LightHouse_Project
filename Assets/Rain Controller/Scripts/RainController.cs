using LightHouse.Core.Audio;
using LightHouse.Core.Player;
using LightHouse.Core.Services;
using LightHouse.Features.Weather;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class RainController : MonoBehaviour
{
    #region VARIABLES

    [Header("References")]
    public Transform playerTransformLocator; // Player pos
    public GameObject rainTransformLocator; // Rain rotator and position
    public ParticleSystem rainParticleSystem;

    [Header("Configuration")]
    public WeatherDataSO weatherData; //SO data for weather information
    public RainConfigurationSO rainConfiguration; //SO data for rain configuration

    [Header("Bools")]
    public bool isRaining; // on off switch for rain
    public bool alignToWind; // Align the rain to the wind direction, if false the rain will fall straight down

    [Header("Rain")] 
    public float currentRainSpawnRate; // current rain particle spawn rate based on humidity
    [Range(0, 1)] public float RainIntensity = 0f; // based on humidity

    [Header("Audio Variation")]
    [Range(0f, 0.5f)] public float volumeVariation = 0.1f;
    public float variationSpeed = 0.3f; // variation speed for audio volume changes

    /// <summary>
    /// Les dernières valeurs de vent, d'humidité, de turbulence et de direction du vent pour détecter les changements et mettre à jour l'orientation du vent en conséquence.
    /// </summary>

    private float lastWindSpeed = -1f;
    private float lastHumidity = -1f;
    private float lastTurbulence = -1f;
    private float lastWindDirection = -1f;

    //test audio
    public SO_AudioCue LightRainCue;
    public SO_AudioCue MediumRainCue;
    public SO_AudioCue HeavyRainCue;

    private IAudioHandle lightHandle;
    private IAudioHandle mediumHandle;
    private IAudioHandle heavyHandle;

    #endregion

    #region METHODS

    static float NormalizeAngle180(float a) // Normalizes an angle to the range [-180, 180] 
    {
        a %= 360f;

        if (a > 180f) a -= 360f;

        if (a < -180f) a += 360f;

        return a;
    }

    public float CalculateWindRotation() // Calculates the wind rotation based on the current wind speed and the min/max wind angles defined in the rain configuration
    {
        return Mathf.Lerp(rainConfiguration.minWindAngle, rainConfiguration.maxWindAngle, Mathf.InverseLerp(rainConfiguration.minWindSpeed, rainConfiguration.maxWindSpeed, weatherData.windSpeed));
    }

    #endregion

    #region UNITY LIFECYCLE

    private void Awake()
    {
        PlayerHandlerData.OnHandlerInitialized += PlayerHandlerData_OnHandlerInitialized;
    }

    private void PlayerHandlerData_OnHandlerInitialized()
    {
        playerTransformLocator = PlayerHandlerData.MainPlayer.Character.transform;

        if (isRaining)
        {
            StartRainAudio();
        }
    }

    private void OnDestroy()
    {
        PlayerHandlerData.OnHandlerInitialized -= PlayerHandlerData_OnHandlerInitialized;
    }

    public void Update()
    {
        if(!PlayerHandlerData.IsInitialized || PlayerHandlerData.MainPlayer == null)
        {
            return;
        }
        

        if (isRaining)
        {

            float turbulence = WeatherHandlerData.CurrentWeather.WindSpeed / rainConfiguration.maxWindSpeed; // calculates the noise power based on wind speed

            if (alignToWind)
            {
                if (!Mathf.Approximately(lastWindDirection, WeatherHandlerData.CurrentWeather.WindOrientation)) // checks if the wind orientation has changed since the last update, if not, skip the update to avoid unnecessary calculations
                {
                    CalculateWindOrientation();
                    lastWindDirection = WeatherHandlerData.CurrentWeather.WindOrientation;
                }
            }
            else
            {
                ResetWindPos(); // resets the rain rotation to 0 if alignToWind is false
            }

            if (!Mathf.Approximately(lastWindSpeed, WeatherHandlerData.CurrentWeather.WindSpeed)) // checks if the wind speed has changed since the last update, if not, skip the update to avoid unnecessary calculations
            {
                SetWindPower();
                lastWindSpeed = WeatherHandlerData.CurrentWeather.WindSpeed;
            }

            if (!Mathf.Approximately(lastHumidity, WeatherHandlerData.CurrentWeather.Humidity)) // checks if the humidity has changed since the last update, if not, skip the update to avoid unnecessary calculations
            {
                SetRainIntensity(RainIntensityFromHumidity(WeatherHandlerData.CurrentWeather.Humidity, rainConfiguration.humidityRainStart, rainConfiguration.humidityRainFull));
                lastHumidity = WeatherHandlerData.CurrentWeather.Humidity;
            }

            if (!Mathf.Approximately(lastTurbulence, turbulence)) // checks if the turbulence has changed since the last update, if not, skip the update to avoid unnecessary calculations
            {
                SetNoise(turbulence);
                lastTurbulence = turbulence;
            }

            UpdateRainAudio();
        }

        if (rainTransformLocator.activeSelf != isRaining)
        {
            rainTransformLocator.SetActive(isRaining);
        }
    }

    public void LateUpdate()
    {
        if (isRaining)
        {
            GoToPlayerPos();
        }
    }

    #endregion

    #region PUBLIC METHODS

    public void StartRainAudio()
    {
        if (lightHandle == null || !lightHandle.IsValid)
            lightHandle = ServiceLocator.Audio.PlayAt(LightRainCue, playerTransformLocator.position,new AudioPlayOptions{Owner = rainTransformLocator, FollowTransform = true});

        if (mediumHandle == null || !mediumHandle.IsValid)
            mediumHandle = ServiceLocator.Audio.PlayAt(MediumRainCue, playerTransformLocator.position, new AudioPlayOptions { Owner = rainTransformLocator, FollowTransform = true });

        if (heavyHandle == null || !heavyHandle.IsValid)
            heavyHandle = ServiceLocator.Audio.PlayAt(HeavyRainCue, playerTransformLocator.position, new AudioPlayOptions { Owner = rainTransformLocator, FollowTransform = true });

        lightHandle.SetVolume(0f);
        mediumHandle.SetVolume(0f);
        heavyHandle.SetVolume(0f);
    }

    public void UpdateRainAudio()
    {
        if (lightHandle == null || !lightHandle.IsValid)
            return;

        float t = RainIntensity;
        float turbulence = WeatherHandlerData.CurrentWeather.WindSpeed / rainConfiguration.maxWindSpeed;
        float time = Time.time * variationSpeed;

        float lightVolume; // fade in 0% to 20%, full volume 20% to 40%, fade out 40% to 60%, off 60% to 100%
        if (t < 0.2f)
            lightVolume = Mathf.InverseLerp(0f, 0.2f, t);
        else if (t < 0.4f)
            lightVolume = 1f;
        else if (t < 0.6f)
            lightVolume = 1f - Mathf.InverseLerp(0.4f, 0.6f, t);
        else
            lightVolume = 0f;

        float lightVariation = 1f + ((Mathf.PerlinNoise(1, time) - 0.5f) * 2f) * volumeVariation * (1 + turbulence);

        lightHandle.SetVolume(Mathf.Clamp01(lightVolume * lightVariation));

        float mediumVolume; // fade in 0% to 40%, full volume 40% to 60%, fade out 60% to 80%, off 80% to 100%
        if (t < 0.4f)
            mediumVolume = 0f;
        else if (t < 0.6f)
            mediumVolume = Mathf.InverseLerp(0.4f, 0.6f, t);
        else if (t < 0.8f)
            mediumVolume = 1f;
        else
            mediumVolume = 1f - Mathf.InverseLerp(0.8f, 1f, t);

        float mediumVariation = 1f + ((Mathf.PerlinNoise(1, time) - 0.5f) * 2f) * volumeVariation * (1 + turbulence);

        mediumHandle.SetVolume(Mathf.Clamp01(mediumVolume * mediumVariation));

        float heavyVolume; // fade in 60% to 80%, full volume 80% to 100%
        if (t < 0.6f)
            heavyVolume = 0f;
        else if (t < 0.8f)
            heavyVolume = Mathf.InverseLerp(0.6f, 0.8f, t);
        else
            heavyVolume = 1f;

        float heavyVariation = 1f + ((Mathf.PerlinNoise(1, time) - 0.5f) * 2f) * volumeVariation * (1 + turbulence);

        heavyHandle.SetVolume(Mathf.Clamp01(heavyVolume * heavyVariation));
    }

    public void StopRainAudio()
    {
        lightHandle?.Stop(1f);
        mediumHandle?.Stop(1f);
        heavyHandle?.Stop(1f);

        lightHandle = null;
        mediumHandle = null;
        heavyHandle = null;
    }

    public void GoToPlayerPos() // Moves the rain transform locator to the player's position
    {
        if (playerTransformLocator != null)
        {
            Vector3 playerPosition = playerTransformLocator.position;
            rainTransformLocator.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);   
        }
    }

    public void CalculateWindOrientation() // Calculates the wind orientation based on the current wind speed and the min/max wind angles
    {
        if (rainTransformLocator == null || weatherData == null)
        {
            return;
        }
        
        rainTransformLocator.transform.rotation = Quaternion.Euler(CalculateWindRotation(), NormalizeAngle180(WeatherHandlerData.CurrentWeather.WindOrientation), 0f);
    }

    public WeatherDataSO.WindOrientationType GetWindOrientation() // Returns the wind orientation as an enum value based on the current wind orientation in degrees
    {
        float angle = WeatherHandlerData.CurrentWeather.WindOrientation % 360f;

        if (angle < 0f)
        {
            angle += 360f;
        }

        int index = Mathf.RoundToInt(angle / 45f) % 8;

        return (WeatherDataSO.WindOrientationType)index;
    }

    public void ResetWindPos() // Resets the rain rotation to 0 if alignToWind is false
    {
        if (rainTransformLocator == null)
        {
            return;
        }

        rainTransformLocator.transform.rotation = Quaternion.identity;
    }

    static float RainIntensityFromHumidity(float humidity, float min, float max) // Calculates the rain intensity based on the current humidity and the min/max humidity
    {
        if (humidity < min)
        {
            return 0f;
        }
        else if (humidity >= max)
        {
            return 1f;
        }
        else
        {
            return Mathf.InverseLerp(min, max, humidity);
        }
    }

    public void SetRainIntensity(float intensity) // Sets the rain intensity based on the current humidity and the min/max humidity
    {
        RainIntensity = Mathf.Clamp01(intensity);
        currentRainSpawnRate = Mathf.Lerp(0f, rainConfiguration.maxRainOverTime, RainIntensity);

        var emission = rainParticleSystem.emission;
        emission.rateOverTime = currentRainSpawnRate;
    }

    public void SetWindPower() // Sets the wind power based on the current wind speed and the min/max wind speeds
    {
        var velocity = rainParticleSystem.velocityOverLifetime;
        velocity.enabled = true;
        
        velocity.y = Mathf.Lerp(rainConfiguration.rainMinVelocity.y, rainConfiguration.rainMaxVelocity.y, Mathf.InverseLerp(rainConfiguration.minWindSpeed, rainConfiguration.maxWindSpeed, WeatherHandlerData.CurrentWeather.WindSpeed));
    }

    public void SetNoise(float turbulence) // turbulence is a value between 0 and 1, where 0 is no turbulence and 1 is maximum turbulence
    {
        var noise = rainParticleSystem.noise; // Get the noise module of the particle system
        noise.enabled = turbulence > 0f;
        noise.separateAxes = true;

        noise.strengthY = 0f; //noise strength de Y a 0 sinon accelere la pluie vers le bas
        noise.strengthX = Mathf.Lerp(rainConfiguration.noiseInitialStrength, rainConfiguration.noiseMaxStrength, turbulence); //noise strength de X
        noise.strengthZ = Mathf.Lerp(rainConfiguration.noiseInitialStrength, rainConfiguration.noiseMaxStrength, turbulence); //noise strength de Z
        noise.frequency = Mathf.Lerp(rainConfiguration.noiseInitialFrequency, rainConfiguration.noiseMaxFrequency, turbulence); //noise frequency
        noise.scrollSpeed = Mathf.Lerp(rainConfiguration.noiseInitialScrollSpeed, rainConfiguration.noiseMaxScrollSpeed, turbulence); //noise speed
    }

    #endregion
}