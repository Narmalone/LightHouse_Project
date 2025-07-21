using UnityEngine;

public class Crouch : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Crouch Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Crouch reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
