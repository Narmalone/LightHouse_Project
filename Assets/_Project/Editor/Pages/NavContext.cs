using System;
using System.Collections.Generic;

namespace LightHouse.EditorTools.SuperGameManager
{
    public sealed class NavContext
    {
        private readonly Stack<NavNode> _stack = new();

        public NavNode Current => _stack.Count > 0 ? _stack.Peek() : null;
        public IReadOnlyCollection<NavNode> Stack => _stack;

        public Action<NavNode> RequestNavigate;
        public Action RequestBack;

        public void Push(NavNode node) => _stack.Push(node);

        public void Pop()
        {
            if (_stack.Count > 0) _stack.Pop();
        }

        public void ResetTo(NavNode node)
        {
            _stack.Clear();
            _stack.Push(node);
        }
    }
}
