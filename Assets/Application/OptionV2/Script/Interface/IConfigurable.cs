using UnityEngine;

public interface IConfigurable
{
    void Apply();
    void Reset();
    bool HasChanged();
    bool HasBeenApplied();
}
