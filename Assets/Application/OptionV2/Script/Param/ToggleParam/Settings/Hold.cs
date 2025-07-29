using UnityEngine;

public class Hold : ToggleParameter, IConfigurable
{
    private void Update()
    {
        //Debuging();
    }
    void Debuging()
    {
        if (HasBeenApplied())
        {
            print("Hold :" + HasBeenApplied());
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
            //Debug.Log("Hold Apply");
            _appliedEnable = _enable;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            //Debug.Log("Hold reset");
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
