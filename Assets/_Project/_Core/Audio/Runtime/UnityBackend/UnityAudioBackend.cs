using System.Linq;
using UnityEngine;

public class UnityAudioBackend : MonoBehaviour, IAudioBackend
{
    [SerializeField] private AudioSourcePool _pool;
    //[SerializeField] private string busPrefix = "Bus:"; // ex: "Bus:SFX"
    private IVoiceLimiter _limiter;

    public void Initialize(IVoiceLimiter limiter) => _limiter = limiter;

    public IAudioHandle Play(AudioCue cue, Vector3 pos, AudioPlayOptions opt)
    {
        // Limites cue-level
        if (cue.MaxSimultaneousVoices > 0)
        {
            var key = $"Cue:{cue.Id}";
            if (!_limiter.TryEnter(key)) return AudioHandle.Null;
        }

        // Choix de variant pondéré
        var v = Pick(cue);
        if (v == null || v.Clip == null) return AudioHandle.Null;

        var group = opt.OverrideGroup ? opt.OverrideGroup : cue.MixerGroup;
        var src = _pool.Rent(group, opt.Persistent);
        ConfigureSource(src, cue, v, opt);
        src.transform.position = pos;

        var handle = new AudioHandle(src, _pool, cue, _limiter, opt.Owner);
        if (opt.FollowTransform && opt.Owner) handle.Follow(opt.Owner.transform);
        src.Play();
        return handle;
    }

    public void StopAllOwned(GameObject owner)
    {
        foreach (var h in AudioHandle.Active.Where(h => h.Owner == owner).ToList())
            h.Stop();
    }

    private static CueVariant Pick(AudioCue cue)
    {
        if (cue.Variants == null || cue.Variants.Length == 0) return null;
        float sum = cue.Variants.Sum(x => Mathf.Max(0.0001f, x.Weight));
        float r = Random.value * sum;
        foreach (var v in cue.Variants)
        {
            r -= Mathf.Max(0.0001f, v.Weight);
            if (r <= 0) return v;
        }
        return cue.Variants[cue.Variants.Length - 1];
    }

    private void ConfigureSource(AudioSource s, AudioCue cue, CueVariant v, AudioPlayOptions opt)
    {
        s.loop = cue.Loop;
        s.clip = v.Clip;

        bool is2D = opt.Force2D ?? (cue.Spatial == Spatialization.TwoD);
        s.spatialBlend = opt.SpatialBlend.HasValue ? Mathf.Clamp01(opt.SpatialBlend.Value) : (is2D ? 0f : 1f);

        if (!is2D)
        {
            s.rolloffMode = cue.Rolloff; // AudioRolloffMode (Unity)
            s.minDistance = cue.MinDistance;
            s.maxDistance = cue.MaxDistance;
            s.dopplerLevel = cue.DopplerLevel;
        }
        else
        {
            // En 2D ces réglages sont sans effet, on met juste ce qui est utile
            s.panStereo = cue.StereoPan;
            s.dopplerLevel = 0f;
        }

        s.bypassListenerEffects = cue.BypassListenerFx;
        s.bypassReverbZones = cue.BypassReverbZones;

        float vol = cue.Volume * (1f + Random.Range(-cue.RandomVolume, cue.RandomVolume)) * (opt.VolumeMul == 0 ? 1f : opt.VolumeMul);
        float pit = cue.Pitch * (1f + Random.Range(-cue.RandomPitch, cue.RandomPitch)) * (opt.PitchMul == 0 ? 1f : opt.PitchMul);
        s.volume = Mathf.Clamp01(vol);
        s.pitch = Mathf.Clamp(pit, -3f, 3f);
    }


}