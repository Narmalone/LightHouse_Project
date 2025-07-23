using UnityEngine;

public class PullPush : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged() && !HasBeenApplied())
        {
            Debug.Log("Pull/Push Apply");
            _appliedEnable = _enable;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Pull/Push reset");
            _enable = _defaultEnable;
            _appliedEnable = _defaultEnable;
            Toggle();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedEnable = _enable;
    }
}
