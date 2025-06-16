using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.WaterExtension
{
    public class RandomPointOnWaterSurface : MonoBehaviour
    {
        [SerializeField] private WaterSurface _waterSurface;
        [SerializeField] private float _radiusForPointGeneration = 100f;
        [SerializeField] private Transform _centerTransform;
        [SerializeField] private bool _enableDebug = true;

        private Vector3 _entryPoint;
        private Vector3 _exitPoint;
        private bool _hasGeneratedPoints = false;

        private Vector3 _destination;
        public Vector3 Destination => _destination;

        private void Awake()
        {
            // Génère une paire au lancement
            GenerateNewEntryExitPoints();

            var (start, end) = GetEntryExitPoints();
            transform.position = start;
            _destination = end;
        }


        public Vector3 GetRandomPointInsideCircle()
        {
            Vector2 random2D = Random.insideUnitCircle * _radiusForPointGeneration;
            Vector3 point = _centerTransform.position + new Vector3(random2D.x, 0, random2D.y);
            return point;
        }

        public void GenerateNewEntryExitPoints()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            _entryPoint = _centerTransform.position + dir * _radiusForPointGeneration;
            _exitPoint = _centerTransform.position - dir * _radiusForPointGeneration;

            _hasGeneratedPoints = true;
        }

        public (Vector3 entry, Vector3 exit) GetEntryExitPoints()
        {
            if (!_hasGeneratedPoints)
            {
                GenerateNewEntryExitPoints();
            }

            return (_entryPoint, _exitPoint);
        }

        private void OnDrawGizmos()
        {
            if (!_enableDebug || _centerTransform == null || !_hasGeneratedPoints) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_centerTransform.position, _radiusForPointGeneration);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_entryPoint, 2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_exitPoint, 2f);
            Gizmos.DrawLine(_entryPoint, _exitPoint);
        }
    }

}
