public class Luminosity : SliderParam, IConfigurable
{
    new private void Start()
    {
        _defaultValue = 0.5f;
        base.Start();
    }
    void Update()
    {
        //Apply();
    }

    public void Apply()
    {
        if (HasChanged())
        {
            print ("luminosity : " + _slider.value);
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
            print("luminosity reset");
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedValue == _slider.value;
    }
}
