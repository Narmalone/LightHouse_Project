using UnityEngine;

public interface ILEOWindow
{
    void Open();
    void Close();
    CanvasGroup CanvasGroup { get; }
}
