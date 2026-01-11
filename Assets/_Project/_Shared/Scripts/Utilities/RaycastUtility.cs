using UnityEngine;

namespace LightHouse.Core.Utilities
{
    public static class RaycastUtility
    {
        /// <summary>
        /// Effectue un raycast et retourne l'objet touché.
        /// </summary>
        public static bool TryRaycast(Ray ray, float distance, LayerMask layerMask, QueryTriggerInteraction triggerInteraction, out RaycastHit hit)
        {
            return Physics.Raycast(ray, out hit, distance, layerMask, triggerInteraction);
        }

        /// <summary>
        /// Effectue un raycast et retourne l'objet touché.
        /// </summary>
        public static bool TryRaycast(Vector3 pos, Vector3 dir, float distance, LayerMask layerMask, QueryTriggerInteraction triggerInteraction, out RaycastHit hit)
        {
            return Physics.Raycast(pos, dir, out hit, distance, layerMask, triggerInteraction);
        }
    }

}
