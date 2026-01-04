using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    public static class SerializedObjectAutoUI
    {
        public static void BuildExposedFieldsUI(VisualElement root, SerializedObject so)
        {
            root.Clear();

            var target = so.targetObject;
            var type = target.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(f => (field: f, attr: f.GetCustomAttribute<SgmExposeAttribute>()))
                .Where(x => x.attr != null)
/*                .OrderBy(x => x.attr.Group ?? "")
                .ThenBy(x => x.attr.Order)
                .ThenBy(x => x.field.Name)*/
                .ToList();

            if (fields.Count == 0)
            {
                root.Add(new HelpBox(
                    $"Aucun champ marqué avec [{nameof(SgmExposeAttribute)}] dans {type.Name}.",
                    HelpBoxMessageType.Info));
                return;
            }

            string currentGroup = null;
            VisualElement groupContainer = null;

            foreach (var (field, attr) in fields)
            {
                var group = attr.Group ?? "General";
                if (currentGroup != group)
                {
                    currentGroup = group;

                    var header = new Label(currentGroup);
                    header.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                    header.style.marginTop = 10;
                    header.style.marginBottom = 4;
                    root.Add(header);

                    groupContainer = new VisualElement();
                    groupContainer.style.marginLeft = 6;
                    root.Add(groupContainer);
                }

                // Find serialized property by field name
                var prop = so.FindProperty(field.Name);
                if (prop == null)
                {
                    groupContainer.Add(new HelpBox(
                        $"Impossible de trouver la SerializedProperty pour '{field.Name}'. " +
                        $"(Si c’est un champ auto-property ou non sérialisé, Unity ne le voit pas.)",
                        HelpBoxMessageType.Warning));
                    continue;
                }

                var pf = new PropertyField(prop, attr.Label ?? ObjectNames.NicifyVariableName(field.Name));
                pf.Bind(so);
                groupContainer.Add(pf);
            }

            // IMPORTANT: écoute les changements UI => Apply
            root.TrackSerializedObjectValue(so, _ =>
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
            });
        }
    }
}
