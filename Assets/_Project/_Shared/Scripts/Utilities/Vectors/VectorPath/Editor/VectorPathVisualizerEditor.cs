using LightHouse.Features.Boats;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
    [CustomEditor(typeof(VectorPathVisualiser))]
    public class VectorPathVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            VectorPathVisualiser visualizer = (VectorPathVisualiser)target;
            VectorPath path = visualizer.path;
            DrawDefaultInspector();

            if (path == null)
            {
                EditorGUILayout.HelpBox("No VectorPath assigned.", MessageType.Warning);
                return;
            }

            // --- Configurable generation settings ---
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Generation Settings (Override)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            path.radius = EditorGUILayout.FloatField("Radius", path.radius);
            path.arcAngle = EditorGUILayout.Slider("Arc Angle", path.arcAngle, 0f, 360f);
            path.startingAngle = EditorGUILayout.FloatField("Starting Angle", path.startingAngle);
            path.numberOfPoints = EditorGUILayout.IntSlider("Number of Points", path.numberOfPoints, 2, 10);
            path.useBezier = EditorGUILayout.Toggle("Use Bezier", path.useBezier);
            path.pathRandomness = EditorGUILayout.FloatField("Randomness", path.pathRandomness);
            path.minEntryExitDistance = EditorGUILayout.FloatField("Min Entry/Exit Distance", path.minEntryExitDistance);

            path.obstacleMask = EditorGUILayout.LayerField("Obstacle Mask", path.obstacleMask);
            path.pointCheckRadius = EditorGUILayout.FloatField("Point Check Radius", path.pointCheckRadius);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(path);
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Path Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(path, "Add Point");
                var list = new System.Collections.Generic.List<Vector3>(path.Paths ?? new Vector3[0]);
                list.Add(new Vector3(3f, 0f, 0f));
                path.Paths = list.ToArray();
                EditorUtility.SetDirty(path);
            }

            if (GUILayout.Button("Remove Last Point"))
            {
                Undo.RecordObject(path, "Remove Last Point");
                var list = new System.Collections.Generic.List<Vector3>(path.Paths ?? new Vector3[0]);
                if (list.Count > 0)
                {
                    list.RemoveAt(list.Count - 1);
                    path.Paths = list.ToArray();
                    EditorUtility.SetDirty(path);
                }
            }

            if (GUILayout.Button("Clear All Points"))
            {
                if (EditorUtility.DisplayDialog("Clear All Points", "Are you sure?", "Yes", "Cancel"))
                {
                    Undo.RecordObject(path, "Clear Points");
                    path.Paths = new Vector3[0];
                    EditorUtility.SetDirty(path);
                }
            }

            if (GUILayout.Button("Generate Path"))
            {
                Undo.RecordObject(path, "Generate Path");
                path.GenerateNewPath(visualizer.transform);
                EditorUtility.SetDirty(path);
            }

            // Keep base inspector at the end
            GUILayout.Space(15);
        }

        private void OnSceneGUI()
        {
            VectorPathVisualiser visualizer = (VectorPathVisualiser)target;
            VectorPath path = visualizer.path;

            if (path == null) return;

            Transform tf = visualizer.transform;

            // === Handle centerPoint ===
            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = Handles.PositionHandle(path.centerPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Move Center Point");
                path.centerPoint = newCenter;
                EditorUtility.SetDirty(path);
            }

            // === Points & Lignes ===
            if (path.Paths != null && path.Paths.Length > 0)
            {
                Handles.color = Color.green;
                for (int i = 0; i < path.Paths.Length; i++)
                {
                    Vector3 worldPos = tf.TransformPoint(path.Paths[i]);

                    EditorGUI.BeginChangeCheck();
                    Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(path, "Move Path Point");
                        path.Paths[i] = tf.InverseTransformPoint(newWorldPos);
                        EditorUtility.SetDirty(path);
                    }

                    Handles.Label(worldPos + Vector3.up * 0.5f, $"Point {i}");
                }

                Handles.color = Color.cyan;
                for (int i = 0; i < path.Paths.Length - 1; i++)
                {
                    Vector3 p1 = tf.TransformPoint(path.Paths[i]);
                    Vector3 p2 = tf.TransformPoint(path.Paths[i + 1]);
                    Handles.DrawLine(p1, p2);
                }
            }

            // === Arc de génération ===
            if (path.radius > 0f)
            {
                Vector3 center = path.centerPoint;

                Handles.color = new Color(0f, 1f, 1f, 0.3f);
                Handles.DrawWireDisc(center, Vector3.up, path.radius);

                Handles.color = Color.yellow;
                float halfArc = path.arcAngle * 0.5f;
                int segments = 64;
                float angleStep = path.arcAngle / segments;
                Vector3 prev = center + Quaternion.Euler(0, path.startingAngle - halfArc, 0) * Vector3.forward * path.radius;

                for (int i = 1; i <= segments; i++)
                {
                    float angle = path.startingAngle - halfArc + i * angleStep;
                    Vector3 next = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * path.radius;
                    Handles.DrawLine(prev, next);
                    prev = next;
                }

                Handles.color = Color.white;
                Handles.DrawLine(center, center + Quaternion.Euler(0, path.startingAngle - halfArc, 0) * Vector3.forward * path.radius);
                Handles.DrawLine(center, center + Quaternion.Euler(0, path.startingAngle + halfArc, 0) * Vector3.forward * path.radius);

                Handles.color = Color.green;
                Handles.SphereHandleCap(0, center, Quaternion.identity, 1f, EventType.Repaint);
                Handles.Label(center + Vector3.up, "Center Point");
            }

            // === Entry & Exit ===
            if (path.EntryPoint != Vector3.zero)
            {
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, path.EntryPoint, Quaternion.identity, 1.2f, EventType.Repaint);
                Handles.Label(path.EntryPoint + Vector3.up, "Entry");
            }

            if (path.ExitPoint != Vector3.zero)
            {
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, path.ExitPoint, Quaternion.identity, 1.2f, EventType.Repaint);
                Handles.Label(path.ExitPoint + Vector3.up, "Exit");
            }
        }
    }
}
