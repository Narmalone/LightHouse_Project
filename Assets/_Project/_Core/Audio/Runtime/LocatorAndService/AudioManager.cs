using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Core.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioRegistry _registry;
        [SerializeField] private AudioService _service;
        [SerializeField] private int _poolInitialSize = 20;
        [SerializeField] private int MaxAmbianceAtSameTime = 8;
        [SerializeField] private int MaxSFXAtSameTime = 32;
        [SerializeField] private int MaxMusicsAtSameTime = 2;

        void Awake()
        {
            var limits = new System.Collections.Generic.Dictionary<string, int>
            {
                { "Bus:Ambience", MaxAmbianceAtSameTime },
                { "Bus:SFX", MaxSFXAtSameTime },
                { "Bus:MUSICS", MaxMusicsAtSameTime },
                // par Cue (facultatif, redondant avec cue.MaxSimultaneousVoices)
            };

            _service = new AudioService(new TokenBucketLimiter(limits), _registry, new AudioSourcePool(_poolInitialSize));
            ServiceLocator.Audio = _service;
        }
    }
}

