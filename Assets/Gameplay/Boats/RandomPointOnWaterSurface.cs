using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.WaterExtension
{
    public class RandomPointOnWaterSurface : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WaterSurface _waterSurface;
        [SerializeField] public float _radiusForPointGeneration = 100f;
        [SerializeField] public Transform _centerTransform;

        [Header("Arc Settings")]
        [Range(0f, 360f)] public float arcAngle = 180f; // ouverture totale
        public float startingAngle = 0f; // orientation du cône (0 = vers Z+)

        [Header("Constraints")]
        [SerializeField] private float _minEntryExitDistance = 20f;

        [Header("Debug")]
        [SerializeField] private bool _enableDebug = true;

        public Vector3 _entryPoint;
        public Vector3 _exitPoint;
        private bool _hasGeneratedPoints = false;

        private Vector3 _destination;
        public Vector3 Destination => _destination;

        private void Awake()
        {
            GenerateNewEntryExitPoints();

            var (start, end) = GetEntryExitPoints();
            transform.position = start;
            _destination = end;
        }

        public void GenerateNewEntryExitPoints()
        {
            const int maxTries = 20;
            int attempt = 0;

            do
            {
                _entryPoint = GetRandomPointOnArc();
                _exitPoint = GetRandomPointOnArc();
                attempt++;
            }
            while (Vector3.Distance(_entryPoint, _exitPoint) < _minEntryExitDistance && attempt < maxTries);

            _hasGeneratedPoints = true;
        }

        private Vector3 GetRandomPointOnArc()
        {
            float halfArc = arcAngle * 0.5f;
            float randomAngle = Random.Range(-halfArc, halfArc);
            float finalAngle = startingAngle + randomAngle;

            Vector3 dir = Quaternion.Euler(0f, finalAngle, 0f) * Vector3.forward;
            return _centerTransform.position + dir * _radiusForPointGeneration;
        }

        public (Vector3 entry, Vector3 exit) GetEntryExitPoints()
        {
            if (!_hasGeneratedPoints)
                GenerateNewEntryExitPoints();

            return (_entryPoint, _exitPoint);
        }

        private void OnDrawGizmos()
        {
            if (!_enableDebug || _centerTransform == null) return;

            Vector3 center = _centerTransform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, _radiusForPointGeneration);

            Gizmos.color = Color.white;
            int segments = 64;
            float halfArc = arcAngle * 0.5f;
            float angleStep = arcAngle / segments;

            Vector3 prev = center + Quaternion.Euler(0, startingAngle - halfArc, 0) * Vector3.forward * _radiusForPointGeneration;
            for (int i = 1; i <= segments; i++)
            {
                float angle = startingAngle - halfArc + i * angleStep;
                Vector3 next = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * _radiusForPointGeneration;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_entryPoint, 1.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_exitPoint, 1.5f);
            Gizmos.DrawLine(_entryPoint, _exitPoint);
        }
    }
}
