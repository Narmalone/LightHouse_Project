using UnityEngine;

public abstract class EnumWrapper : MonoBehaviour, IConfigurable
{
    public abstract string[] GetNames();
    public abstract int GetCount();
    public abstract string GetName(int index);
    public abstract void SetIndex(int index);
    public abstract int GetIndex();

    public bool HasChanged()
    {
        throw new System.NotImplementedException();
    }

    public void Apply()
    {
        throw new System.NotImplementedException();
    }

    public void Revert()
    {
        throw new System.NotImplementedException();
    }
}
