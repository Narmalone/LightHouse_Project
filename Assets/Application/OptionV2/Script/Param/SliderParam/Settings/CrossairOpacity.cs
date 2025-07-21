using System.Diagnostics;

public class CrossairOpacity : SliderParam, IConfigurable
{
    new private void Start()
    {
        _defaultValue = 1f;
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
            print ("Crossair Opacity : " + _slider.value);
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
            print("Crossair Opacity reset");
        }
    }
}
