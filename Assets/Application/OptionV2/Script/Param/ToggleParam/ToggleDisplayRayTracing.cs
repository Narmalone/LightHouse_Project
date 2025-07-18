using UnityEngine;

public class ToggleRayTracing : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("RayTracing Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("RayTracing reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
