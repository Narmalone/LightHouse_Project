using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

namespace LightHouse.Game.WaterExtension
{
    public class RandomPointOnWaterSurface : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] public float radius = 100f;
        public Vector3 CenterPoint;

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
        [SerializeField] private float minEntryExitDistance = 20f;

        [Header("Debug")]
        [SerializeField] private bool enableDebug = true;

        public Vector3 EntryPoint { get; private set; }
        public Vector3 ExitPoint { get; private set; }
        public List<Vector3> PathPoints { get; private set; } = new();

        private void Awake()
        {
            CenterPoint = GameWorldHandlerData.IslandCenterPoint.position;
            transform.position = CenterPoint;
            GenerateNewEntryExitPoints();
            PathPoints = GeneratePath(EntryPoint, ExitPoint);
            transform.position = EntryPoint;
        }

        #region Public Interface

        public void GenerateNewEntryExitPoints()
        {
            const int maxTries = 20;
            int attempt = 0;
            do
            {
                EntryPoint = GetRandomPointOnArc();
                ExitPoint = GetRandomPointOnArc();
                attempt++;
            }
            while (Vector3.Distance(EntryPoint, ExitPoint) < minEntryExitDistance && attempt < maxTries);
        }

        public (Vector3 entry, Vector3 exit) GetEntryExitPoints()
        {
            return (EntryPoint, ExitPoint);
        }

        public List<Vector3> GeneratePath(Vector3 start, Vector3 end)
        {
            return useBezier
                ? GetBezierPath(start, end, numberOfPoints, pathRandomness)
                : GetLinearPath(start, end, numberOfPoints, pathRandomness);
        }

        public Vector3 GetRandomPointOnArc()
        {
            float halfArc = arcAngle * 0.5f;
            float angle = startingAngle + Random.Range(-halfArc, halfArc);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            return CenterPoint + direction * radius;
        }

        #endregion

        #region Path Algorithms

        private List<Vector3> GetLinearPath(Vector3 start, Vector3 end, int count, float randomness)
        {
            var points = new List<Vector3>();
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                Vector3 basePoint = Vector3.Lerp(start, end, t);
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * randomness;
                points.Add(ValidatePoint(basePoint + offset));
            }
            return points;
        }

        private List<Vector3> GetBezierPath(Vector3 start, Vector3 end, int count, float randomness)
        {
            var points = new List<Vector3>();
            Vector3 control = CenterPoint + Random.insideUnitSphere * randomness;
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                Vector3 point =
                    Mathf.Pow(1 - t, 2) * start +
                    2 * (1 - t) * t * control +
                    Mathf.Pow(t, 2) * end;
                points.Add(ValidatePoint(point));
            }
            return points;
        }

        #endregion

        #region Validation & Utilities

        public Vector3 ValidatePoint(Vector3 point)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-2f, 2f),
                    0,
                    Random.Range(-2f, 2f)
                );
                Vector3 candidate = point + offset;
                if (!Physics.CheckSphere(candidate, pointCheckRadius, obstacleMask))
                    return candidate;
            }
            return point; // fallback
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!enableDebug || CenterPoint == null) return;

            Vector3 centerPos = CenterPoint;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centerPos, radius);

            // Arc
            Gizmos.color = Color.white;
            int segments = 64;
            float halfArc = arcAngle * 0.5f;
            float angleStep = arcAngle / segments;

            Vector3 prev = centerPos + Quaternion.Euler(0, startingAngle - halfArc, 0) * Vector3.forward * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = startingAngle - halfArc + i * angleStep;
                Vector3 next = centerPos + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }

            // Entry / Exit
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(EntryPoint, 1.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(ExitPoint, 1.5f);
            Gizmos.DrawLine(EntryPoint, ExitPoint);

            // Path
            if (PathPoints.Count > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < PathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(PathPoints[i], PathPoints[i + 1]);
                    Gizmos.DrawSphere(PathPoints[i], 0.5f);
                }
                Gizmos.DrawSphere(PathPoints[^1], 0.5f);
            }
        }

        #endregion

        /// <summary>
        /// Calcule la progression du bateau entre EntryPoint et ExitPoint (0 = début, 1 = arrivé).
        /// </summary>
        public float GetProgress01(Vector3 currentPosition)
        {
            Vector3 path = ExitPoint - EntryPoint;
            Vector3 toCurrent = currentPosition - EntryPoint;

            if (path.sqrMagnitude < 0.01f)
                return 0f;

            float t = Vector3.Dot(toCurrent, path.normalized) / path.magnitude;
            return Mathf.Clamp01(t);
        }


        private Vector3 ProjectPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
            return a + Mathf.Clamp01(t) * ab;
        }

        private bool IsPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            float d1 = Vector3.Distance(a, p);
            float d2 = Vector3.Distance(p, b);
            float dTotal = Vector3.Distance(a, b);
            return Mathf.Abs((d1 + d2) - dTotal) < 0.1f;
        }

    }
}
