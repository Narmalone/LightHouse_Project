using UnityEngine;

public class Hold : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Hold Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Hold reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
