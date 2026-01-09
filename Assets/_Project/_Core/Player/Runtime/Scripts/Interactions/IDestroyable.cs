using System;

namespace LightHouse.Interactions
{
    public interface IDestroyable
    {
        event Action OnDestroyed;
    }

}
