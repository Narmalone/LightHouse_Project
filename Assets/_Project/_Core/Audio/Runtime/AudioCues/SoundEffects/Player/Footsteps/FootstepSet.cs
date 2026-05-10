using LightHouse.Core.Audio;
using LightHouse.Features.TerrainSurface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Core.Player.Footsteps
{
    [CreateAssetMenu(menuName = "LightHouse/Audio/Footstep Set")]
    public class FootstepSet : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public SurfaceType Surface;
            public SO_AudioCue Cue;   // ta banque de pas pour cette surface
        }

        public SO_AudioCue DefaultCue;
        public List<Entry> Entries = new();

        public SO_AudioCue GetCue(SurfaceType s)
        {
            foreach (var e in Entries) if (e.Surface == s && e.Cue) return e.Cue;
            return DefaultCue;
        }
    }
}
