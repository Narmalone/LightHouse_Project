using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    public sealed class ConfigAssetPanel<T> where T : ScriptableObject
    {
        private readonly string _title;
        private readonly string _objectFieldLabel;
        private readonly string _defaultFolder;
        private readonly string _defaultFileName;

        private T _asset;
        private SerializedObject _so;

        private ObjectField _assetField;
        private VisualElement _inspector;

        private VisualElement _root; // <-- container interne du panel

        private readonly Action<VisualElement, SerializedObject> _drawInspector;

        public T Asset => _asset;

        public ConfigAssetPanel(
            string title,
            string objectFieldLabel,
            string defaultFolder,
            string defaultFileName,
            Action<VisualElement, SerializedObject> drawInspector = null
        )
        {
            _title = title;
            _objectFieldLabel = objectFieldLabel;
            _defaultFolder = defaultFolder;
            _defaultFileName = defaultFileName;
            _drawInspector = drawInspector ?? DefaultDrawInspector;
        }

        /// <summary>
        /// Ajoute le panel dans un parent (sans jamais clear le parent).
        /// Renvoie le container du panel (pratique si tu veux styliser/masquer).
        /// </summary>
        public VisualElement AttachTo(VisualElement parent)
        {
            _root ??= CreateVisualTree();
            parent.Add(_root);

            // Refresh UI
            RebuildInspector();
            return _root;
        }

        /// <summary>Optionnel: pour enlever le panel.</summary>
        public void Detach()
        {
            _root?.RemoveFromHierarchy();
        }

        public void SetAsset(T asset)
        {
            _asset = asset;
            _assetField?.SetValueWithoutNotify(_asset);
            RebuildInspector();
        }

        private VisualElement CreateVisualTree()
        {
            // Un “card” simple
            var card = new VisualElement();
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.marginBottom = 10;

            // un petit look "panel"
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;

            // Title
            var title = new Label(_title);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 14;
            title.style.marginBottom = 6;
            card.Add(title);

            // Asset field
            _assetField = new ObjectField(_objectFieldLabel)
            {
                objectType = typeof(T),
                allowSceneObjects = false
            };
            _assetField.RegisterValueChangedCallback(evt => SetAsset(evt.newValue as T));
            card.Add(_assetField);

            // Buttons row
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            //row.style.gap = 6;
            row.style.marginTop = 6;
            card.Add(row);

            row.Add(new Button(Ping) { text = "Ping" });
            row.Add(new Button(FindFirst) { text = "Find First" });
            row.Add(new Button(CreateNew) { text = "Create New" });
            row.Add(new Button(Clear) { text = "Clear" });

            // Inspector container
            _inspector = new VisualElement();
            _inspector.style.marginTop = 10;
            card.Add(_inspector);

            return card;
        }

        private void Ping()
        {
            if (_asset == null) return;
            EditorGUIUtility.PingObject(_asset);
            Selection.activeObject = _asset;
        }

        private void FindFirst()
        {
            var found = AssetUtils.FindFirstAsset<T>();
            SetAsset(found);
        }

        private void CreateNew()
        {
            var created = AssetUtils.CreateScriptableAsset<T>(_defaultFolder, _defaultFileName);
            SetAsset(created);
        }

        private void Clear()
        {
            SetAsset(null);
        }

        private void RebuildInspector()
        {
            _inspector?.Clear();

            if (_asset == null)
            {
                _inspector?.Add(new HelpBox(
                    $"Aucun asset assigné. Utilise Find First, Create New, ou drag & drop un {typeof(T).Name}.",
                    HelpBoxMessageType.Info));
                return;
            }

            _so = new SerializedObject(_asset);
            _drawInspector?.Invoke(_inspector, _so);
        }

        private static void DefaultDrawInspector(VisualElement container, SerializedObject so)
        {
            SerializedObjectAutoUI.BuildExposedFieldsUI(container, so);
        }
    }
}
