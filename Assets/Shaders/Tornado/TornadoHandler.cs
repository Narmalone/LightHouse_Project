using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class TornadoHandler : MonoBehaviour
{
    [Header("Materials")]
    public Material BaseTornadoMaterial;
    public Material SubTornadoMaterial;
    public Material BottomTornadoFogMaterial;
    public Material TopTornadoFogMaterial;
    public Material TornadoParticles;

    private Material[] TornadoMaterials;

    [Header("RotationSettings")]

    public float RotationSpeed = 1f;

    public float FogDistance = 15f;

    public Color TornadoOuterColor;
    public Color TornadoInnerColor;
    public Color TornadoParticlesColor;

    
    private static int _tornadoOuterColor = Shader.PropertyToID("_OuterColor");
    private static int _tornadoInnerColor = Shader.PropertyToID("_InnerColor");
    private static int _tornadoParticlesColor = Shader.PropertyToID("_TornadoParticlesColor");
    private static int _tornadoRotationSpeed = Shader.PropertyToID("_RotationSpeed");
    private static int _tornadoFogDistance = Shader.PropertyToID("_FogVolumeFogDistanceProperty");

    void Start() 
    {
    }

    // Update is called once per frame
    void Update()
    {


        BaseTornadoMaterial.SetColor(_tornadoOuterColor,TornadoOuterColor);
        SubTornadoMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);
        TopTornadoFogMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);
        BottomTornadoFogMaterial.SetColor(_tornadoOuterColor, TornadoOuterColor);


        BaseTornadoMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);
        SubTornadoMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);
        TopTornadoFogMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);
        BottomTornadoFogMaterial.SetColor(_tornadoInnerColor, TornadoInnerColor);

        TornadoParticles.SetColor(_tornadoParticlesColor, TornadoParticlesColor);

        BaseTornadoMaterial.SetFloat(_tornadoRotationSpeed, RotationSpeed/2);
        SubTornadoMaterial.SetFloat(_tornadoRotationSpeed, RotationSpeed/2);
        TopTornadoFogMaterial.SetFloat(_tornadoRotationSpeed, -RotationSpeed*0.01f);
        BottomTornadoFogMaterial.SetFloat(_tornadoRotationSpeed, RotationSpeed*0.01f);
        TornadoParticles.SetFloat(_tornadoRotationSpeed, -RotationSpeed * 10);

        BaseTornadoMaterial.SetFloat(_tornadoFogDistance, FogDistance/3);
        SubTornadoMaterial.SetFloat(_tornadoFogDistance, FogDistance);
        TopTornadoFogMaterial.SetFloat(_tornadoFogDistance, FogDistance);
        BottomTornadoFogMaterial.SetFloat(_tornadoFogDistance, FogDistance);
        TornadoParticles.SetFloat(_tornadoFogDistance, FogDistance/5f);

        // Search for fog volume property names
        //Shader shader = BottomTornadoFogMaterial.shader;
        //int propertyCount = shader.GetPropertyCount();

        //for (int i = 0; i < propertyCount; i++)
        //{
        //    string propertyName = shader.GetPropertyName(i);
        //    Debug.Log("Property #" + i + ": " + propertyName + " (" + shader.GetPropertyType(i) + ")");
        //}

    }
}
