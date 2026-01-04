using System;

namespace LightHouse.EditorTools.SuperGameManager
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SgmPageAttribute : Attribute
    {
        public readonly string Path;     // ex: "Core/Audio" ou "Features/OS3D/Nightwatch"
        public readonly string Display;  // ex: "Audio"
        public readonly int Order;

        public SgmPageAttribute(string path, string display, int order = 0)
        {
            Path = path;
            Display = display;
            Order = order;
        }
    }
}
