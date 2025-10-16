using UnityEngine;

public class TerrainSurfaceProvider : MonoBehaviour
{
    [SerializeField] private TerrainSurfaceMap map;

    public bool TryGetSurfaceAt(RaycastHit hit, out SurfaceType type)
    {
        type = map ? map.defaultSurface : SurfaceType.Default;

        if (hit.collider is TerrainCollider tc)
        {
            var terrain = tc.GetComponent<Terrain>();
            if (terrain)
            {
                int idx = TerrainSampling.GetDominantLayerIndex(terrain, hit.point);
                var layers = terrain.terrainData.terrainLayers;
                if (idx >= 0 && idx < layers.Length)
                {
                    var layer = layers[idx];
                    if (map) type = map.FromLayer(layer);
                    return true;
                }
            }
        }
        return false;
    }
}
