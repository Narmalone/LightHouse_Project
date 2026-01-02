using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class FireController : MonoBehaviour
{
    
    [Header("VFX")]
    public VisualEffect FireVFX;
    
    [Header("Global Settings")]
    public float FireMaxSize = 10f;
    [Range(0, 1)]
    public float FireIntensity = 1f;
    
    [Header("Fire Settings")]
    public float FireRate = 32f;
    public float FireLifeTimeMin = 1f;
    public float FireLifeTimeMax = 3f;
    public float FirePivotY = -0.5f;
    public float FireFrameRate = 35f;
    public Gradient FireColor = new Gradient();
    public float FireScaleY = 1f;
    public float FireScaleX = 1f;
    public Vector3 FireAngle = new Vector3(0, 0, 0);

    [Header("Smoke Settings")]
    public float SmokeRate = 50f;
    public float SmokeLifeTimeMin = 3f;
    public float SmokeLifeTimeMax = 5f;
    public Vector3 SmokeVelocity1 = new Vector3(-0.1f, 0.2f, -0.333f);
    public Vector3 SmokeVelocity2 = new Vector3(0.1f, 10f, 0.333f);
    public float SmokeGravityY = 0.5f;
    public float SmokeFrameRate = 32f;
    //public float SmokeSize = 10f;
    public float SmokeScaleX = 1f;
    public float SmokeScaleY = 1f;
    public Gradient SmokeColor = new Gradient();
    public Vector3 SmokePosition = new Vector3(0f, 7.5f, 0f);
    
    [Header("Sparkles Settings")]
    public float SparklesRate = 64f;
    public float SparklesLifeTimeMin = 1f;
    public float SparklesLifeTimeMax = 2f;
    public float SparklesVelocityMin = 4.8f;
    public float SparklesVelocityMax = 7.5f;
    public float SparklesScaleX = 0.5f;
    //public float SparklesSize = 5;
    public Gradient SparklesColor = new Gradient();
    public float SparklesFadeDistance = 1f;
    public Vector3 SparklesPosition = new Vector3(0f, 4.5f, 1.75f);
    
    private float CurrentFireIntensity => FireIntensity * FireMaxSize;
    
    private static int _fireSize = Shader.PropertyToID("FireSize");
    private static int _smokeSize = Shader.PropertyToID("SmokeSize");
    private static int _sparklesSize = Shader.PropertyToID("SparklesSize");
    private static int _fireRate = Shader.PropertyToID("FireRate");
    private static int _fireLifeTimeMin = Shader.PropertyToID("FireLifeTimeMin");
    private static int _fireLifeTimeMax = Shader.PropertyToID("FireLifeTimeMax");
    private static int _firePivotY = Shader.PropertyToID("FirePivotY");
    private static int _fireFrameRate = Shader.PropertyToID("FireFrameRate");
    private static int _fireColor = Shader.PropertyToID("FireColor");
    private static int _fireScaleY = Shader.PropertyToID("FireScaleY");
    private static int _fireScaleX = Shader.PropertyToID("FireScaleX");
    private static int _fireAngle = Shader.PropertyToID("FireAngle");
    
    private static int _smokeRate = Shader.PropertyToID("SmokeRate");
    private static int _smokeLifeTimeMin = Shader.PropertyToID("SmokeLifeTimeMin");
    private static int _smokeLifeTimeMax = Shader.PropertyToID("SmokeLifeTimeMax");
    private static int _smokeVelocity1 = Shader.PropertyToID("SmokeVelocity1");
    private static int _smokeVelocity2 = Shader.PropertyToID("SmokeVelocity2");
    private static int _smokeGravityY = Shader.PropertyToID("SmokeGravityY");
    private static int _smokeFrameRate = Shader.PropertyToID("SmokeFrameRate");
    private static int _smokeColor = Shader.PropertyToID("SmokeColor");
    //private static int _smokeSizeID = Shader.PropertyToID("SmokeSize");
    private static int _smokeScaleY = Shader.PropertyToID("SmokeScaleY");
    private static int _smokeScaleX = Shader.PropertyToID("SmokeScaleX");
    private static int _smokePosition = Shader.PropertyToID("SmokePosition");
    
    private static int _sparklesRate = Shader.PropertyToID("SparklesRate");
    private static int _sparklesLifeTimeMin = Shader.PropertyToID("SparklesLifeTimeMin");
    private static int _sparklesLifeTimeMax = Shader.PropertyToID("SparklesLifeTimeMax");
    private static int _sparklesVelocityMin = Shader.PropertyToID("SparklesVelocityMin");
    private static int _sparklesVelocityMax = Shader.PropertyToID("SparklesVelocityMax");
    private static int _sparklesScaleX = Shader.PropertyToID("SparklesScaleX");
    //private static int _sparklesSizeID = Shader.PropertyToID("SparklesSize");
    private static int _sparklesColor = Shader.PropertyToID("SparklesColor");
    private static int _sparklesFadeDistance = Shader.PropertyToID("SparklesFadeDistance");
    private static int _sparklesPosition = Shader.PropertyToID("SparklesPosition");
    
    void Update()
    {
        FireVFX.SetFloat(_fireSize, CurrentFireIntensity);
        FireVFX.SetFloat(_smokeSize, CurrentFireIntensity);
        FireVFX.SetFloat(_sparklesSize, CurrentFireIntensity * 0.5f);
        
        FireVFX.SetFloat(_fireRate, FireRate);
        FireVFX.SetFloat(_fireLifeTimeMin, FireLifeTimeMin);
        FireVFX.SetFloat(_fireLifeTimeMax, FireLifeTimeMax);
        FireVFX.SetFloat(_firePivotY, FirePivotY);
        FireVFX.SetFloat(_fireFrameRate, FireFrameRate);
        FireVFX.SetGradient(_fireColor, FireColor);
        FireVFX.SetFloat(_fireScaleY, FireScaleY);
        FireVFX.SetFloat(_fireScaleX, FireScaleX);
        FireVFX.SetVector3(_fireAngle, FireAngle);
        
        FireVFX.SetFloat(_smokeRate, SmokeRate);
        FireVFX.SetFloat(_smokeLifeTimeMin, SmokeLifeTimeMin);
        FireVFX.SetFloat(_smokeLifeTimeMax, SmokeLifeTimeMax);
        FireVFX.SetVector3(_smokeVelocity1, SmokeVelocity1);
        FireVFX.SetVector3(_smokeVelocity2, SmokeVelocity2);
        FireVFX.SetFloat(_smokeGravityY, SmokeGravityY);
        FireVFX.SetFloat(_smokeFrameRate, SmokeFrameRate);
        FireVFX.SetGradient(_smokeColor, SmokeColor);
        //FireVFX.SetFloat(_smokeSizeID, SmokeSize);
        FireVFX.SetFloat(_smokeScaleY, SmokeScaleY);
        FireVFX.SetFloat(_smokeScaleX, SmokeScaleX);
        FireVFX.SetVector3(_smokePosition, SmokePosition);
        
        FireVFX.SetFloat(_sparklesRate, SparklesRate);
        FireVFX.SetFloat(_sparklesLifeTimeMin, SparklesLifeTimeMin);
        FireVFX.SetFloat(_sparklesLifeTimeMax, SparklesLifeTimeMax);
        FireVFX.SetFloat(_sparklesVelocityMin, SparklesVelocityMin);
        FireVFX.SetFloat(_sparklesVelocityMax, SparklesVelocityMax);
        FireVFX.SetFloat(_sparklesScaleX, SparklesScaleX);
        //FireVFX.SetFloat(_sparklesSizeID, SparklesSize);
        FireVFX.SetGradient(_sparklesColor, SparklesColor);
        FireVFX.SetFloat(_sparklesFadeDistance, SparklesFadeDistance);
        FireVFX.SetVector3(_sparklesPosition, SparklesPosition);


    }
}
