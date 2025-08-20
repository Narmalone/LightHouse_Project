using UnityEngine;

public abstract class EnumWrapper : ScriptableObject
{
    public abstract string[] GetNames();
    public abstract int GetCount();
    public abstract string GetName(int index);
    public abstract void SetIndex(int index);
    public abstract int GetIndex();
}

