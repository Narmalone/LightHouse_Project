using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class SmokeController : MonoBehaviour
{
    [Header("VFX")]
    public VisualEffect SmokeVFX;
    
    [Header("Smoke Settings")]
    public float SmokeRate = 1000f;
    public float SmokeLifeTimeMin = 3f;
    public float SmokeLifeTimeMax = 5f;
    public Vector3 SmokeVelocity1 = new Vector3(-0.2f, -1f, -0.333f);
    public Vector3 SmokeVelocity2 = new Vector3(0.2f, 1f, 0.333f);
    public float SmokeGravityY = 0.1f;
    public float SmokeFrameRate = 2f;
    public float SmokeSize = 10f;
    public float SmokeScaleX = 1f;
    public float SmokeScaleY = 1f;
    public Gradient SmokeColor = new Gradient();
    public Vector3 SmokePosition = new Vector3(0f, 7.5f, 0f);
    public Vector3 SmokeShapeScale = new Vector3(5f, 1f, 5f);
    
    private static int _smokeRate = Shader.PropertyToID("SmokeRate");
    private static int _smokeLifeTimeMin = Shader.PropertyToID("SmokeLifeTimeMin");
    private static int _smokeLifeTimeMax = Shader.PropertyToID("SmokeLifeTimeMax");
    private static int _smokeVelocity1 = Shader.PropertyToID("SmokeVelocity1");
    private static int _smokeVelocity2 = Shader.PropertyToID("SmokeVelocity2");
    private static int _smokeGravityY = Shader.PropertyToID("SmokeGravityY");
    private static int _smokeFrameRate = Shader.PropertyToID("SmokeFrameRate");
    private static int _smokeColor = Shader.PropertyToID("SmokeColor");
    private static int _smokeSizeID = Shader.PropertyToID("SmokeSize");
    private static int _smokeScaleY = Shader.PropertyToID("SmokeScaleY");
    private static int _smokeScaleX = Shader.PropertyToID("SmokeScaleX");
    private static int _smokePosition = Shader.PropertyToID("SmokePosition");
    private static int _smokeShapeScale = Shader.PropertyToID("SmokeShapeScale");
    
    void Update()
    {
        SmokeVFX.SetFloat(_smokeScaleY, SmokeScaleY);
        SmokeVFX.SetFloat(_smokeScaleX, SmokeScaleX);
        SmokeVFX.SetFloat(_smokeRate, SmokeRate);
        SmokeVFX.SetFloat(_smokeLifeTimeMin, SmokeLifeTimeMin);
        SmokeVFX.SetFloat(_smokeLifeTimeMax, SmokeLifeTimeMax);
        SmokeVFX.SetVector3(_smokeVelocity1, SmokeVelocity1);
        SmokeVFX.SetVector3(_smokeVelocity2, SmokeVelocity2);
        SmokeVFX.SetFloat(_smokeGravityY, SmokeGravityY);
        SmokeVFX.SetFloat(_smokeFrameRate, SmokeFrameRate);
        SmokeVFX.SetGradient(_smokeColor, SmokeColor);
        SmokeVFX.SetFloat(_smokeSizeID, SmokeSize);
        SmokeVFX.SetVector3(_smokePosition, SmokePosition);
        SmokeVFX.SetVector3(_smokeShapeScale, SmokeShapeScale);
    }
}
