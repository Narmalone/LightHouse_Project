using LightHouse.Core.Save;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)] // true = s'applique aux sous-classes
public class BindableEntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Vérifie si le composant inspecté implémente IBind<> (n'importe quel TData)
        bool isBindable = target.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBind<>));

        if (!isBindable) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Save System", EditorStyles.boldLabel);

        // Récupère la propriété Id via reflection
        var idProperty = serializedObject.FindProperty("<Id>k__BackingField");

        if (idProperty != null)
        {
            // Affiche le GUID actuel en lecture seule
            var part1 = idProperty.FindPropertyRelative("Part1");
            var part2 = idProperty.FindPropertyRelative("Part2");
            var part3 = idProperty.FindPropertyRelative("Part3");
            var part4 = idProperty.FindPropertyRelative("Part4");

            if (part1 != null)
            {
                string currentGuid = $"{part1.longValue:X8}{part2.longValue:X8}" +
                                     $"{part3.longValue:X8}{part4.longValue:X8}";

                bool isEmpty = currentGuid == "0000000000000000000000000000000000000000";

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("GUID", isEmpty ? "— non généré —" : currentGuid);
                EditorGUI.EndDisabledGroup();

                // Warning si GUID vide
                if (isEmpty)
                {
                    EditorGUILayout.HelpBox(
                        "Cet objet n'a pas de GUID. Génère-en un avant de jouer.",
                        MessageType.Warning);
                }
            }
        }

        // Bouton de génération
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🎲 Générer un nouveau GUID"))
        {
            var component = (MonoBehaviour)target;

            // Accède à la propriété Id via reflection
            var idProp = target.GetType().GetProperty("Id");
            if (idProp != null)
            {
                Undo.RecordObject(target, "Generate GUID");
                idProp.SetValue(target, SerializableGuid.NewGuid());
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"[SaveSystem] GUID généré pour {component.gameObject.name}");
            }
        }
        GUI.backgroundColor = Color.white;
    }
}