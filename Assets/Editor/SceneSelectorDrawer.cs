

namespace LightHouse.CustomEditors
{

#if UNITY_EDITOR
    using LightHouse.CustomAttributes;
    using UnityEngine;
    using UnityEditor;
    using System.Linq;

    [CustomPropertyDrawer(typeof(SceneSelectorAttribute))]
    public class SceneSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use SceneSelector with string.");
                return;
            }

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene =>
                {
                    var path = scene.path;
                    var lastSlash = path.LastIndexOf('/');
                    var name = path.Substring(lastSlash + 1).Replace(".unity", "");
                    return name;
                })
                .ToArray();

            int selectedIndex = Mathf.Max(0, System.Array.IndexOf(scenes, property.stringValue));
            int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, scenes);

            if (newIndex >= 0 && newIndex < scenes.Length)
            {
                property.stringValue = scenes[newIndex];
            }
        }
    }
#endif

}
