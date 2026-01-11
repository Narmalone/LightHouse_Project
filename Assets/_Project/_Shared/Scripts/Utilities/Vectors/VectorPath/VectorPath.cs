using UnityEngine;
using System.Collections.Generic;

namespace LightHouse.Features.Boats
{
    [CreateAssetMenu(fileName = "NewVectorPath", menuName = "Utilities/Vectors/Vector Path")]
    public class VectorPath : ScriptableObject
    {
        [Header("Generated Path Data")]
        public Vector3[] Paths;

        [Header("Path Generation Config")]
        public float radius = 100f;
        public Vector3 centerPoint;
        [Range(0f, 360f)] public float arcAngle = 180f;
        public float startingAngle = 0f;
        [Range(2, 10)] public int numberOfPoints = 5;
        public bool useBezier = false;
        public float pathRandomness = 5f;
        public float minEntryExitDistance = 20f;

        [Header("Obstacle Check")]
        public LayerMask obstacleMask;
        public float pointCheckRadius = 2f;

        [HideInInspector] public Vector3 EntryPoint;
        [HideInInspector] public Vector3 ExitPoint;

        public void GenerateNewPath(Transform contextTransform = null)
        {
            if (centerPoint == Vector3.zero && contextTransform != null)
                centerPoint = contextTransform.position;

            int attempt = 0;
            const int maxTries = 20;
            do
            {
                EntryPoint = GetRandomPointOnArc();
                ExitPoint = GetRandomPointOnArc();
                attempt++;
            }
            while (Vector3.Distance(EntryPoint, ExitPoint) < minEntryExitDistance && attempt < maxTries);

            Paths = useBezier
                ? GetBezierPath(EntryPoint, ExitPoint, numberOfPoints, pathRandomness)
                : GetLinearPath(EntryPoint, ExitPoint, numberOfPoints, pathRandomness);
        }

        public Vector3 GetRandomPointOnArc()
        {
            float halfArc = arcAngle * 0.5f;
            float angle = startingAngle + Random.Range(-halfArc, halfArc);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            return centerPoint + direction * radius;
        }

        public Vector3[] GetLinearPath(Vector3 start, Vector3 end, int count, float randomness)
        {
            List<Vector3> points = new();
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                if (i == 0)
                    points.Add(start);
                else if (i == count)
                    points.Add(end);
                else
                {
                    Vector3 basePoint = Vector3.Lerp(start, end, t);
                    Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * randomness;
                    points.Add(ValidatePoint(basePoint + offset));
                }
            }
            return points.ToArray();
        }

        public Vector3[] GetBezierPath(Vector3 start, Vector3 end, int count, float randomness)
        {
            List<Vector3> points = new();
            Vector3 control = centerPoint + Random.insideUnitSphere * randomness;
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                if (i == 0)
                    points.Add(start);
                else if (i == count)
                    points.Add(end);
                else
                {
                    Vector3 point =
                        Mathf.Pow(1 - t, 2) * start +
                        2 * (1 - t) * t * control +
                        Mathf.Pow(t, 2) * end;
                    points.Add(point);
                }
            }
            return points.ToArray();
        }

        public Vector3 ValidatePoint(Vector3 point)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                Vector3 candidate = point + offset;
                if (!Physics.CheckSphere(candidate, pointCheckRadius, obstacleMask))
                    return candidate;
            }
            return point;
        }
    }

}
