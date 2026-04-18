using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

public class HorrorLightFlicker : MonoBehaviour
{
    public Light targetLight;

    [Header("Bruit continu")]
    public float baseIntensity = 1f;
    public float noiseAmplitude = 0.2f;
    public float noiseSpeed = 1f;

    [Header("Flickers impulsifs")]
    public float minFlickerInterval = 8f;
    public float maxFlickerInterval = 20f;
    public float flickerDuration = 0.08f;
    public float flickerIntensityFactor = 0.3f;

    [Header("Double Flicker")]
    [Range(0f, 1f)] public float doubleFlickerChance = 0.3f;
    public float delayBetweenDoubleFlicker = 0.05f;

    [Header("Audio")]
    public AudioCue flickerSound;
    public AudioCue ambianceLight;

    private IAudioHandle currentAmbianceHandle;
    private IAudioHandle currentFlickerHandle;

    private float _timer;
    private float _nextFlickerTime;

    private int _remainingFlickers;
    private bool _isFlickering;
    private bool _waitingBetweenFlickers;

    private void Start()
    {
        if (!targetLight)
            targetLight = GetComponent<Light>();

        ScheduleNextFlicker();

        if (ServiceLocator.Audio != null && ambianceLight != null)
        {
            currentAmbianceHandle?.Stop();
            currentAmbianceHandle = ServiceLocator.Audio.PlayAt(ambianceLight, transform.position);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float t = Time.time;

        // ───── START FLICKER ─────
        if (!_isFlickering && t >= _nextFlickerTime)
        {
            _remainingFlickers = Random.value < doubleFlickerChance ? 2 : 1;
            StartFlicker();
        }

        // ───── ACTIVE FLICKER ─────
        if (_isFlickering)
        {
            _timer -= dt;
            targetLight.intensity = baseIntensity * flickerIntensityFactor;

            if (_timer <= 0f)
            {
                _remainingFlickers--;

                if (_remainingFlickers > 0)
                {
                    // passage en délai entre flickers
                    _isFlickering = false;
                    _waitingBetweenFlickers = true;
                    _timer = delayBetweenDoubleFlicker;
                }
                else
                {
                    EndFlicker();
                }
            }

            return;
        }

        // ───── DELAY BETWEEN DOUBLE FLICKERS ─────
        if (_waitingBetweenFlickers)
        {
            _timer -= dt;

            if (_timer <= 0f)
            {
                _waitingBetweenFlickers = false;
                StartFlicker();
            }

            return;
        }

        // ───── NORMAL NOISE ─────
        float noise = Mathf.PerlinNoise(t * noiseSpeed, 0f) * 2f - 1f;
        targetLight.intensity = baseIntensity + noise * noiseAmplitude;
    }

    private void StartFlicker()
    {
        _isFlickering = true;
        _timer = flickerDuration;

        if (flickerSound != null && ServiceLocator.Audio != null)
        {
            currentFlickerHandle?.Stop();
            currentFlickerHandle = ServiceLocator.Audio.PlayAt(flickerSound, transform.position);
        }
    }

    private void EndFlicker()
    {
        _isFlickering = false;
        _waitingBetweenFlickers = false;
        ScheduleNextFlicker();
    }

    private void ScheduleNextFlicker()
    {
        _nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
    }
}