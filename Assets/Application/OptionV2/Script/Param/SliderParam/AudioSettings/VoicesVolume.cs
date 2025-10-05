using UnityEngine;
using UnityEngine.Audio;

public class VoicesVolume : SliderParam, IConfigurable
{
    [SerializeField] AudioMixer _audioMixer;

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
            //print("Voices Volume : " + _slider.value);
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
            //Debug.Log("Voices Volume reset");
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedValue == _slider.value;
    }
}
