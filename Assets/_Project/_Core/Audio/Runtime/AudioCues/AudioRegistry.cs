using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioRegistry")]
public class AudioRegistry : ScriptableObject, IAudioRegistry
{
    [SerializeField] private AudioCue[] cues;

    // Index principaux
    public SerializedDictionary<string, AudioCue> _byId;                 // data-driven (string)
    public SerializedDictionary<int, AudioCue> _byHash;                  // clé compacte (int)
    public SerializedDictionary<AudioCategory, List<AudioCue>> _byCat;   // navigation/menus
    public SerializedDictionary<AudioCategory, SerializedDictionary<string, AudioCue>> _byCatAndName; // optionnel

    void OnEnable()
    {
        RegenerateDatas();
    }

    public void RegenerateDatas()
    {
        _byId = new(cues.Length);
        _byHash = new(cues.Length);
        _byCat = new();
        _byCatAndName = new();

        foreach (var c in cues)
        {
            if (!c) continue;
            if (!string.IsNullOrEmpty(c.Id)) _byId[c.Id] = c;

            int h = StableHash(c.Id); // hash 32-bit stable (voir impl ci-dessous)
            _byHash[h] = c;

            if (!_byCat.TryGetValue(c.Category, out var list))
                _byCat[c.Category] = list = new List<AudioCue>();
            list.Add(c);

            var idLeaf = LastSegment(c.Id); // ex. "Dirt" à partir de "Char/Footstep/Dirt"
            if (!_byCatAndName.TryGetValue(c.Category, out var dict))
                _byCatAndName[c.Category] = dict = new SerializedDictionary<string, AudioCue>();
            dict[idLeaf] = c; // lookup rapide Category+NomCourt si tu aimes ce pattern
        }
    }

    public bool TryGet(string id, out AudioCue cue) => _byId.TryGetValue(id, out cue);
    public bool TryGet(int hash, out AudioCue cue) => _byHash.TryGetValue(hash, out cue);
    public IReadOnlyList<AudioCue> GetAll(AudioCategory cat) => _byCat.TryGetValue(cat, out var l) ? l : System.Array.Empty<AudioCue>();
    public bool TryGet(AudioCategory cat, string shortName, out AudioCue cue)
    {
        cue = null;
        return _byCatAndName.TryGetValue(cat, out var d) && d.TryGetValue(shortName, out cue);
    }

    // --- helpers ---
    private static string LastSegment(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        int i = s.LastIndexOf('/');
        return i >= 0 ? s[(i + 1)..] : s;
    }

    // Hash stable non alloc (ex: FNV-1a 32)
    private static int StableHash(string s)
    {
        unchecked
        {
            const uint fnvOffset = 2166136261;
            const uint fnvPrime = 16777619;
            uint hash = fnvOffset;
            for (int i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= fnvPrime;
            }
            return (int)hash;
        }
    }
}

public interface IAudioRegistry
{
    bool TryGet(string id, out AudioCue cue);
    bool TryGet(int hash, out AudioCue cue);
    IReadOnlyList<AudioCue> GetAll(AudioCategory cat);
    bool TryGet(AudioCategory cat, string shortName, out AudioCue cue);
}
