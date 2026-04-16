using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;
using UnityEngine.Audio;

public class AmbianceMenuController : MonoBehaviour
{
    [SerializeField] private AudioMixer _sfxMixer;
    [SerializeField] private AudioCue _rainEffect;
    [SerializeField] private AudioCue _windEffect;

    private void Start()
    {
        _sfxMixer.SetFloat("Rain_LPF_Cutoff", Mathf.Clamp(1800f, 20f, 22000f));//data from the rain controller
        ServiceLocator.Audio.PlayAt(_rainEffect, this.transform.position);
        ServiceLocator.Audio.PlayAt(_windEffect, this.transform.position);
    }
}
