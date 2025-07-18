using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class TornadoHandler : MonoBehaviour
{
    [Header("FogGameObjects")]
    public GameObject GO_TornadoParticles;
    public GameObject GO_BaseTornado;
    public GameObject GO_SubTornado;
    public GameObject GO_BottomFog;
    public GameObject GO_TopFog;

    
    [Header("Materials")]
    public Material M_BaseTornadoMaterial;
    public Material M_SubTornadoMaterial;
    public Material M_BottomTornadoFogMaterial;
    public Material M_TopTornadoFogMaterial;
    public Material M_TornadoParticles;

    [Header("RenderDistance")]
    public float RenderDistance = 10000;

    [Header("Shape")]
    public float Height;

    [Header("Rotation Settings")]
    public float RotationSpeed = 1f;


    [Header("Fog Distance")]
    public float FogDistance = 15f;
    [Range(0.01f, 2f)]
    public float ParticlesFogDistanceDilution = 2f;


    [Header("Colors")]
    public Color TornadoOuterColor;
    public Color TornadoInnerColor;
    public Color TornadoParticlesColor;
    [Range(0,1)]
    public float Opacity;

    private LocalVolumetricFog LVF_TornadoParticles;
    private LocalVolumetricFog LVF_BaseTornado;
    private LocalVolumetricFog LVF_SubTornado;
    private LocalVolumetricFog LVF_BottomFog;
    private LocalVolumetricFog LVF_TopFog;

    private static int _tornadoOuterColor = Shader.PropertyToID("_OuterColor");
    private static int _tornadoInnerColor = Shader.PropertyToID("_InnerColor");
    private static int _tornadoOpacity = Shader.PropertyToID("_Opacity");
    private static int _tornadoParticlesColor = Shader.PropertyToID("_TornadoParticlesColor");
    private static int _tornadoRotationSpeed = Shader.PropertyToID("_RotationSpeed");
    private static int _tornadoRadius = Shader.PropertyToID("_Radius");
    private static int _tornadoRadiusOfGrowth = Shader.PropertyToID("_RadiusOfGrowth");
    private static int _tornadoFogDistance = Shader.PropertyToID("_FogVolumeFogDistanceProperty");

    void Start() 
    {
        LVF_TornadoParticles = GO_TornadoParticles.GetComponent<LocalVolumetricFog>();
        LVF_BaseTornado = GO_BaseTornado.GetComponent<LocalVolumetricFog>();
        LVF_SubTornado = GO_SubTornado.GetComponent<LocalVolumetricFog>();
        LVF_BottomFog = GO_BottomFog.GetComponent<LocalVolumetricFog>();
        LVF_TopFog = GO_TopFog.GetComponent<LocalVolumetricFog>();
    }

    // Update is called once per frame
    void Update()
    {
        SetMaterials();
        //DebugGetFogMaterialPropertyNames();
        LVF_TornadoParticles = GO_TornadoParticles.GetComponent<LocalVolumetricFog>();
        LVF_BaseTornado = GO_BaseTornado.GetComponent<LocalVolumetricFog>();
        LVF_SubTornado = GO_SubTornado.GetComponent<LocalVolumetricFog>();
        LVF_BottomFog = GO_BottomFog.GetComponent<LocalVolumetricFog>();
        LVF_TopFog = GO_TopFog.GetComponent<LocalVolumetricFog>();


        //Set positions
        GO_TopFog.transform.localPosition = new Vector3(GO_TopFog.transform.localPosition.x,Height, GO_TopFog.transform.localPosition.z);
        GO_BaseTornado.transform.localPosition = new Vector3(GO_TopFog.transform.localPosition.x, Height / 2, GO_TopFog.transform.localPosition.z);
        GO_SubTornado.transform.localPosition = new Vector3(GO_TopFog.transform.localPosition.x, Height / 2, GO_TopFog.transform.localPosition.z);
        GO_TornadoParticles.transform.localPosition = new Vector3(GO_TopFog.transform.localPosition.x, Height / 2, GO_TopFog.transform.localPosition.z);

        //Update Local Volumetric Fogs

        float updatedHeightForLVF = Height + Height / 5;
        LVF_BaseTornado.parameters.size = new Vector3(updatedHeightForLVF / 2, updatedHeightForLVF, updatedHeightForLVF/2);
        LVF_SubTornado.parameters.size = new Vector3(updatedHeightForLVF / 2, updatedHeightForLVF, updatedHeightForLVF / 2);
        LVF_BottomFog.parameters.size = new Vector3(updatedHeightForLVF, Height/5, updatedHeightForLVF);
        LVF_TopFog.parameters.size = new Vector3(updatedHeightForLVF, Height / 5, updatedHeightForLVF);
        LVF_TornadoParticles.parameters.size = new Vector3(Height * 3 / 4, Height, Height * 3 / 4);

        LVF_BaseTornado.parameters.distanceFadeStart = RenderDistance;
        LVF_SubTornado.parameters.distanceFadeStart = RenderDistance;
        LVF_BottomFog.parameters.distanceFadeStart = RenderDistance;
        LVF_TopFog.parameters.distanceFadeStart = RenderDistance;
        LVF_TornadoParticles.parameters.distanceFadeStart = RenderDistance;

        LVF_BaseTornado.parameters.distanceFadeEnd = RenderDistance;
        LVF_SubTornado.parameters.distanceFadeEnd = RenderDistance;
        LVF_BottomFog.parameters.distanceFadeEnd = RenderDistance;
        LVF_TopFog.parameters.distanceFadeEnd = RenderDistance;
        LVF_TornadoParticles.parameters.distanceFadeEnd = RenderDistance;

    }
    private void SetMaterials()
    {
        //Outer Color
        M_BaseTornadoMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);
        M_SubTornadoMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);
        M_TopTornadoFogMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);
        M_BottomTornadoFogMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);

        //Inner Color
        M_BaseTornadoMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);
        M_SubTornadoMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);
        M_TopTornadoFogMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);
        M_BottomTornadoFogMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);

        //Particles Color
        M_TornadoParticles.SetColor(_tornadoParticlesColor, TornadoParticlesColor);

        //Rotation Speed
        M_BaseTornadoMaterial.SetFloat(_tornadoRotationSpeed, RotationSpeed / 2);
        M_SubTornadoMaterial.SetFloat(_tornadoRotationSpeed, RotationSpeed / 2);
        M_TopTornadoFogMaterial.SetFloat(_tornadoRotationSpeed, -RotationSpeed * 0.01f);
        M_BottomTornadoFogMaterial.SetFloat(_tornadoRotationSpeed, RotationSpeed * 0.01f);
        M_TornadoParticles.SetFloat(_tornadoRotationSpeed, -RotationSpeed * 10);

        //Fog Distance
        M_BaseTornadoMaterial.SetFloat(_tornadoFogDistance, FogDistance / 3);
        M_SubTornadoMaterial.SetFloat(_tornadoFogDistance, FogDistance);
        M_TopTornadoFogMaterial.SetFloat(_tornadoFogDistance, FogDistance);
        M_BottomTornadoFogMaterial.SetFloat(_tornadoFogDistance, FogDistance);
        M_TornadoParticles.SetFloat(_tornadoFogDistance, FogDistance / (1 / ParticlesFogDistanceDilution));

        //Opacity

        M_BaseTornadoMaterial.SetFloat(_tornadoOpacity, Opacity);
        M_SubTornadoMaterial.SetFloat(_tornadoOpacity, Opacity);
        M_TopTornadoFogMaterial.SetFloat(_tornadoOpacity, Opacity);
        M_BottomTornadoFogMaterial.SetFloat(_tornadoOpacity, Opacity);
        M_TornadoParticles.SetFloat(_tornadoOpacity, Opacity);

        //Radius
        M_TopTornadoFogMaterial.SetFloat(_tornadoRadius, Height / 2);
        M_TopTornadoFogMaterial.SetFloat(_tornadoRadiusOfGrowth, Height / 2);

        M_BottomTornadoFogMaterial.SetFloat(_tornadoRadius, Height / 2);
        M_BottomTornadoFogMaterial.SetFloat(_tornadoRadiusOfGrowth, Height / 2);
    }
    private void DebugGetFogMaterialPropertyNames()
    {
        Shader shader = M_BottomTornadoFogMaterial.shader;
        int propertyCount = shader.GetPropertyCount();

        for (int i = 0; i < propertyCount; i++)
        {
            string propertyName = shader.GetPropertyName(i);
            Debug.Log("Property #" + i + ": " + propertyName + " (" + shader.GetPropertyType(i) + ")");
        }
    }
}
