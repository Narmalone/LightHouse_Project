using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WavesController : MonoBehaviour
{
    [SerializeField] private OceanSettings currentSettings;

    [SerializeField] private WaterDeformer _waterDeformer;
    [SerializeField] private AudioSource _waveAudioSource;
    [SerializeField] private ShoreDecalTrigger _shoreDecal;

    public void OverrideSettings(OceanSettings settings)
    {
        currentSettings = settings;
    }

    private void Awake()
    {
        _shoreDecal.OnWaveGo += () =>
        {
            _waveAudioSource.Play();
        };
    }

    private void Update()
    {
        
    }
}
