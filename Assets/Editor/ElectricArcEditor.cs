using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ElectricArcController))]
public class ElectricArcEditor : Editor
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("GenerateArc"))
        {
            ElectricArcController controller = (ElectricArcController)target;
            controller.GenerateArcs();
        }
    }
}
