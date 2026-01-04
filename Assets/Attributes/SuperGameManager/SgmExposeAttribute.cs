using System;

namespace LightHouse.EditorTools.SuperGameManager
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SgmExposeAttribute : Attribute
    {
        public readonly string Group;   // ex: "General", "Debug"
        public readonly string Label;   // override label (optionnel)
        public readonly int Order;

        public SgmExposeAttribute(string group = null, string label = null, int order = 0)
        {
            Group = group;
            Label = label;
            Order = order;
        }
    }
}
