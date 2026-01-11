using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Core.Audio
{
    public class AudioBootstrap : MonoBehaviour
    {
        [SerializeField] private AudioService _service;
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
                { "Cue:Env/Rain/Heavy", 2 }
            };
            _service.Init(new TokenBucketLimiter(limits));
            ServiceLocator.Audio = _service; // simple locator si tu n’utilises pas Zenject
        }
    }
}

