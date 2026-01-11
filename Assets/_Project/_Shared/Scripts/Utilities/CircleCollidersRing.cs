using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LightHouse.Core.Utilities
{
    [ExecuteAlways]
    public class CircularColliderRing : MonoBehaviour
    {
        [Range(3, 64)] public int segmentCount = 12;
        public float radius = 0.5f;
        public Vector3 boxSize = new Vector3(0.1f, 0.3f, 0.05f);
        public bool autoUpdate = true;
        public Vector3 positionOffset = Vector3.zero;

        public List<GameObject> generatedColliders = new List<GameObject>();

        public void GenerateColliders()
        {
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                Debug.LogWarning("Cannot generate colliders inside a prefab asset. Use prefab instance or open in prefab mode.");
                return;
            }
#endif

            ClearColliders();

            for (int i = 0; i < segmentCount; i++)
            {
                float angle = (360f / segmentCount) * i;
                Quaternion rot = Quaternion.Euler(0, angle, 0);
                Vector3 localOffset = rot * Vector3.forward * radius + positionOffset;
                Vector3 pos = transform.TransformPoint(localOffset);


                GameObject go = new GameObject($"RingCollider_{i}");
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(go, "Create Ring Collider");
#endif
                go.transform.SetParent(transform);
                go.transform.position = pos;
                go.transform.rotation = rot;

                var col = go.AddComponent<BoxCollider>();
                col.size = boxSize;

                generatedColliders.Add(go);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            foreach (var go in generatedColliders)
            {
                if (go != null)
                {
                    var col = go.GetComponent<BoxCollider>();
                    if (col == null) continue;

                    // Matrice locale → monde du collider
                    Matrix4x4 cubeMatrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, col.transform.lossyScale);
                    Gizmos.matrix = cubeMatrix;

                    // Le centre est en local (par défaut (0,0,0)) + offset éventuel du BoxCollider
                    Gizmos.DrawWireCube(col.center, col.size);
                }
            }

            // Réinitialiser la matrice Gizmo pour éviter de tout casser après
            Gizmos.matrix = Matrix4x4.identity;
        }


        public void ClearColliders()
        {
#if UNITY_EDITOR
            var collidersToRemove = new List<GameObject>(generatedColliders);

            EditorApplication.delayCall += () =>
            {
                foreach (var go in collidersToRemove)
                {
                    if (go != null)
                        Undo.DestroyObjectImmediate(go);
                }
            };
#endif

            generatedColliders.Clear();
        }


        private bool _pendingUpdate = false;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!autoUpdate || Application.isPlaying) return;

            // Ne rien faire si l'objet n'est pas dans la scène OU n'est pas dans le prefab context actif
            if (!gameObject.scene.IsValid() && !UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.IsPartOfPrefabContents(gameObject) == true)
                return;
            if (_pendingUpdate) return;
            _pendingUpdate = true;

            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                GenerateColliders();
                _pendingUpdate = false;
            };
#endif
        }

    }
}

