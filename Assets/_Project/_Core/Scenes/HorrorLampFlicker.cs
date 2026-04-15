using System.Collections;
using UnityEngine;

public class HorrorLampFlicker : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer rend;
    public int materialIndex = 2;

    [Header("Light")]
    public Light lamp;
    public float baseLumens = 20000f;
    public float minLumensPercent = 0.3f;
    public float maxLumensPercent = 1.2f;

    [Header("Temperature (Kelvin)")]
    public float baseTemperature = 6500f;
    public float flickerTemperatureOffset = 800f;

    [Header("Emission")]
    public Color baseColor = new Color(1f, 0.85f, 0.6f);
    public float baseEmissionIntensity = 10f;
    public float minEmissionIntensity = 0f;

    [Header("Flick Timing")]
    public float minTimeBetweenFlicks = 1f;
    public float maxTimeBetweenFlicks = 4f;
    public float minFlickDuration = 0.03f;
    public float maxFlickDuration = 0.15f;

    [Header("Flick Strength")]
    public float minFlickPercent = 0.2f;
    public float maxFlickPercent = 1.2f;

    [Header("Behavior")]
    public float smoothSpeed = 15f;
    public float chanceDoubleFlick = 0.3f;
    public float chanceBigDrop = 0.1f;

    private Material mat;

    private float currentPercent;
    private float targetPercent;

    void Start()
    {
        mat = rend.materials[materialIndex];

        currentPercent = 1f;
        targetPercent = 1f;

        StartCoroutine(FlickLoop());
    }

    void Update()
    {
        // Smooth
        currentPercent = Mathf.Lerp(currentPercent, targetPercent, Time.deltaTime * smoothSpeed);

        // ===== LIGHT (Lumens) =====
        float lumens = baseLumens * currentPercent;
        lamp.intensity = lumens;

        // ===== TEMPERATURE =====
        float tempOffset = (1f - currentPercent) * flickerTemperatureOffset;
        lamp.colorTemperature = baseTemperature - tempOffset;

        // ===== EMISSION =====
        float emission = Mathf.Lerp(minEmissionIntensity, baseEmissionIntensity, currentPercent);

        Color finalColor = baseColor * emission;

        mat.SetColor("_EmissiveColor", finalColor);
    }

    IEnumerator FlickLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenFlicks, maxTimeBetweenFlicks));
            yield return StartCoroutine(FlickEvent());
        }
    }

    IEnumerator FlickEvent()
    {
        float flickPercent = Random.Range(minFlickPercent, maxFlickPercent);

        if (Random.value < chanceBigDrop)
        {
            flickPercent = Random.Range(0f, 0.3f); // quasi extinction 😈
        }

        targetPercent = flickPercent;

        float duration = Random.Range(minFlickDuration, maxFlickDuration);
        yield return new WaitForSeconds(duration);

        targetPercent = 1f;

        // Double flick
        if (Random.value < chanceDoubleFlick)
        {
            yield return new WaitForSeconds(0.05f);

            targetPercent = Random.Range(0.2f, 0.7f);

            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));

            targetPercent = 1f;
        }
    }
}