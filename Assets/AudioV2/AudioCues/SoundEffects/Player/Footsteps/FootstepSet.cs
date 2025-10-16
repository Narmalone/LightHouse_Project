using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Footstep Set")]
public class FootstepSet : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public SurfaceType Surface;
        public AudioCue Cue;   // ta banque de pas pour cette surface
    }

    public AudioCue DefaultCue;
    public List<Entry> Entries = new();

    public AudioCue GetCue(SurfaceType s)
    {
        foreach (var e in Entries) if (e.Surface == s && e.Cue) return e.Cue;
        return DefaultCue;
    }
}
