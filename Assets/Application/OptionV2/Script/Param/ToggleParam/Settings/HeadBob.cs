using UnityEngine;

public class HeadBob : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Head Bob Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Head Bob reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
