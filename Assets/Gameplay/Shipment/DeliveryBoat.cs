using LightHouse.Features.Boats;
using LightHouse.Features.Computer.LEO.Supplies;
using LightHouse.Features.Sonar.Core;
using LightHouse.Features.TimeOfDay.TimeCore;
using System;
using System.Collections;
using UnityEngine;

namespace LightHouse.Features.Shipment.Delivery
{
    public class DeliveryBoat : MonoBehaviour, ISonarable
    {
        // === ISonarable ===
        public string Name => "";
        public int UniqueID { get; set; }
        public bool IsDetectedBySonar { get; set; }
        public Vector3 Position => transform.position;
        public Vector3 RotationAngles => transform.eulerAngles;
        public Sprite DotSprite { get; set; }
        public Color DotColor { get; set; }
        public Vector2 DotSize { get; set; }
        public string SonarInfo { get; set; }
        public Action ForceDotUpdate { get; set; }

        // Path
        private VectorPath _path;
        private float[] _segLen;     // longueurs par segment (XZ)
        private float[] _cumLen;     // cumulées
        private float _totalLen;

        [Header(" --- COMPONENTS --- ")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private GameObject[] _buoyancys;

        [Header(" --- TIMING (DRIVEN BY GAME CLOCK) --- ")]
        [SerializeField] private AnimationCurve _ease = AnimationCurve.Linear(0, 0, 1, 1);

        // horodatages in-game absolus (heures de jeu depuis J0 00:00)
        private float _absStartGameHours;   // au spawn du bateau
        private float _absEndGameHours;     // dispatch (jour*24 + heure)

        [Header(" --- ROTATION (Yaw only) --- ")]
        [SerializeField, Tooltip("Distance en mètres sur le chemin pour regarder en avance")]
        private float _lookAheadMeters = 8f;

        [SerializeField, Tooltip("Temps de lissage de l'orientation (s)")]
        private float _yawSmoothTime = 0.35f;

        [SerializeField, Tooltip("Vitesse angulaire maximale (deg/s)")]
        private float _maxYawDegPerSec = 120f;

        private float _yawVelDeg;
        private bool _running;

        private Vector3 _deliverySpawnPoint = Vector3.zero;

        public event Action<DeliveryBoat> OnPathCompleted;

        private void Awake() => SonarHandlerData.Register(this);
        private void OnDestroy() => SonarHandlerData.Unregister(this);

        private IEnumerator EnablePhysics()
        {
            yield return new WaitForFixedUpdate();
            _rb.isKinematic = false;
            foreach (var b in _buoyancys) b.SetActive(true);
        }

        private void FixedUpdate()
        {
            if (!_running || _path?.Paths == null || _path.Paths.Length < 2) return;

            // 1) lire l’heure in-game absolue (jour*24 + heure)
            float absNow = TimeHandlerData.CurrentDay * 24f + TimeHandlerData.CurrentTime;

            // 2) progression calée sur l’horloge du jeu
            float t = Mathf.InverseLerp(_absStartGameHours, _absEndGameHours, absNow);
            t = Mathf.Clamp01(_ease.Evaluate(t));

            // 3) position le long du chemin (arclength) + look-ahead pour la direction
            float sNow = t * _totalLen;
            float sAhead = Mathf.Min(_totalLen, sNow + Mathf.Max(0.1f, _lookAheadMeters));

            Vector3 pNowXZ = SamplePathXZAtArcLength(sNow);
            Vector3 pAheadXZ = SamplePathXZAtArcLength(sAhead);

            // déplacer en XZ (laisser la houle/Y au Rigidbody + floaters)
            Vector3 newPos = new Vector3(pNowXZ.x, _rb.position.y, pNowXZ.z);
            _rb.MovePosition(newPos);

            // 4) yaw smooth vers la tangente du chemin
            Vector3 dir = pAheadXZ - pNowXZ; dir.y = 0f;
            if (dir.sqrMagnitude > 1e-6f)
            {
                float currYaw = _rb.rotation.eulerAngles.y;
                float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

                float smoothedYaw = Mathf.SmoothDampAngle(
                    current: currYaw,
                    target: targetYaw,
                    currentVelocity: ref _yawVelDeg,
                    smoothTime: _yawSmoothTime,
                    maxSpeed: _maxYawDegPerSec,
                    deltaTime: Time.fixedDeltaTime
                );

                var e = _rb.rotation.eulerAngles; // préserve roll/pitch (houle)
                _rb.MoveRotation(Quaternion.Euler(e.x, smoothedYaw, e.z));
            }

            // 5) fin : si l’horloge a dépassé la dispatch → compléter
            if (absNow >= _absEndGameHours - 1e-4f)
            {
                _running = false;
                OnPathComplete();
            }
        }

        private ShipmentSystem _shipment;

        /// <summary>
        /// Initialise le bateau pour un trajet calé entre l’heure de spawn (NOW) et la dispatch (shipment.DispatchDay/Hour).
        /// Peu importe la vitesse du temps de jeu : la progression suit l’horloge.
        /// </summary>
        public void Initialize(float gameHoursToFill, VectorPath path, ShipmentSystem shipment, Vector3 spawnPoint)
        {
            _path = path;
            _shipment = shipment;
            _deliverySpawnPoint = spawnPoint;

            foreach (var b in _buoyancys) b.SetActive(false);
            _rb.isKinematic = true;

            // points du chemin (au moins 2)
            var pts = (_path.Paths != null && _path.Paths.Length >= 2)
                ? _path.Paths
                : new[] { _path.EntryPoint, _path.ExitPoint };

            // spawn exactement au point 0 (on garde Y actuel pour la houle)
            Vector3 p0 = pts[0]; p0.y = _rb.position.y;
            _rb.position = p0;

            // orientation initiale (vers point 1)
            Vector3 dir0 = pts[1] - p0; dir0.y = 0f;
            if (dir0.sqrMagnitude > 1e-6f)
                _rb.rotation = Quaternion.LookRotation(dir0.normalized, Vector3.up);

            // longueurs (XZ)
            PrecomputeLengthsXZ(pts, out _segLen, out _cumLen, out _totalLen);

            // horodatages in-game
            _absStartGameHours = TimeHandlerData.CurrentDay * 24f + TimeHandlerData.CurrentTime;

            // on s’appuie sur les valeurs fixées par ShipmentSystem (il les règle quand il passe en WaitingDispatchWindow)
            float dispatchDay = shipment.DispatchDay;      // byte → float OK
            float dispatchHour = shipment.DispatchHour;    // ex: 9f
            _absEndGameHours = dispatchDay * 24f + dispatchHour;

            // garde-fous : si la fin est avant le début (cas rare), pousse à +24h
            if (_absEndGameHours <= _absStartGameHours)
                _absEndGameHours += 24f;

            _running = true;
            StartCoroutine(EnablePhysics());
        }

        // === géométrie / arclength ===
        private void PrecomputeLengthsXZ(Vector3[] pts, out float[] seg, out float[] cum, out float total)
        {
            int n = pts.Length;
            seg = new float[n - 1];
            cum = new float[n - 1];
            total = 0f;
            for (int i = 0; i < n - 1; i++)
            {
                Vector3 a = pts[i]; a.y = 0f;
                Vector3 b = pts[i + 1]; b.y = 0f;
                float d = Vector3.Distance(a, b);
                seg[i] = d;
                total += d;
                cum[i] = total;
            }
        }

        private Vector3 SamplePathXZAtArcLength(float s)
        {
            s = Mathf.Clamp(s, 0f, _totalLen);
            var pts = _path.Paths;
            if (s <= 0f) { var v = pts[0]; v.y = 0f; return v; }
            if (s >= _totalLen) { var v = pts[^1]; v.y = 0f; return v; }

            int segIdx = 0; float prevCum = 0f;
            for (; segIdx < _cumLen.Length; segIdx++)
            {
                float cum = _cumLen[segIdx];
                if (s <= cum) break;
                prevCum = cum;
            }

            Vector3 a = pts[segIdx]; a.y = 0f;
            Vector3 b = pts[segIdx + 1]; b.y = 0f;
            float segLen = _segLen[segIdx];
            float t = (segLen <= 1e-6f) ? 0f : (s - prevCum) / segLen;
            return Vector3.Lerp(a, b, t);
        }

        private void OnPathComplete()
        {
            Debug.Log($"{name} path complete (game-timed).");
            OnPathCompleted?.Invoke(this);

            // Spawn des items livrés (comme avant)
            foreach (var item in _shipment.SupplyOrderLines)
                for (int i = 0; i < item.Quantity; i++)
                    Instantiate(item.Prefab, _deliverySpawnPoint, Quaternion.identity);
            StartCoroutine(DespawnTempRoutine(15f));

        }

        private IEnumerator DespawnTempRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(this.gameObject);
        }
    }
}