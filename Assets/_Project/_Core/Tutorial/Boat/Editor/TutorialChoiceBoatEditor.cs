using LightHouse.Features.Boats;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
    [CustomEditor(typeof(TutorialChoiceBoat))]
    public class TutorialChoiceBoatEditor : Editor
    {
        private SerializedProperty _commonWaypointsProp;
        private SerializedProperty _directionPathsProp;
        private SerializedProperty _playerSpawnProp;

        private void OnEnable()
        {
            _commonWaypointsProp = serializedObject.FindProperty("_commonWaypoints");
            _directionPathsProp = serializedObject.FindProperty("_directionPaths");
            _playerSpawnProp = serializedObject.FindProperty("_playerSpawnTutorial");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Path Tools - Tronc commun", EditorStyles.boldLabel);
            DrawArrayTools(_commonWaypointsProp);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Path Tools - Branches", EditorStyles.boldLabel);

            for (int i = 0; i < _directionPathsProp.arraySize; i++)
            {
                SerializedProperty entryProp = _directionPathsProp.GetArrayElementAtIndex(i);
                SerializedProperty directionProp = entryProp.FindPropertyRelative("Direction");
                SerializedProperty waypointsProp = entryProp.FindPropertyRelative("Waypoints");

                EditorGUILayout.LabelField(((BoatDirection)directionProp.enumValueIndex).ToString(), EditorStyles.miniBoldLabel);
                DrawArrayTools(waypointsProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Boutons Add/Remove/Clear pour un tableau de Vector3, dans le style de
        /// VectorPathVisualizerEditor, mais via SerializedProperty (champ privé).
        /// </summary>
        private void DrawArrayTools(SerializedProperty arrayProp)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Point"))
            {
                int index = arrayProp.arraySize;
                arrayProp.InsertArrayElementAtIndex(index);

                Vector3 newPoint = index > 0
                    ? arrayProp.GetArrayElementAtIndex(index - 1).vector3Value + Vector3.forward * 5f
                    : ((TutorialChoiceBoat)target).transform.position;

                arrayProp.GetArrayElementAtIndex(index).vector3Value = newPoint;
            }

            if (GUILayout.Button("Remove Last") && arrayProp.arraySize > 0)
            {
                arrayProp.DeleteArrayElementAtIndex(arrayProp.arraySize - 1);
            }

            if (GUILayout.Button("Clear") && arrayProp.arraySize > 0)
            {
                if (EditorUtility.DisplayDialog("Clear Points", "Are you sure?", "Yes", "Cancel"))
                {
                    arrayProp.ClearArray();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();

            DrawWaypointHandles(_commonWaypointsProp, Color.cyan, "Common");

            for (int i = 0; i < _directionPathsProp.arraySize; i++)
            {
                SerializedProperty entryProp = _directionPathsProp.GetArrayElementAtIndex(i);
                SerializedProperty directionProp = entryProp.FindPropertyRelative("Direction");
                SerializedProperty waypointsProp = entryProp.FindPropertyRelative("Waypoints");

                BoatDirection direction = (BoatDirection)directionProp.enumValueIndex;
                DrawWaypointHandles(waypointsProp, GetDirectionColor(direction), direction.ToString());
            }

            DrawPlayerSpawnHandle();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Handle de déplacement pour chaque waypoint d'un tableau Vector3.
        /// Pas de conversion local/monde : les waypoints sont déjà en coordonnées
        /// monde, exactement comme le runtime les consomme dans TutorialChoiceBoat.
        /// </summary>
        private void DrawWaypointHandles(SerializedProperty arrayProp, Color color, string label)
        {
            if (arrayProp == null || arrayProp.arraySize == 0) return;

            Handles.color = color;

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                SerializedProperty pointProp = arrayProp.GetArrayElementAtIndex(i);
                Vector3 pos = pointProp.vector3Value;

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move Waypoint");
                    pointProp.vector3Value = newPos;
                    pos = newPos;
                }

                Handles.Label(pos + Vector3.up * 0.5f, $"{label} {i}");

                if (i < arrayProp.arraySize - 1)
                {
                    Vector3 next = arrayProp.GetArrayElementAtIndex(i + 1).vector3Value;
                    Handles.DrawLine(pos, next);
                }
            }
        }

        private void DrawPlayerSpawnHandle()
        {
            if (_playerSpawnProp == null || _playerSpawnProp.objectReferenceValue == null) return;

            Transform spawn = (Transform)_playerSpawnProp.objectReferenceValue;

            Handles.color = Color.white;
            Handles.Label(spawn.position + Vector3.up * 0.6f, "Player Spawn");
            // Le Transform garde ses propres handles de déplacement/rotation natifs
            // quand il est sélectionné ; pas besoin d'un PositionHandle en plus ici.
        }

        private static Color GetDirectionColor(BoatDirection direction)
        {
            switch (direction)
            {
                case BoatDirection.Left: return Color.red;
                case BoatDirection.Midle: return Color.blue;
                case BoatDirection.Right: return Color.green;
                default: return Color.white;
            }
        }
    }
}