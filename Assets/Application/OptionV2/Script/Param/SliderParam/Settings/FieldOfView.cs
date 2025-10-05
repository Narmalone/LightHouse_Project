using System.Diagnostics;

public class FieldOfView : SliderParam, IConfigurable
{
    new private void Start()
    {
        _defaultValue = 0.9f;
        base.Start();
    }
    void Update()
    {
        //Apply();
        //Debuging();
    }

    void Debuging()
    {
        if (HasBeenApplied())
        {
            //print("Field Of View : " + HasBeenApplied());
        }
    }
    public void Apply()
    {
        if (HasChanged())
        {
            //print ("FOV : " + _slider.value);
            _appliedValue = _slider.value;
        }
    }
     
    public bool HasChanged()
    {
        return _slider.value != _defaultValue;
    }

    public void Reset()
    {
        if (HasChanged()) 
        {
            _slider.value = _defaultValue;
            _appliedValue = _defaultValue;
            //print("FOV reset");
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedValue == _slider.value;
    }
}
