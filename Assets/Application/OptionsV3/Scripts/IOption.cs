using UnityEngine;

namespace LightHouse.Options.V3
{
    public interface IOption
    {
        public void Apply();
        public void Revert();
        public bool HasChanges();
    }
}

