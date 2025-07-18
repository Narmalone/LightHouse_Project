using UnityEngine;

public interface IConfigurable
{
    bool HasChanged();
    void Apply();
    void Reset();
}
