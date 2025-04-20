using UnityEditor;
using UnityEngine;

public class GizmoToggleWindow : EditorWindow
{
    [MenuItem("Tools/Gizmo Manager")]
    public static void ShowWindow()
    {
        GetWindow<GizmoToggleWindow>("Gizmo Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("Gizmo Display", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        GizmoDisplaySettings.ShowInventory = EditorGUILayout.Toggle("Show Inventory Items", GizmoDisplaySettings.ShowInventory);
        GizmoDisplaySettings.ShowInteractables = EditorGUILayout.Toggle("Show Interactables", GizmoDisplaySettings.ShowInteractables);

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll(); // Force la scène à se redessiner
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Refresh SceneView"))
        {
            SceneView.RepaintAll();
        }
    }
}
