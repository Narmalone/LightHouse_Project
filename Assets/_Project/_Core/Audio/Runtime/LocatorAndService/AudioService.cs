using LightHouse.Core.Audio.UnityBackend;
using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Core.Audio
{
    public class AudioService : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private int _poolInitialSize = 20;
        [SerializeField] private int MaxAmbianceAtSameTime = 8;
        [SerializeField] private int MaxSFXAtSameTime = 32;
        [SerializeField] private int MaxMusicsAtSameTime = 2;
        [SerializeField] private AudioRegistry _registry;
        [SerializeField] private UnityAudioBackend _backend;

        protected void Awake()
        {
            var limits = new System.Collections.Generic.Dictionary<string, int>
            {
                { "Bus:Ambience", MaxAmbianceAtSameTime },
                { "Bus:SFX", MaxSFXAtSameTime },
                { "Bus:MUSICS", MaxMusicsAtSameTime },
                // par Cue (facultatif, redondant avec cue.MaxSimultaneousVoices)
            };
            _backend = new UnityAudioBackend(new TokenBucketLimiter(limits), new AudioSourcePool(_poolInitialSize));
            ServiceLocator.SetAudioPlayer(this);
        }

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

