using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Terrain Surface Map")]
public class TerrainSurfaceMap : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public TerrainLayer Layer;   // référence à l'asset TL_*
        public SurfaceType Surface;
    }

    public SurfaceType defaultSurface = SurfaceType.Default;
    public List<Entry> entries = new();

    public SurfaceType FromLayer(TerrainLayer layer)
    {
        if (!layer) return defaultSurface;
        foreach (var e in entries)
            if (e.Layer == layer) return e.Surface; // comparaison par référence d’asset
        return defaultSurface;
    }
}
