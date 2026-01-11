using UnityEngine;
using System.Collections;

namespace LightHouse.Core.Audio 
{
    public class AudioSourceDriver : MonoBehaviour
    {
        private AudioSource _src;
        private AudioHandle _handle;
        private Transform _follow;

        public void Bind(AudioHandle handle, AudioSource src)
        {
            _handle = handle;
            _src = src;
        }

        public void SetFollow(Transform t) => _follow = t;

        void Update()
        {
            // Follow 3D
            if (_src && _follow) _src.transform.position = _follow.position;

            // Auto-return quand non-looping terminé
            if (_src && !_src.loop && !_src.isPlaying)
                _handle?.ReturnNow();
        }

        public void StartFadeOutAndReturn(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(FadeAndReturn(duration));
        }

        private IEnumerator FadeAndReturn(float d)
        {
            if (!_src) yield break;
            float t = 0f; float start = _src.volume;
            while (t < d && _src)
            {
                t += Time.deltaTime;
                _src.volume = Mathf.Lerp(start, 0f, t / d);
                yield return null;
            }
            _handle?.ReturnNow();
        }
    }

}
