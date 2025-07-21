using UnityEngine;

public class Vibrations : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Vibrations Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Vibrations reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
