using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Core.Utilities
{
    public class FloaterGetterController : MonoBehaviour
    {
        [Header("Water")]
        [SerializeField] private WaterSurface _waterSurface;

        [Header("Floaters")]
        [SerializeField] private Transform[] _floaters;

        [Header("Physics")]
        [SerializeField] private Rigidbody _rb;

        [Header("Search Settings")]
        [SerializeField] private float _searchError = 0.01f;

        [Range(1, 200)] public int _maxIterations = 10;
        [SerializeField] private bool _includeDeformation = true;
        [SerializeField] private bool _excludeSimulation = false;

        public Vector3 AverageWaterNormal { get; private set; }
        public float AverageWaterHeight { get; private set; }

        private void Start()
        {
            if (_waterSurface == null)
            {
                _waterSurface = FindFirstObjectByType<WaterSurface>();
            }
        }

        void FixedUpdate()
        {
            if (_rb == null || _waterSurface == null || _floaters == null || _floaters.Length == 0) return;

            Vector3 normalAccum = Vector3.zero;
            float heightAccum = 0f;
            int samples = 0;

            foreach (Transform floater in _floaters)
            {
                WaterSearchParameters param = new WaterSearchParameters
                {
                    startPositionWS = floater.position,
                    targetPositionWS = floater.position,
                    error = _searchError,
                    maxIterations = _maxIterations,
                    includeDeformation = _includeDeformation,
                    excludeSimulation = _excludeSimulation,
                    outputNormal = true  // <- IMPORTANT pour récupérer la normale
                };

                if (_waterSurface.ProjectPointOnWaterSurface(param, out WaterSearchResult result))
                {
                    heightAccum += result.projectedPositionWS.y;
                    normalAccum += (Vector3)result.normalWS; // ou result.projectedNormalWS selon la version HDRP
                    samples++;
                }
            }

            if (samples > 0)
            {
                AverageWaterHeight = heightAccum / samples;
                AverageWaterNormal = normalAccum.normalized;
            }
            else
            {
                AverageWaterHeight = transform.position.y;
                AverageWaterNormal = Vector3.up;
            }

            // IMPORTANT : on ne fait PLUS AddForceAtPosition ici,
            // puisque le rigidbody est kinematic maintenant.
            // On laisse juste ce script servir de "senseur d'océan".
        }
    }
}

