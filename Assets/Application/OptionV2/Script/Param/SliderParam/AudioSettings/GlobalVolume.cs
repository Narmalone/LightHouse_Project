using UnityEngine;
using UnityEngine.Audio;

public class GlobalVolume : SliderParam, IConfigurable
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
            print("Global Volume : " + _slider.value);
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
            Debug.Log("Global Volume reset");
        }
    }
}
