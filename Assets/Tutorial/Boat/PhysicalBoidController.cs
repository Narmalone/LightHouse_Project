using KinematicCharacterController;
using LightHouse.Utilities;
using System;
using UnityEngine;

/// <summary>
/// Bateau cinématique qui suit un VectorPath point par point,
/// compatible KCC via PhysicsMover.
/// Pas de AddForce, pas de rigidbody dynamique.
/// </summary>
public class BoatPathMover : MonoBehaviour, IMoverController
{
    [Header("KCC Mover")]
    [SerializeField] private PhysicsMover _mover;

    [Header("Path / Navigation")]
    [SerializeField] private VectorPath _path;
    [SerializeField] private int _currentPathIndex = 0;

    [Tooltip("Distance pour considérer un waypoint atteint et passer au suivant")]
    [SerializeField] private float _waypointReachDistance = 5f;

    [Tooltip("Vitesse linéaire (m/s) le long du path")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Rotation (Yaw uniquement)")]
    [Tooltip("Vitesse max de rotation yaw en °/s")]
    [SerializeField] private float _maxYawDegPerSec = 45f;

    [Tooltip("Deadzone d'angle yaw (°) pour éviter le jitter quand aligné")]
    [SerializeField] private float _yawDeadZoneDeg = 0.25f;

    [Header("Mer / Tangage")]
    [SerializeField] private FloaterGetterController _floater; // -> script senseur mer
    [SerializeField] private float _waterTiltLerp = 5f;    // réactivité inclinaison (pitch/roll)
    [SerializeField] private float _waterHeightLerp = 5f;  // réactivité hauteur (heave)
    [SerializeField] private float _waterHeightOffset = 0f; // offset du hull au-dessus de l'eau

    private float _pitchDeg;
    private float _rollDeg;
    private float _currentSeaHeight;

    // Progress tracking (0..1 sur toute la polyline)
    [SerializeField, Range(0f, 1f)]
    private float _progress01 = 0f;
    public float Progress => _progress01;

    private float[] _cumLengths;
    private float _totalLength;

    // état courant
    private Vector3 _currentPos;         // position simulée (ce qu'on donne à KCC)
    private Quaternion _currentRot;      // rotation simulée
    private Vector3 _velocity;           // vel debug
    private float _currentYawDeg;        // yaw accumulé
    private bool _initialized;

    private void Awake()
    {
        if (_mover == null)
            _mover = GetComponent<PhysicsMover>();

        if (_mover != null)
        {
            _mover.MoverController = this;
        }
    }

    private void Start()
    {
        InitializeOnPath();
    }

    /// <summary>
    /// Initialise la position/rotation du bateau sur le path
    /// </summary>
    private void InitializeOnPath()
    {
        if (_path == null || _path.Paths == null || _path.Paths.Length == 0)
        {
            Debug.LogWarning($"{name}: BoatPathMover - Path vide, je désactive.");
            enabled = false;
            return;
        }

        // point de départ
        Vector3 startPos = _path.Paths[0];
        _currentPathIndex = Mathf.Min(1, _path.Paths.Length - 1);

        // direction initiale
        Vector3 dir = (_path.Paths[_currentPathIndex] - startPos);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;

        Quaternion startRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        _currentPos = startPos;
        _currentRot = startRot;
        _currentYawDeg = _currentRot.eulerAngles.y;

        // init pitch/roll/hauteur mer
        _pitchDeg = 0f;
        _rollDeg = 0f;
        _currentSeaHeight = _currentPos.y;

        // pousser ça directement dans le mover pour qu'il parte synchro
        _mover.SetPositionAndRotation(_currentPos, _currentRot);

        // pré-calcul des longueurs cumulées du path (pour Progress)
        PrecomputePolylineLengths();

        _initialized = true;
    }

    /// <summary>
    /// Appelé par PhysicsMover.VelocityUpdate() chaque tick physique,
    /// pour donner la pos/rot cible (goal) ce frame.
    /// </summary>
    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        // Valeurs par défaut si pas prêt
        goalPosition = _currentPos;
        goalRotation = _currentRot;

        if (!_initialized)
            return;

        if (_path == null || _path.Paths == null || _path.Paths.Length == 0)
            return;

        // Fin du path => rester sur place
        if (_currentPathIndex >= _path.Paths.Length)
            return;

        // -------------------------------------------------
        // 1. Avancer le long du path (on modifie _currentPos.xz)
        // -------------------------------------------------
        Vector3 targetWp = _path.Paths[_currentPathIndex];
        Vector3 flatToTarget = targetWp - _currentPos;
        flatToTarget.y = 0f;

        float distToTarget = flatToTarget.magnitude;
        float stepDist = _moveSpeed * deltaTime;

        if (distToTarget <= _waypointReachDistance || stepDist >= distToTarget)
        {
            // on "atteint" ce waypoint
            _currentPos = targetWp;

            // next point
            _currentPathIndex++;

            if (_currentPathIndex >= _path.Paths.Length)
            {
                // fin du path
                UpdateProgressAlongPath();
                // goal = rester tel quel
                goalPosition = _currentPos;
                goalRotation = _currentRot;
                return;
            }
        }
        else
        {
            // déplacement partiel vers le waypoint
            Vector3 moveDir = (flatToTarget / distToTarget); // normalisé XZ
            _currentPos += moveDir * stepDist;
        }

        // -------------------------------------------------
        // 2. Mettre à jour le yaw cible (_currentYawDeg)
        // -------------------------------------------------
        Vector3 desiredDir = (_path.Paths[_currentPathIndex] - _currentPos);
        desiredDir.y = 0f;
        if (desiredDir.sqrMagnitude < 0.0001f)
        {
            // si le point est super proche, fallback sur le forward actuel
            desiredDir = _currentRot * Vector3.forward;
            desiredDir.y = 0f;
            if (desiredDir.sqrMagnitude < 0.0001f)
                desiredDir = Vector3.forward;
        }
        desiredDir.Normalize();

        float targetYawDeg = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;
        float deltaYaw = Mathf.DeltaAngle(_currentYawDeg, targetYawDeg);

        if (Mathf.Abs(deltaYaw) > _yawDeadZoneDeg)
        {
            float maxStep = _maxYawDegPerSec * deltaTime;
            float clampedStep = Mathf.Clamp(deltaYaw, -maxStep, maxStep);
            _currentYawDeg += clampedStep;
        }

        // -------------------------------------------------
        // 3. Récupérer la "mer" (pitch/roll + hauteur)
        //    et lisser ces composantes
        // -------------------------------------------------
        if (_floater != null)
        {
            // hauteur eau moyenne + offset
            float targetSeaHeight = _floater.AverageWaterHeight + _waterHeightOffset;

            _currentSeaHeight = Mathf.Lerp(
                _currentSeaHeight,
                targetSeaHeight,
                1f - Mathf.Exp(-_waterHeightLerp * deltaTime)
            );

            // normale de l'eau -> orientation locale
            Vector3 seaUp = (_floater.AverageWaterNormal.sqrMagnitude > 0.001f)
                ? _floater.AverageWaterNormal.normalized
                : Vector3.up;

            // forward "voulue" (notre cap en yaw), projeté sur le plan de la mer
            Vector3 fwd = Quaternion.Euler(0f, _currentYawDeg, 0f) * Vector3.forward;
            Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, seaUp).normalized;
            if (fwdOnPlane.sqrMagnitude < 1e-4f)
                fwdOnPlane = Vector3.forward;

            // rot alignée avec la mer
            Quaternion waterAlignRot = Quaternion.LookRotation(fwdOnPlane, seaUp);
            Vector3 waterEul = waterAlignRot.eulerAngles;

            float targetPitch = Normalize180(waterEul.x);
            float targetRoll = Normalize180(waterEul.z);

            _pitchDeg = Mathf.Lerp(
                _pitchDeg,
                targetPitch,
                1f - Mathf.Exp(-_waterTiltLerp * deltaTime)
            );
            _rollDeg = Mathf.Lerp(
                _rollDeg,
                targetRoll,
                1f - Mathf.Exp(-_waterTiltLerp * deltaTime)
            );
        }
        else
        {
            // Pas de floater => pas de tangage/roulis
            _currentSeaHeight = Mathf.Lerp(
                _currentSeaHeight,
                _currentPos.y,
                1f - Mathf.Exp(-_waterHeightLerp * deltaTime)
            );

            _pitchDeg = Mathf.Lerp(
                _pitchDeg,
                0f,
                1f - Mathf.Exp(-_waterTiltLerp * deltaTime)
            );
            _rollDeg = Mathf.Lerp(
                _rollDeg,
                0f,
                1f - Mathf.Exp(-_waterTiltLerp * deltaTime)
            );
        }

