using System;

namespace LightHouse.Features.Items.Detection
{
    public interface IDestroyable
    {
        event Action OnDestroyed;
    }

}
