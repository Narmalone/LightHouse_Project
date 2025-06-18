using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class RainController : MonoBehaviour
{
    [Header("Materials and VFX")]
    public Material WaterMaterial;
    public Material WaterLensMaterial;
    public VisualEffect RainVFX;
    
    [Header("Ripple Settings")]
    [Range(0, 1)]
    public float RippleStrength = 0.3f;
    [Range(0, 1)]
    public float WaterMask = 1f;
    [Range(0, 1)]
    public float Smoothness = 0f;
    
    [Header("Rain Settings")]
    public float RainMaxIntensity = 100000f;
    [Range(0,1)]
    public float RainIntensity = 1f;
    private float CurrentRainIntensity => RainIntensity * RainMaxIntensity;
    public Vector3 RainMinVelocity = new Vector3(0, -24, 0);
    public Vector3 RainMaxVelocity = new Vector3(0, -30, 0);
    
    private static int _rippleStrengthID = Shader.PropertyToID("_RippleStrength");
    private static int _waterMaskID = Shader.PropertyToID("_Mask");
    private static int _smoothnessID = Shader.PropertyToID("_Smoothness");
    private static int _rateId = Shader.PropertyToID("Rate");
    private static int _rainMaxVelocityID = Shader.PropertyToID("Velocity2");
    private static int _rainMinVelocityID = Shader.PropertyToID("Velocity1");
    private static int _isRaining = Shader.PropertyToID("_isRaining");
    
    public AudioMixer mixer;

    private void Update()
    {
        WaterMaterial.SetFloat(_rippleStrengthID, RippleStrength);
        WaterMaterial.SetFloat(_waterMaskID, WaterMask);
        WaterMaterial.SetFloat(_smoothnessID, Smoothness);
        RainVFX.SetFloat(_rateId, CurrentRainIntensity);
        RainVFX.SetVector3(_rainMaxVelocityID, RainMaxVelocity);
        RainVFX.SetVector3(_rainMinVelocityID, RainMinVelocity);
        WaterLensMaterial.SetFloat(_isRaining, RainIntensity);
        
        if (mixer != null)
        {
            float volumeInDb = (RainIntensity <= 0.0001f) ? -80f : Mathf.Log10(RainIntensity) * 20f;
            mixer.SetFloat("RainVolume", volumeInDb);
        }
    }
}
