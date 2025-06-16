using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.WaterExtension
{
    public class PhysicalBuoyancy : MonoBehaviour
    {
        [Header(" --- REFERENCES --- ")]
        [SerializeField] private WaterSurface _water;
        [SerializeField] private Rigidbody _rb;

        [Header(" --- PHYSICS CONFIG --- ")]
        [SerializeField] private float _depthBeforeSubmersion = 0.5f;
        [SerializeField] private float _displacementAmount = 3f;
        [SerializeField] private float _waterDrag = 1.0f;
        [SerializeField] private float _waterAngularDrag = 1.0f;

        private WaterSearchParameters _waterSearchParams;
        private WaterSearchResult _waterSearchResult;

        private void FixedUpdate()
        {
            // Gravit�
            _rb.AddForceAtPosition(Physics.gravity / 4f, transform.position, ForceMode.Acceleration);

            // Hauteur de l'eau au point actuel
            _waterSearchParams.startPositionWS = transform.position;
            _water.ProjectPointOnWaterSurface(_waterSearchParams, out _waterSearchResult);
            float waterHeight = _waterSearchResult.projectedPositionWS.y;

            if (transform.position.y < waterHeight)
            {
                float depth = waterHeight - transform.position.y;
                float displacementMultiplier = Mathf.Clamp01(depth / _depthBeforeSubmersion) * _displacementAmount;

                // Pouss�e vers le haut (Archim�de)
                Vector3 uplift = Vector3.up * Mathf.Abs(Physics.gravity.y) * displacementMultiplier;
                _rb.AddForceAtPosition(uplift, transform.position, ForceMode.Acceleration);

                // Amortissement (drag)
                _rb.AddForce(-_rb.linearVelocity * _waterDrag * displacementMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange);
                _rb.AddTorque(-_rb.angularVelocity * _waterAngularDrag * displacementMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }
}

