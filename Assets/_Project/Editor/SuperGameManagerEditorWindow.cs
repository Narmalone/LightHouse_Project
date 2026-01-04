using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    public sealed class SuperGameManagerWindow : EditorWindow
    {
        private const string TOOLNAME = "SuperGameManager";

        private NavContext _ctx;
        private VisualElement _content;
        private ToolbarBreadcrumbs _breadcrumbs;
        private ToolbarSearchField _search;

        [MenuItem("Tools/SuperGameManager")]
        public static void Open()
        {
            var wnd = GetWindow<SuperGameManagerWindow>();
            wnd.titleContent = new GUIContent(TOOLNAME);
            wnd.minSize = new Vector2(820, 520);
        }

        public void CreateGUI()
        {
            NavRegistry.Rebuild();

            _ctx = new NavContext();
            _ctx.RequestNavigate = NavigateTo;
            _ctx.RequestBack = GoBack;

            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            // Top bar (Core / Features / Shared) + search
            var top = new Toolbar();
            root.Add(top);

            top.Add(MakeTopButton("Core"));
            top.Add(MakeTopButton("Features"));
            top.Add(MakeTopButton("Shared"));

            top.Add(new ToolbarSpacer() { flex = true });

            _search = new ToolbarSearchField();
            _search.RegisterValueChangedCallback(evt => RenderSearch(evt.newValue));
            _search.style.width = 260;
            top.Add(_search);

            // Breadcrumbs
            _breadcrumbs = new ToolbarBreadcrumbs();
            root.Add(_breadcrumbs);

            // Content
            _content = new VisualElement();
            _content.style.flexGrow = 1;
            _content.style.paddingLeft = 10;
            _content.style.paddingRight = 10;
            _content.style.paddingTop = 10;
            root.Add(_content);

            // Start on Core
            var core = NavRegistry.FindNodeByPath("Core");
            _ctx.ResetTo(core);
            RenderCurrent();
        }

        private Button MakeTopButton(string key)
        {
            return new Button(() =>
            {
                var node = NavRegistry.FindNodeByPath(key);
                if (node == null) return;
                _search.value = "";
                _ctx.ResetTo(node);
                RenderCurrent();
            })
            { text = key };
        }

        private void NavigateTo(NavNode node)
        {
            if (node == null) return;
            _ctx.Push(node);
            RenderCurrent();
        }

        private void GoBack()
        {
            _ctx.Pop();
            if (_ctx.Current == null)
                _ctx.ResetTo(NavRegistry.FindNodeByPath("Core"));
            RenderCurrent();
        }

        private void RenderCurrent()
        {
            var current = _ctx.Current;
            if (current == null) return;

            RenderBreadcrumbs();

            IToolPage page = CreatePageForNode(current);
            page.Build(_content, _ctx);
        }

        private IToolPage CreatePageForNode(NavNode node)
        {
            // Si node a une page dédiée, on l’instancie
            if (node.PageType != null)
            {
                try { return (IToolPage)Activator.CreateInstance(node.PageType); }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return new HubPage(node);
                }
            }

            // Sinon c’est un hub (dossier)
            return new HubPage(node);
        }

        private void RenderBreadcrumbs()
        {
            _breadcrumbs.Clear();

            var chain = _ctx.Stack.Reverse().ToArray(); // bas -> haut
            for (int i = 0; i < chain.Length; i++)
            {
                var n = chain[i];
                _breadcrumbs.PushItem(n.DisplayName, () =>
                {
                    // reset stack jusqu’à ce node
                    var target = n;
                    _ctx.ResetTo(target);
                    RenderCurrent();
                });
            }
        }

        private void RenderSearch(string query)
        {
            query = (query ?? "").Trim();
            if (query.Length == 0)
            {
                RenderCurrent();
                return;
            }

            _content.Clear();
            _breadcrumbs.Clear();

            var title = new Label($"Search: {query}");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16;
            title.style.marginBottom = 8;
            _content.Add(title);

            var results = FindNodesMatching(query);

            foreach (var node in results)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.marginBottom = 6;

                var pathLabel = new Label(GetFullPath(node));
                pathLabel.style.flexGrow = 1;

                var go = new Button(() =>
                {
                    _search.value = "";
                    _ctx.ResetTo(node);
                    RenderCurrent();
                })
                { text = "Open" };

                row.Add(pathLabel);
                row.Add(go);

                _content.Add(row);
            }
        }

        private NavNode[] FindNodesMatching(string q)
        {
            bool Match(NavNode n) => n.DisplayName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;

            var list = NavRegistry.Root.Children
                .SelectMany(Flatten)
                .Where(n => n != NavRegistry.Root && Match(n))
                .OrderBy(n => GetFullPath(n))
                .ToArray();

            return list;

            static System.Collections.Generic.IEnumerable<NavNode> Flatten(NavNode n)
            {
                yield return n;
                foreach (var c in n.Children)
                    foreach (var x in Flatten(c))
                        yield return x;
            }
        }

        private static string GetFullPath(NavNode node)
        {
            var parts = new System.Collections.Generic.List<string>();
            var cur = node;
            while (cur != null && cur.Key != "Root")
            {
                parts.Add(cur.DisplayName);
                cur = cur.Parent;
            }
            parts.Reverse();
            return string.Join(" / ", parts);
        }
    }
}
