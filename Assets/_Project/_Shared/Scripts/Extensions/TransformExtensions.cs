using UnityEngine;

namespace LightHouse.Features.TerrainSurface
{
    public static class TransformExtensions
    {
        #region Collider Interaction Layers
        public static void SetColliderInteractionLayers(this Transform transform, string layer)
        {
            foreach (Collider child in transform.GetComponentsInChildren<Collider>())
                child.gameObject.layer = LayerMask.NameToLayer(layer);
        }

        public static void SetColliderInteractionLayers(this Collider[] colliders, string layer)
        {
            foreach (Collider child in colliders)
                child.gameObject.layer = LayerMask.NameToLayer(layer);
        }

        public static void SetColliderInteractionLayers(this Collider collider, string layer)
        {
            collider.gameObject.layer = LayerMask.NameToLayer(layer);
        }

        #endregion

        #region Global Transforms Functions

        public static Quaternion SetRotation(this Transform t, Vector3 eulers)
        {
            return t.rotation = Quaternion.Euler(eulers);
        }

        public static Quaternion SetRotation(this Transform t, Quaternion quat)
        {
            return t.rotation = quat;
        }

        public static void ReparentAndAlign(this Transform transform, Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }

        public static void DestroyAllChildren(this Transform transform, bool destroyInactive = false)
        {
            foreach (Transform child in transform)
                if (child != transform)
                    if (destroyInactive || child.gameObject.activeSelf)
                        Object.Destroy(child.gameObject);
        }

        #endregion
    }

}
