using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.WaterExtension
{
    public class ModularPhysicalBuoyancy : MonoBehaviour
    {
        [Header(" --- REFERENCES --- ")]
        [SerializeField] private WaterSurface _water;
        [SerializeField] private Rigidbody _rb;

        [Header(" --- PHYSICS CONFIG --- ")]
        [SerializeField] private float depthBeforeSubmersion = 0.5f;
        [SerializeField] private float displacementAmount = 3f;
        [SerializeField] private float waterDrag = 1.0f;
        [SerializeField] private float waterAngularDrag = 1.0f;
        public int FloatersNumber = 4;

        [Header(" --- WATER SEARCH CONFIG --- ")]
        [SerializeField] private float searchError = 0.01f;
        [SerializeField, Range(1, 200)] private int maxIterations = 10;
        [SerializeField] private bool includeDeformation = true;
        [SerializeField] private bool excludeSimulation = false;
        [SerializeField] private bool outputNormal = false;

        private WaterSearchParameters _searchParams;
        private WaterSearchResult _searchResult;

        private void Start()
        {
            if (_water == null)
                _water = GameWorldHandlerData.MainOceanSurface;
        }

        private void FixedUpdate()
        {
            if (_water == null || _rb == null) return;

            // Gravit� r�partie (utile pour multi-floater)
            _rb.AddForceAtPosition(Physics.gravity / FloatersNumber, transform.position, ForceMode.Acceleration);

            // Setup water search params
            _searchParams.startPositionWS = transform.position;
            _searchParams.targetPositionWS = transform.position;
            _searchParams.error = searchError;
            _searchParams.maxIterations = maxIterations;
            _searchParams.includeDeformation = includeDeformation;
            _searchParams.excludeSimulation = excludeSimulation;
            _searchParams.outputNormal = outputNormal;

            if (_water.ProjectPointOnWaterSurface(_searchParams, out _searchResult))
            {
                float waterHeight = _searchResult.projectedPositionWS.y;
                float depth = waterHeight - transform.position.y;

                if (depth > 0f)
                {
                    float displacementMultiplier = Mathf.Clamp01(depth / depthBeforeSubmersion) * displacementAmount;

                    Vector3 uplift = Vector3.up * Mathf.Abs(Physics.gravity.y) * displacementMultiplier;
                    _rb.AddForceAtPosition(uplift, transform.position, ForceMode.Acceleration);

                    _rb.AddForce(-_rb.linearVelocity * waterDrag * displacementMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    _rb.AddTorque(-_rb.angularVelocity * waterAngularDrag * displacementMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }

                Debug.DrawLine(transform.position, _searchResult.projectedPositionWS, Color.cyan);
            }
        }
    }
}
