using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

public class RadioAudioController : MonoBehaviour
{
    [SerializeField] private AudioCue noiseCue;
    [SerializeField] private AudioCue voiceCue;
    [SerializeField] private RadioFrequencyController frequencyController;

    [SerializeField] private float maxDistance = 2f;

    private IAudioHandle _noiseHandle;
    private IAudioHandle _voiceHandle;

    private void OnEnable()
    {
        frequencyController.OnFrequencyChanged += UpdateAudio;
    }

    private void OnDisable()
    {
        frequencyController.OnFrequencyChanged -= UpdateAudio;
    }

    public void PlayAudio()
    {
        if(ServiceLocator.Audio != null)
        {
            _noiseHandle = ServiceLocator.Audio.PlayAt(noiseCue, this.transform.position);
            _voiceHandle = ServiceLocator.Audio.PlayAt(voiceCue, this.transform.position);
        }
    }

    public void StopAudio()
    {
        _noiseHandle?.Stop();
        _voiceHandle?.Stop();
    }

    private void UpdateAudio(float frequency, float targetFrequency)
    {
        float distance = Mathf.Abs(frequency - targetFrequency);

        float t = Mathf.InverseLerp(maxDistance, 0f, distance);

        if(_noiseHandle != null && _noiseHandle.CurrentSource != null)
            _noiseHandle.CurrentSource.volume = 1f - t;
        if(_voiceHandle != null && _voiceHandle.CurrentSource != null)
            _voiceHandle.CurrentSource.volume = t;
    }
}