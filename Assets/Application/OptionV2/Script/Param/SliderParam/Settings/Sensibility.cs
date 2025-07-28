using UnityEngine;

public class Sensibility : SliderParam, IConfigurable
{
    [SerializeField] string _axis;

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
            print ("Sensibility" + _axis + " : " + _slider.value);
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
            print("Sensibility reset");
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedValue == _slider.value;
    }
}
