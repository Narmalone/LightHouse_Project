using LightHouse.Features.Computer.Calendar.Clock;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
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
}
