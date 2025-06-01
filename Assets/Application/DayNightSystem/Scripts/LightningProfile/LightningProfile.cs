using UnityEngine;

[CreateAssetMenu(fileName = "LightingProfile", menuName = "DayNight/Lighting Profile")]
public class LightingProfile : ScriptableObject
{
    [Header("Light")]
    public Color sunColor = Color.white;
    public float sunIntensity = 1.0f;
    public float temperature = 6500f;

    [Header("Ambient")]
    public Color ambientColor = Color.gray;
    public float ambientIntensity = 1.0f;

    [Header("Fog (HDRP Sky & Fog Volume)")]
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;

    [Header("Post Process")]
    public float exposure = 1.0f;
}
