using System;
using System.Collections.Generic;
using System.Linq;

namespace LightHouse.EditorTools.SuperGameManager
{
    public sealed class NavNode
    {
        public string Key { get; }
        public string DisplayName { get; set; }
        public int Order { get; set; }

        public Type PageType { get; set; } // null => juste un dossier/hub
        public NavNode Parent { get; private set; }

        private readonly List<NavNode> _children = new();
        public IReadOnlyList<NavNode> Children => _children;

        public bool IsLeaf => PageType != null && _children.Count == 0;

        public NavNode(string key, string displayName = null, int order = 0)
        {
            Key = key;
            DisplayName = displayName ?? key;
            Order = order;
        }

        public void AddChild(NavNode child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        public IEnumerable<NavNode> OrderedChildren()
            => _children.OrderBy(c => c.Order).ThenBy(c => c.DisplayName);
    }
}
