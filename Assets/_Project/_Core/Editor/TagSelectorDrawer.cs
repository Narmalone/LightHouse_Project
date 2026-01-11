using LightHouse.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.CustomEditors
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == UnityEditor.SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use TagAttribute with string.");
            }
        }
    }
#endif
}
