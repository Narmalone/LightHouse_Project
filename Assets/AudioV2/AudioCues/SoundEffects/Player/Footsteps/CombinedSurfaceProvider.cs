using UnityEngine;

public class CombinedSurfaceProvider : MonoBehaviour
{
    public TerrainSurfaceProvider terrainProvider;   // optionnel
    public SurfaceFallbackMap fallbackMap;           // optionnel

    public SurfaceType GetSurface(in RaycastHit hit)
    {
        // 1) Terrain
        if (terrainProvider && terrainProvider.TryGetSurfaceAt(hit, out var s))
            return s;

        // 2) PhysicMaterial / Layer
        var col = hit.collider;
        if (fallbackMap)
        {
            if (col && col.sharedMaterial)
            {
                var t = fallbackMap.FromPhysMat(col.sharedMaterial);
                if (t != fallbackMap.defaultSurface) return t;
            }
            return fallbackMap.FromLayer(col ? col.gameObject.layer : 0);
        }

        return SurfaceType.Default;
    }
}
