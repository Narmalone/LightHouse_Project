using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;
using UnityEngine.Audio;

public class AmbianceMenuController : MonoBehaviour
{
    [SerializeField] private AudioMixer _sfxMixer;
    [SerializeField] private AudioCue _ambianceHorrorSound;
    [SerializeField] private AudioCue _rainEffect;
    [SerializeField] private AudioCue _windEffect;

    private void Start()
    {
        ServiceLocator.Audio?.PlayAt(_ambianceHorrorSound, this.transform.position);
        ServiceLocator.Audio?.PlayAt(_rainEffect, this.transform.position);
        ServiceLocator.Audio?.PlayAt(_windEffect, this.transform.position);
    }
}
