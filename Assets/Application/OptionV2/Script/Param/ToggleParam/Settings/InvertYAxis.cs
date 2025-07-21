using UnityEngine;

public class InvertYAxis : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Invert Y Axis Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Invert Y Axis reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
