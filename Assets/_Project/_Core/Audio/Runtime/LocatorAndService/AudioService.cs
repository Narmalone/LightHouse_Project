using LightHouse.Core.Audio.UnityBackend;
using UnityEngine;

namespace LightHouse.Core.Audio
{
    public class AudioService : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private AudioRegistry _registry;
        [SerializeField] private UnityAudioBackend _backend;

        public void Init(IVoiceLimiter limiter) => _backend.Initialize(limiter);

        public IAudioHandle PlayAt(string cueId, Vector3 pos, AudioPlayOptions opt = default)
        {
            if (!_registry.TryGet(cueId, out var cue)) { Debug.LogWarning($"Cue not found: {cueId}"); return AudioHandle.Null; }
            return _backend.Play(cue, pos, opt);
        }

        public IAudioHandle PlayAt(AudioCue cue, Vector3 position, AudioPlayOptions opt = default)
        {
            if (!cue) { Debug.LogWarning("Null AudioCue"); return AudioHandle.Null; }
            return _backend.Play(cue, position, opt);
        }

        public IAudioHandle PlayOneShot(string cueId, AudioPlayOptions opt = default)
        {
            AudioPlayOptions win = opt;
            win.Persistent = opt.Persistent;
            win.FollowTransform = false;
            return PlayAt(cueId, Vector3.zero, win);
        }

        public IAudioHandle PlayOneShot(AudioCue cue, AudioPlayOptions opt = default)
        {
            return PlayAt(cue, Vector3.zero, opt);
        }

        public void Preload(string cueId)
        {
            if (_registry.TryGet(cueId, out var cue))
            {
                // Hook Addressables si besoin plus tard
                foreach (var v in cue.Variants) { /* Addressables.DownloadDependenciesAsync(v.Clip) */ }
            }
        }

        public void StopAllOn(GameObject owner) => _backend.StopAllOwned(owner);
    }
}

