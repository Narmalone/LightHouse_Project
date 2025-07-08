using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClockTextPlacer))]
public class ClockTextPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClockTextPlacer placer = (ClockTextPlacer)target;
        if (GUILayout.Button("Générer les chiffres"))
        {
            placer.PlaceTextsInCircle();
        }
    }
}
