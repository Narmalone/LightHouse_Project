using UnityEngine;

namespace LightHouse.Features.Computer.LEO
{
    public interface ILEOWindow
    {
        void Open();
        void Close();
        CanvasGroup CanvasGroup { get; }
    }
}