        // -------------------------------------------------
        // 4. Composer la rot finale et la position finale
        // -------------------------------------------------
        _currentRot = Quaternion.Euler(_pitchDeg, _currentYawDeg, _rollDeg);
        _currentPos.y = _currentSeaHeight;

        // -------------------------------------------------
        // 5. Progress & debug vel
        // -------------------------------------------------
        _velocity = (_path.Paths[_currentPathIndex] - _currentPos).normalized * _moveSpeed;
        UpdateProgressAlongPath();

        // -------------------------------------------------
        // 6. Donner la sortie à PhysicsMover
        // -------------------------------------------------
        goalPosition = _currentPos;
        goalRotation = _currentRot;
    }

    private static float Normalize180(float deg)
    {
        deg %= 360f;
        if (deg > 180f) deg -= 360f;
        return deg;
    }

    // === Gestion du pourcentage d'avancement le long de la polyline ===
    private void PrecomputePolylineLengths()
    {
        var pts = _path.Paths;
        int n = pts.Length;
        _cumLengths = new float[n];
        _cumLengths[0] = 0f;

        float acc = 0f;
        for (int i = 1; i < n; i++)
        {
            Vector3 a = pts[i - 1]; a.y = 0f;
            Vector3 b = pts[i]; b.y = 0f;
            float segLen = Vector3.Distance(a, b);
            acc += segLen;
            _cumLengths[i] = acc;
        }
        _totalLength = Mathf.Max(0.0001f, acc);
    }

    private void UpdateProgressAlongPath()
    {
        if (_cumLengths == null || _cumLengths.Length < 2 || _currentPathIndex <= 0)
        {
            _progress01 = 0f;
            return;
        }

        int i = Mathf.Clamp(_currentPathIndex, 1, _path.Paths.Length - 1);

        Vector3 a = _path.Paths[i - 1]; a.y = 0f;
        Vector3 b = _path.Paths[i]; b.y = 0f;
        Vector3 p = _currentPos; p.y = 0f;

        Vector3 ab = b - a;
        float abLen = ab.magnitude;
        if (abLen < 1e-4f)
        {
            _progress01 = _cumLengths[i] / _totalLength;
            return;
        }

        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab / abLen) / abLen);
        float along = _cumLengths[i - 1] + t * abLen;

        _progress01 = Mathf.Clamp01(along / _totalLength);
    }

    // debug gizmos
    private void OnDrawGizmosSelected()
    {
        if (_path != null && _path.Paths != null && _path.Paths.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _path.Paths.Length; i++)
            {
                Gizmos.DrawSphere(_path.Paths[i], 0.3f);
                if (i < _path.Paths.Length - 1)
                    Gizmos.DrawLine(_path.Paths[i], _path.Paths[i + 1]);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_currentPos, 0.4f);
            Gizmos.DrawRay(_currentPos, (_currentRot * Vector3.forward) * 2f);
        }
    }
}
