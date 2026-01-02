#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using LightHouse.CustomAttributes;

namespace LightHouse.CustomEditors
{
    [CustomPropertyDrawer(typeof(AddressableLabelSelectorAttribute))]
    public class AddressableLabelSelectorDrawer : PropertyDrawer
    {
        private string[] _labels;

        private void LoadLabels()
        {
            if (_labels != null) return;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                _labels = new[] { "<no Addressable settings found>" };
                return;
            }

            // ✅ On récupère tous les labels définis dans les settings
            var labelSet = settings.GetLabels();
            _labels = labelSet.Count > 0 ? labelSet.ToArray() : new[] { "<no labels defined>" };
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use AddressableLabelSelector with string.");
                return;
            }

            LoadLabels();

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(_labels, property.stringValue));
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, _labels);

            property.stringValue = _labels[selectedIndex];
        }
    }

}
#endif
