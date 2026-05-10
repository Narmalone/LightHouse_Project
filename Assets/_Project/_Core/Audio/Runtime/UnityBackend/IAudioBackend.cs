using UnityEngine;

namespace LightHouse.Core.Audio
{
    public interface IAudioBackend
    {
        IAudioHandle Play(SO_AudioCue cue, Vector3 pos, AudioPlayOptions opt);
        void StopAllOwned(GameObject owner);
    }

    public class AudioHandle : IAudioHandle
    {
        public static readonly System.Collections.Generic.HashSet<AudioHandle> Active = new();

        private AudioSource _src;
        private readonly IAudioSourcePool _pool;
        private readonly IVoiceLimiter _limiter;
        private readonly string _cueKey;
        private readonly AudioSourceDriver _driver;

        public GameObject Owner { get; }

        public AudioHandle(AudioSource s, IAudioSourcePool pool, SO_AudioCue cue, IVoiceLimiter limiter, GameObject owner)
        {
            _src = s; _pool = pool; _limiter = limiter; Owner = owner;
            _cueKey = $"Cue:{cue.Id}";
            Active.Add(this);

            _driver = s.GetComponent<AudioSourceDriver>() ?? s.gameObject.AddComponent<AudioSourceDriver>();
            _driver.Bind(this, s); // le driver connaît sa source et ce handle
        }

        public bool IsValid => _src;

        public void Stop(float fadeOut = 0f)
        {
            if (!_src) return;
            if (fadeOut > 0f) _driver.StartFadeOutAndReturn(fadeOut);
            else ReturnNow();
        }

        public void SetVolume(float v) { if (_src) _src.volume = Mathf.Clamp01(v); }
        public void SetPitch(float p) { if (_src) _src.pitch = Mathf.Clamp(p, -3f, 3f); }
        public void SetPosition(Vector3 p) { if (_src) _src.transform.position = p; }

        public void Follow(Transform t) { _driver.SetFollow(t); }

        // <-- appelé par le driver (ou Stop sans fade)
        public void ReturnNow()
        {
            Active.Remove(this);
            _limiter?.Exit(_cueKey);
            _pool.Return(_src);
        }

        public static readonly AudioHandle NullHandle = null;
        public static IAudioHandle Null => NullHandle;

        public AudioSource CurrentSource => _src;
        public AudioClip SelectedClip => _src.clip;
    }

}

