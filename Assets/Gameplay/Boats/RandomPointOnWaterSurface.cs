using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

namespace LightHouse.Game.WaterExtension
{
    public class RandomPointOnWaterSurface : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WaterSurface _waterSurface;
        [SerializeField] public float _radiusForPointGeneration = 100f;
        [SerializeField] public Transform _centerTransform;

        [Header("Arc Settings")]
        [Range(0f, 360f)] public float arcAngle = 180f;
        public float startingAngle = 0f;

        [Header("Path Generation")]
        [Range(2, 10)] public int numberOfPoints = 5;
        public bool useBezier = false;
        public float pathRandomness = 5f;
        public LayerMask obstacleMask;
        public float pointCheckRadius = 2f;

        [Header("Constraints")]
        [SerializeField] private float _minEntryExitDistance = 20f;

        [Header("Debug")]
        [SerializeField] private bool _enableDebug = true;

        public Vector3 _entryPoint;
        public Vector3 _exitPoint;
        private bool _hasGeneratedPoints = false;

        private Vector3 _destination;
        public Vector3 Destination => _destination;

        public List<Vector3> pathPoints = new List<Vector3>();

        private void Awake()
        {
            GenerateNewEntryExitPoints();
            GeneratePathPoints();

            transform.position = _entryPoint;
            _destination = _exitPoint;
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

        public void GeneratePathPoints()
        {
            pathPoints.Clear();

            if (useBezier)
            {
                Vector3 control = _centerTransform.position + Random.insideUnitSphere * pathRandomness;
                for (int i = 0; i <= numberOfPoints; i++)
                {
                    float t = i / (float)numberOfPoints;
                    Vector3 point = Mathf.Pow(1 - t, 2) * _entryPoint +
                                    2 * (1 - t) * t * control +
                                    Mathf.Pow(t, 2) * _exitPoint;
                    pathPoints.Add(TryRelocatePoint(point));
                }
            }
            else
            {
                for (int i = 0; i <= numberOfPoints; i++)
                {
                    float t = i / (float)numberOfPoints;
                    Vector3 linear = Vector3.Lerp(_entryPoint, _exitPoint, t);
                    Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * pathRandomness;
                    Vector3 offsetPoint = linear + randomOffset;
                    pathPoints.Add(TryRelocatePoint(offsetPoint));
                }
            }
        }

        private Vector3 TryRelocatePoint(Vector3 original)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-2f, 2f),
                    0,
                    Random.Range(-2f, 2f)
                );
                Vector3 candidate = original + offset;
                if (!IsPointObstructed(candidate))
                    return candidate;
            }
            return original;
        }

        private bool IsPointObstructed(Vector3 point)
        {
            return Physics.CheckSphere(point, pointCheckRadius, obstacleMask);
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

            if (pathPoints.Count > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < pathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
                    Gizmos.DrawSphere(pathPoints[i], 0.5f);
                }
                Gizmos.DrawSphere(pathPoints[pathPoints.Count - 1], 0.5f);
            }
        }
    }
}
