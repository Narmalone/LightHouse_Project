using UnityEngine;

namespace LightHouse.Features.TerrainSurface
{
    public static class TerrainSampling
    {
        // Renvoie l'index de la couche la plus dominante au point worldPos
        public static int GetDominantLayerIndex(UnityEngine.Terrain terrain, Vector3 worldPos)
        {
            var td = terrain.terrainData;
            Vector3 tpos = worldPos - terrain.transform.position;

            int mapX = Mathf.Clamp(Mathf.RoundToInt(tpos.x / td.size.x * td.alphamapWidth), 0, td.alphamapWidth - 1);
            int mapZ = Mathf.Clamp(Mathf.RoundToInt(tpos.z / td.size.z * td.alphamapHeight), 0, td.alphamapHeight - 1);

            float[,,] weights = td.GetAlphamaps(mapX, mapZ, 1, 1);
            int best = 0; float bestVal = 0f;
            for (int i = 0; i < weights.GetLength(2); i++)
            {
                float v = weights[0, 0, i];
                if (v > bestVal) { best = i; bestVal = v; }
            }
            return best;
        }
    }
}
