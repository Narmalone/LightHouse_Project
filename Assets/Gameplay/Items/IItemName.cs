using System;

namespace LightHouse.Interactions
{
    public interface IItemName
    {
        public event Action OnNameUpdated;
        string GetName();
    }
}
