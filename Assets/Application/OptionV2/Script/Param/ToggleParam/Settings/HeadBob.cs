using UnityEngine;

public class HeadBob : ToggleParameter, IConfigurable
{
    private void Update()
    {
        //Debuging();
    }

    void Debuging()
    {
        if (HasBeenApplied())
        {
            print("Head Bob :" + HasBeenApplied());
        }
    }

    public bool HasChanged()
    {
        return _enable != _defaultEnable;
    }

    public void Apply()
    {
        if (HasChanged() && !HasBeenApplied())
        {
            //Debug.Log("Head Bob Apply");
            _appliedEnable = _enable;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            //Debug.Log("Head Bob reset");
            _enable = _defaultEnable;
            _appliedEnable = _defaultEnable;
            Toggle();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedEnable == _enable;
    }
}
