using System;
using System.Linq;
using UnityEditor;

namespace LightHouse.EditorTools.SuperGameManager
{
    [InitializeOnLoad]
    public static class NavRegistry
    {
        public static NavNode Root { get; private set; }

        static NavRegistry()
        {
            Rebuild();
        }

        public static void Rebuild()
        {
            Root = new NavNode("Root", "Root");

            // Scanne toutes les classes dans l’assemblage Editor (et autres assemblies chargées)
            var pageTypes = TypeCache.GetTypesDerivedFrom<IToolPage>();

            foreach (var type in pageTypes)
            {
                var attr = (SgmPageAttribute)Attribute.GetCustomAttribute(type, typeof(SgmPageAttribute));
                if (attr == null) continue;

                AddPath(attr.Path, attr.Display, attr.Order, type);
            }

            // Garantir les 3 top nodes même si aucune page n’existe encore
            EnsureTop("Core", 0);
            EnsureTop("Features", 1);
            EnsureTop("Shared", 2);
        }

        private static void EnsureTop(string key, int order)
        {
            if (Root.Children.Any(c => c.Key == key)) return;
            Root.AddChild(new NavNode(key, key, order));
        }

        private static void AddPath(string path, string display, int order, Type pageType)
        {
            var parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            NavNode current = Root;

            for (int i = 0; i < parts.Length; i++)
            {
                string key = parts[i];
                var existing = current.Children.FirstOrDefault(c => c.Key == key);

                if (existing == null)
                {
                    existing = new NavNode(key, key);
                    current.AddChild(existing);
                }

                current = existing;
            }

            // La feuille correspond à la page
            current.DisplayName = display ?? current.DisplayName;
            current.Order = order;
            current.PageType = pageType;
        }

        public static NavNode FindNodeByPath(string path)
        {
            var parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            NavNode current = Root;
            foreach (var p in parts)
            {
                current = current.Children.FirstOrDefault(c => c.Key == p);
                if (current == null) return null;
            }
            return current;
        }
    }
}
