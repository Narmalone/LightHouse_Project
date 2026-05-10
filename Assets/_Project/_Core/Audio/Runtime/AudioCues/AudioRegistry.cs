using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Core.Audio
{
    [CreateAssetMenu(fileName = "SO_Registry_", menuName = GlobalAssetsMenuPaths.AudioAssetsMenuPath + "New Audio Registry")]
    //[Obsolete("Obsolete for now since most of the sounds are not used by it but it will be later with GUID'S")]
#pragma warning disable CS0618 // Type or member is obsolete
    public class AudioRegistry : ScriptableObject, IAudioRegistry
    {
        [SerializeField] private SO_AudioCue[] cues;

        // Index principaux
        public SerializedDictionary<string, SO_AudioCue> _byId;                 // data-driven (string)
        public SerializedDictionary<int, SO_AudioCue> _byHash;                  // clé compacte (int)
        public SerializedDictionary<AudioCategory, List<SO_AudioCue>> _byCat;   // navigation/menus
        public SerializedDictionary<AudioCategory, SerializedDictionary<string, SO_AudioCue>> _byCatAndName; // optionnel

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
                    _byCat[c.Category] = list = new List<SO_AudioCue>();
                list.Add(c);

                var idLeaf = LastSegment(c.Id); // ex. "Dirt" à partir de "Char/Footstep/Dirt"
                if (!_byCatAndName.TryGetValue(c.Category, out var dict))
                    _byCatAndName[c.Category] = dict = new SerializedDictionary<string, SO_AudioCue>();
                dict[idLeaf] = c; // lookup rapide Category+NomCourt si tu aimes ce pattern
            }
        }

        public bool TryGet(string id, out SO_AudioCue cue) => _byId.TryGetValue(id, out cue);
        public bool TryGet(int hash, out SO_AudioCue cue) => _byHash.TryGetValue(hash, out cue);
        public IReadOnlyList<SO_AudioCue> GetAll(AudioCategory cat) => _byCat.TryGetValue(cat, out var l) ? l : System.Array.Empty<SO_AudioCue>();
        public bool TryGet(AudioCategory cat, string shortName, out SO_AudioCue cue)
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
        bool TryGet(string id, out SO_AudioCue cue);
        bool TryGet(int hash, out SO_AudioCue cue);
        IReadOnlyList<SO_AudioCue> GetAll(AudioCategory cat);
        bool TryGet(AudioCategory cat, string shortName, out SO_AudioCue cue);
    }

}
