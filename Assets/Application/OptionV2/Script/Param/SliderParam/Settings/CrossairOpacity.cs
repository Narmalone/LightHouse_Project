public class CrossairOpacity : SliderParam, IConfigurable
{
    new private void Start()
    {
        _defaultValue = 1;
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
            //print ("Crossair Opacity : " + _slider.value);
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
            //print("Crossair Opacity reset");
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedValue == _slider.value;
    }
}
