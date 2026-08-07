using AYellowpaper.SerializedCollections;
using LightHouse.Features.Boats;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
    [CustomEditor(typeof(TutorialChoiceBoat))]
    public class TutorialChoiceBoatEditor : Editor
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private FieldInfo _commonWaypointsField;
        private FieldInfo _directionPathsField;
        private FieldInfo _finalWaypointsField;
        private FieldInfo _playerSpawnField;

        private TutorialChoiceBoat Boat => (TutorialChoiceBoat)target;

        private void OnEnable()
        {
            Type type = typeof(TutorialChoiceBoat);

            _commonWaypointsField = type.GetField("_commonWaypoints", PrivateInstance);
            _directionPathsField = type.GetField("_directionPaths", PrivateInstance);
            _finalWaypointsField = type.GetField("_finalWaypoints", PrivateInstance);
            _playerSpawnField = type.GetField("_playerSpawnTutorial", PrivateInstance);

            if (_commonWaypointsField == null || _directionPathsField == null ||
                _finalWaypointsField == null || _playerSpawnField == null)
            {
                Debug.LogError("TutorialChoiceBoatEditor: un ou plusieurs champs attendus " +
                    "n'ont pas été trouvés par réflexion. Le nom des champs a-t-il changé dans TutorialChoiceBoat ?");
            }
        }

        #region Accès direct aux champs (réflexion)

        // On passe par les champs C# réels plutôt que par SerializedProperty pour
        // _directionPaths : c'est un SerializedDictionary imbriqué (package tiers
        // AYellowpaper), dont la représentation sérialisée interne peut varier selon
        // la version installée. En lisant/écrivant directement l'objet vivant, on
        // n'a pas besoin de connaître cette structure interne.

        private Vector3[] GetCommonWaypoints() => (Vector3[])_commonWaypointsField.GetValue(Boat);
        private void SetCommonWaypoints(Vector3[] value) => _commonWaypointsField.SetValue(Boat, value);

        private Vector3[] GetFinalWaypoints() => (Vector3[])_finalWaypointsField.GetValue(Boat);
        private void SetFinalWaypoints(Vector3[] value) => _finalWaypointsField.SetValue(Boat, value);

        private SerializedDictionary<int, SerializedDictionary<BoatDirection, Vector3[]>> GetDirectionPaths() =>
            (SerializedDictionary<int, SerializedDictionary<BoatDirection, Vector3[]>>)_directionPathsField.GetValue(Boat);

        private Transform GetPlayerSpawn() => (Transform)_playerSpawnField.GetValue(Boat);

        #endregion

        public override void OnInspectorGUI()
        {
            // AYellowpaper fournit son propre drawer pour le SerializedDictionary imbriqué
            // (ajout/suppression d'étapes et de directions) : DrawDefaultInspector() suffit
            // pour ça, pas besoin de boutons custom sur _directionPaths.
            DrawDefaultInspector();

            if (_commonWaypointsField == null) return;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Path Tools", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Tronc commun", EditorStyles.miniBoldLabel);
            DrawArrayTools(GetCommonWaypoints(), SetCommonWaypoints);

            GUILayout.Space(6);
            EditorGUILayout.LabelField("Tronc final", EditorStyles.miniBoldLabel);
            DrawArrayTools(GetFinalWaypoints(), SetFinalWaypoints);
        }

        /// <summary>
        /// Boutons Add/Remove/Clear pour un tableau de Vector3, en manipulant directement
        /// l'objet (les tableaux sont immuables en taille : on doit réassigner le champ).
        /// </summary>
        private void DrawArrayTools(Vector3[] current, Action<Vector3[]> apply)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(target, "Add Point");

                var list = new List<Vector3>(current ?? Array.Empty<Vector3>());
                Vector3 newPoint = list.Count > 0
                    ? list[list.Count - 1] + Vector3.forward * 5f
                    : Boat.transform.position;
                list.Add(newPoint);

                apply(list.ToArray());
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Remove Last") && current != null && current.Length > 0)
            {
                Undo.RecordObject(target, "Remove Last Point");

                var list = new List<Vector3>(current);
                list.RemoveAt(list.Count - 1);

                apply(list.ToArray());
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Clear") && current != null && current.Length > 0)
            {
                if (EditorUtility.DisplayDialog("Clear Points", "Are you sure?", "Yes", "Cancel"))
                {
                    Undo.RecordObject(target, "Clear Points");
                    apply(Array.Empty<Vector3>());
                    EditorUtility.SetDirty(target);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            if (_commonWaypointsField == null) return;

            DrawWaypointHandles(GetCommonWaypoints(), Color.cyan, "Common");

            var directionPaths = GetDirectionPaths();
            if (directionPaths != null)
            {
                foreach (var stepEntry in directionPaths)
                {
                    SerializedDictionary<BoatDirection, Vector3[]> stepOptions = stepEntry.Value;
                    if (stepOptions == null) continue;

                    foreach (var branchEntry in stepOptions)
                    {
                        string label = $"Step{stepEntry.Key}-{branchEntry.Key}";
                        DrawWaypointHandles(branchEntry.Value, GetDirectionColor(branchEntry.Key), label);
                    }
                }
            }

            DrawWaypointHandles(GetFinalWaypoints(), new Color(1f, 0.55f, 0f), "Final");

            DrawPlayerSpawnHandle();
        }

        /// <summary>
        /// Handle de déplacement pour chaque waypoint d'un tableau Vector3, en mutant
        /// directement les éléments du tableau (référence partagée avec le champ du
        /// composant : pas besoin de réassigner, seule la taille du tableau nécessite ça).
        /// Pas de conversion local/monde : les waypoints sont déjà en coordonnées monde,
        /// exactement comme le runtime les consomme dans TutorialChoiceBoat.
        /// </summary>
        private void DrawWaypointHandles(Vector3[] waypoints, Color color, string label)
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Handles.color = color;

            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3 pos = waypoints[i];

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move Waypoint");
                    waypoints[i] = newPos;
                    pos = newPos;
                    EditorUtility.SetDirty(target);
                }

                Handles.Label(pos + Vector3.up * 0.5f, $"{label} {i}");

                if (i < waypoints.Length - 1)
                    Handles.DrawLine(pos, waypoints[i + 1]);
            }
        }

        private void DrawPlayerSpawnHandle()
        {
            Transform spawn = GetPlayerSpawn();
            if (spawn == null) return;

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