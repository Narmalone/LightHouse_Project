using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.TerrainSurface
{
    [CreateAssetMenu(menuName = "LightHouse/Terrain/Surface Fallback Map")]
    public class SurfaceFallbackMap : ScriptableObject
    {
        [Serializable] public class PhysMatEntry { public PhysicsMaterial PhysMat; public SurfaceType Surface; }
        [Serializable] public class LayerEntry { public LayerMask Layer; public SurfaceType Surface; }

        public SurfaceType defaultSurface = SurfaceType.Default;
        public List<PhysMatEntry> physicMaterials = new();
        public List<LayerEntry> layers = new();

        public SurfaceType FromPhysMat(PhysicsMaterial pm)
        {
            if (!pm) return defaultSurface;
            foreach (var e in physicMaterials) if (e.PhysMat == pm) return e.Surface;
            return defaultSurface;
        }

        public SurfaceType FromLayer(int layer)
        {
            foreach (var e in layers)
                if ((e.Layer.value & (1 << layer)) != 0) return e.Surface;
            return defaultSurface;
        }
    }

}
