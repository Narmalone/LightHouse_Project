using UnityEngine;

public class UseSubtitles : ToggleParameter, IConfigurable
{
    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Use Subtitles Apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Use Subtitles reset");
            _enable = _defaultEnable;
            Toggle();
        }
    }
}
