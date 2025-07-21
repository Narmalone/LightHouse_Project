using UnityEngine;

public class PullPush : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Pull/Push Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Pull/Push reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
