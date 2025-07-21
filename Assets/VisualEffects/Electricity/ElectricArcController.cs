using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using UnityEngine.VFX.Utility;
using UnityEngine.UIElements;
[ExecuteInEditMode]
public class ElectricArcController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    public VisualEffectAsset EffectAsset;
    public int NumberOfSubArcs;
    public float Size;
    public GameObject Pos1;
    public GameObject Pos2;
    public GameObject Pos3;
    public GameObject Pos4;


    private static int _pos1 = Shader.PropertyToID("Pos1");
    private static int _pos2 = Shader.PropertyToID("Pos2");
    private static int _pos3 = Shader.PropertyToID("Pos3");
    private static int _pos4 = Shader.PropertyToID("Pos4");
    private static int _noiseFrequency = Shader.PropertyToID("NoiseFrequency");
    private static int _noiseRange = Shader.PropertyToID("NoiseRange");
    private static int _size = Shader.PropertyToID("Size");
    void Start()
    {
        for (int i = 0; i < NumberOfSubArcs; i++)
        {
            GameObject thisNewGameObject = new GameObject("Arc" + i);
            thisNewGameObject.transform.parent = transform;
            VisualEffect thisVisualEffectAsset = thisNewGameObject.AddComponent<VisualEffect>();

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("GenerateArcs")]
    public void GenerateArcs()
    {

        foreach (Transform child in transform.Cast<Transform>().ToArray())
        {
            if (child.name.StartsWith("Arc"))
            DestroyImmediate(child.gameObject);
        }

        for (int i = 1; i < NumberOfSubArcs+1; i++)
        {
            GameObject thisNewGameObject = new GameObject("Arc" + i);
            thisNewGameObject.transform.parent = transform;
            VisualEffect thisVisualEffectAsset = thisNewGameObject.AddComponent<VisualEffect>();
            VFXPropertyBinder thisPropertyBinder = thisNewGameObject.AddComponent<VFXPropertyBinder>();




            EditableVFXPositionBinder binder1 = thisPropertyBinder.AddPropertyBinder<EditableVFXPositionBinder>();
            binder1.Target = Pos1.transform;
            binder1.Property = "Pos1";
            EditableVFXPositionBinder binder2 = thisPropertyBinder.AddPropertyBinder<EditableVFXPositionBinder>();
            binder2.Target = Pos2.transform;
            binder2.Property = "Pos2";
            EditableVFXPositionBinder binder3 = thisPropertyBinder.AddPropertyBinder<EditableVFXPositionBinder>();
            binder3.Target = Pos3.transform;
            binder3.Property = "Pos3";
            EditableVFXPositionBinder binder4 = thisPropertyBinder.AddPropertyBinder<EditableVFXPositionBinder>();
            binder4.Target = Pos4.transform;
            binder4.Property = "Pos4";



            thisVisualEffectAsset.visualEffectAsset = EffectAsset;
            thisVisualEffectAsset.SetVector3(_pos1, Pos1.transform.position);
            thisVisualEffectAsset.SetVector3(_pos2, Pos2.transform.position);
            thisVisualEffectAsset.SetVector3(_pos3, Pos3.transform.position);
            thisVisualEffectAsset.SetVector3(_pos4, Pos4.transform.position);
            thisVisualEffectAsset.SetFloat(_noiseRange, Random.Range(0,1f));
            thisVisualEffectAsset.SetFloat(_noiseFrequency, Random.Range(0,1f));
            thisVisualEffectAsset.SetFloat(_size, Size + Random.Range(0.05f,0.1f)*Size );
        }
    }

}
