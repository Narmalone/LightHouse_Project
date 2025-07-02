using LightHouse.Game.Computer.NightWatch.Sonar;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Gameplay.Boats
{
    public class BoatPhysicsFloatController : MonoBehaviour
    {
        [Header("Water")]
        [SerializeField] private WaterSurface _waterSurface;

        [Header("Floaters")]
        [SerializeField] private Transform[] _floaters;

        [Header("Physics")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _depthBeforeSubmersion = 0.5f;
        [SerializeField] private float _displacementAmount = 3f;
        [SerializeField] private float _waterDrag = 1f;
        [SerializeField] private float _waterAngularDrag = 1f;

        [Header("Search Settings")]
        [SerializeField] private float _searchError = 0.01f;

        [Range(1, 200)] public int _maxIterations = 10;
        [SerializeField] private bool _includeDeformation = true;
        [SerializeField] private bool _excludeSimulation = false;
        [SerializeField] private bool _outputNormal = false;

        private void FixedUpdate()
        {
            if (_rb == null || _waterSurface == null || _floaters == null || _floaters.Length == 0) return;

            // Répartir la gravité (important !)
            foreach (Transform floater in _floaters)
            {
                _rb.AddForceAtPosition(Physics.gravity / _floaters.Length, floater.position, ForceMode.Acceleration);

                WaterSearchParameters param = new WaterSearchParameters
                {
                    startPositionWS = floater.position,
                    targetPositionWS = floater.position,
                    error = _searchError,
                    maxIterations = _maxIterations,
                    includeDeformation = _includeDeformation,
                    excludeSimulation = _excludeSimulation,
                    outputNormal = _outputNormal
                };

                if (_waterSurface.ProjectPointOnWaterSurface(param, out WaterSearchResult result))
                {
                    float waterHeight = result.projectedPositionWS.y;
                    float depth = waterHeight - floater.position.y;

                    if (depth > 0f)
                    {
                        float displacementMultiplier = Mathf.Clamp01(depth / _depthBeforeSubmersion) * _displacementAmount;
                        Vector3 uplift = Vector3.up * Mathf.Abs(Physics.gravity.y) * displacementMultiplier;

                        _rb.AddForceAtPosition(uplift, floater.position, ForceMode.Acceleration);

                        _rb.AddForce(-_rb.linearVelocity * _waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                        _rb.AddTorque(-_rb.angularVelocity * _waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);

                        Debug.DrawLine(floater.position, floater.position + uplift * 0.1f, Color.cyan);
                    }
                }
            }
        }
    }
}

