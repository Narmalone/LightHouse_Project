using System;
using UnityEngine.UIElements;

public class CustomVisualElement : VisualElement
{
    public event Action<string> OnClassAdded;
    public event Action<string> OnClassRemoved;
    public new void AddToClassList(string className)
    {
        base.AddToClassList(className);
        OnClassAdded?.Invoke(className);
    }

    public new void RemoveFromClassList(string className)
    {
        base.RemoveFromClassList(className);
        OnClassRemoved?.Invoke(className);
    }
}
