using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LightHouse.Core.Audio
{
    public interface IAudioSourcePool
    {
        AudioSource Rent(AudioMixerGroup group, bool persistent);
        void Return(AudioSource s);
    }

    public class AudioSourcePool : IAudioSourcePool
    {
        private int _initialSize = 20;
        private readonly Queue<AudioSource> _pool = new();
        private Transform _persistRoot, _sceneRoot;

        public AudioSourcePool(int initialSize)
        {
            _initialSize = initialSize;
            Init();
        }

        void Init()
        {
            _sceneRoot = new GameObject("Audio_Scene").transform;
            _persistRoot = new GameObject("Audio_Persistent").transform;
            UnityEngine.Object.DontDestroyOnLoad(_persistRoot.gameObject);

            for (int i = 0; i < _initialSize; i++) _pool.Enqueue(Create(false));
        }

        private AudioSource Create(bool persistent)
        {
            var go = new GameObject("AudioSource");
            go.transform.SetParent(persistent ? _persistRoot : _sceneRoot);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            return src;
        }

        public AudioSource Rent(AudioMixerGroup group, bool persistent)
        {
            var src = _pool.Count > 0 ? _pool.Dequeue() : Create(persistent);
            src.outputAudioMixerGroup = group;
            src.gameObject.SetActive(true);
            return src;
        }

        public void Return(AudioSource s)
        {
            if (!s) return;
            s.Stop();
            s.clip = null;
            s.transform.SetParent(s.outputAudioMixerGroup ? _sceneRoot : _persistRoot, true);
            s.gameObject.SetActive(false);
            _pool.Enqueue(s);
        }
    }
}
