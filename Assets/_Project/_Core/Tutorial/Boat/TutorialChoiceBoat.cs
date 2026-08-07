using KinematicCharacterController;
using LightHouse.Core.Player;
using LightHouse.Core.Utilities;
using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace LightHouse.Features.Boats
{
    public enum BoatDirection
    {
        Left,
        Midle,
        Right
    }

    /// <summary>
    /// Bateau tutoriel à choix multiples et successifs :
    /// 1) suit un tronc commun (_commonWaypoints)
    /// 2) s'arrête et attend le choix du joueur pour l'étape 0
    /// 3) suit la branche correspondant à la direction choisie
    /// 4) s'arrête et attend le choix du joueur pour l'étape 1 (si configurée)
    /// 5) ... autant d'étapes que défini dans _directionPaths
    /// 6) une fois toutes les étapes de choix passées (ou s'il n'y en a aucune),
    ///    suit le tronc final commun (_finalWaypoints) jusqu'à destination.
    /// Le bateau continue de tanguer avec la mer pendant les arrêts.
    /// </summary>
    public class TutorialChoiceBoat : MonoBehaviour, IMoverController
    {
        #region Events

        /// <summary>Levé quand le bateau s'arrête et attend une décision du joueur.
        /// Utiliser CurrentChoiceStepIndex pour savoir à quelle étape on en est.</summary>
        public event Action OnChoiceRequired;

        /// <summary>Levé quand le joueur a choisi une direction pour l'étape courante.</summary>
        public event Action<BoatDirection> OnDirectionChosen;

        /// <summary>Levé quand le bateau a atteint la destination finale.</summary>
        public event Action OnPathCompleted;

        /// <summary>Levé quand le bateau a terminé les directions à choix et entame le chemin final.</summary>
        public event Action OnFinalPath;

        #endregion

        #region Serialized Fields

        [Header("KCC Mover")]
        [SerializeField] private PhysicsMover _mover;

        [Header("Tronc commun (avant le premier choix)")]
        [SerializeField] private Vector3[] _commonWaypoints;

        [Header("Étapes de choix : index d'étape -> direction -> waypoints")]
        [SerializeField] private SerializedDictionary<int, SerializedDictionary<BoatDirection, Vector3[]>> _directionPaths = new();

        [Header("Tronc final commun (après le dernier choix, vers la destination)")]
        [SerializeField] private Vector3[] _finalWaypoints;

        [Tooltip("Distance pour considérer un waypoint atteint")]
        [SerializeField] private float _waypointReachDistance = 5f;

        [Tooltip("Vitesse linéaire (m/s) le long du path, valeur de référence/reset")]
        [SerializeField] private float _baseMoveSpeed = 5f;

        [Tooltip("Vitesse linéaire (m/s) actuelle le long du path")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Rotation (Yaw uniquement)")]
        [SerializeField] private float _maxYawDegPerSec = 45f;
        [SerializeField] private float _yawDeadZoneDeg = 0.25f;

        [Header("Mer / Tangage")]
        [SerializeField] private FloaterGetterController _floater;
        [SerializeField] private float _waterTiltLerp = 5f;
        [SerializeField] private float _waterHeightLerp = 5f;
        [SerializeField] private float _waterHeightOffset = 0f;

        [Header("Tutoriel Joueur")]
        [SerializeField] private Transform _playerSpawnTutorial;

        #endregion

        #region Public API / Properties

        /// <summary>Vitesse courante le long du path (m/s), modifiable à la volée.</summary>
        public float Speed { get => _moveSpeed; set => _moveSpeed = value; }

        /// <summary>Vitesse de référence configurée dans l'inspecteur.</summary>
        public float BaseMoveSpeed => _baseMoveSpeed;

        /// <summary>Vélocité courante (direction + norme = Speed) ; nulle en pause ou en attente de choix.</summary>
        public Vector3 Velocity => _velocity;

        public bool IsPaused { get; private set; }
        public bool IsWaitingForChoice { get; private set; }
        public bool IsPathCompleted { get; private set; }

        /// <summary>Dernière direction choisie par le joueur (toutes étapes confondues).</summary>
        public BoatDirection? ChosenDirection { get; private set; }

        /// <summary>Index de l'étape de choix en cours (ou en attente).</summary>
        public int CurrentChoiceStepIndex => _currentChoiceStepIndex;

        #endregion

        #region Private State

        private enum BoatSegment
        {
            CommonTrunk,
            Branch,
            Final
        }

        private BoatSegment _currentSegment;
        private int _currentChoiceStepIndex;

        /// <summary>
        /// Empêche de re-déclencher la transition de fin de segment (choix suivant /
        /// passage au segment final / complétion) plusieurs fois tant qu'on reste
        /// à l'arrêt en attente d'une décision.
        /// </summary>
        private bool _segmentEndHandled;

        private Vector3[] _currentWaypoints;
        private int _currentIndex;

        private float _pitchDeg;
        private float _rollDeg;
        private float _currentSeaHeight;

        private Vector3 _currentPos;
        private Quaternion _currentRot;
        private Vector3 _velocity;
        private float _currentYawDeg;
        private bool _initialized;
        private bool _pathCompletedInvoked;

        // Dernier état 100% valide connu : sert de filet de sécurité si un NaN/Infinity
        // apparaît (ex: floater qui renvoie une hauteur d'eau invalide un instant).
        private Vector3 _lastValidPos;
        private Quaternion _lastValidRot;
        private float _lastValidSeaHeight;
        private bool _corruptionWarningLogged;

        #endregion

        #region Spawn Joueur

        public void SpawnPlayerOnBoatPos()
        {
            if (PlayerHandlerData.MainPlayer != null)
            {
                PlayerHandlerData.MainPlayer.Inventory.Disable();
                PlayerHandlerData.MainPlayer.Interactions.Disable();
                PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = false;
                PlayerHandlerData.MainPlayer.EnableCameraRotationInput = false;
            }

            PlayerHandlerData.MainPlayer.Character.ForceCutVelocity();
            PlayerHandlerData.MainPlayer.Character.ForceLookRotation(_playerSpawnTutorial.rotation);

            PlayerHandlerData.MainPlayer.Character.SetPositionAndRotation(
                _playerSpawnTutorial.position,
                _playerSpawnTutorial.rotation,
                true);

            PlayerHandlerData.MainPlayer.PlayerCamera.SetRotation(
                _playerSpawnTutorial.rotation);
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_mover == null)
                _mover = GetComponent<PhysicsMover>();

            if (_mover != null)
                _mover.MoverController = this;
        }

        #endregion

        #region Initialization

        public void InitializeOnPath()
        {
            if (_commonWaypoints == null || _commonWaypoints.Length == 0)
            {
                Debug.LogWarning($"{name}: TutorialChoiceBoat - tronc commun vide, désactivation.");
                enabled = false;
                return;
            }

            _moveSpeed = _baseMoveSpeed;

            _currentSegment = BoatSegment.CommonTrunk;
            _currentChoiceStepIndex = 0;
            _segmentEndHandled = false;
            ChosenDirection = null;

            _currentWaypoints = _commonWaypoints;
            _currentIndex = Mathf.Min(1, _currentWaypoints.Length - 1);

            Vector3 startPos = _currentWaypoints[0];
            Vector3 dir = _currentWaypoints[_currentIndex] - startPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                dir = transform.forward;

            _currentPos = startPos;
            _currentRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            _currentYawDeg = _currentRot.eulerAngles.y;
            _pitchDeg = 0f;
            _rollDeg = 0f;
            _currentSeaHeight = _currentPos.y;
            _velocity = Vector3.zero;

            // Le point de départ sert de première référence "valide".
            _lastValidPos = _currentPos;
            _lastValidRot = _currentRot;
            _lastValidSeaHeight = _currentSeaHeight;
            _corruptionWarningLogged = false;

            _mover.SetPositionAndRotation(_currentPos, _currentRot);

            _initialized = true;
            IsPathCompleted = false;
            IsPaused = false;
            IsWaitingForChoice = false;
            _pathCompletedInvoked = false;

            SpawnPlayerOnBoatPos();
        }

        #endregion

        #region Choix du joueur

        /// <summary>
        /// À appeler depuis l'UI ou l'input du tutoriel quand le joueur choisit une direction
        /// pour l'étape de choix en cours (CurrentChoiceStepIndex).
        /// Ne fait rien si aucun choix n'est actuellement attendu.
        /// </summary>
        public void ChooseDirection(BoatDirection direction)
        {
            if (!IsWaitingForChoice)
            {
                Debug.LogWarning($"{name}: ChooseDirection appelé alors qu'aucun choix n'est attendu.");
                return;
            }

            if (!TryGetBranchWaypoints(_currentChoiceStepIndex, direction, out var waypoints))
            {
                Debug.LogError($"{name}: aucune branche configurée pour l'étape {_currentChoiceStepIndex} / direction {direction}.");
                return;
            }

            ChosenDirection = direction;

            _currentSegment = BoatSegment.Branch;
            _currentWaypoints = waypoints;
            _currentIndex = 0;
            _segmentEndHandled = false;

            IsWaitingForChoice = false;
            IsPaused = false;

            OnDirectionChosen?.Invoke(direction);
        }

        private bool TryGetBranchWaypoints(int stepIndex, BoatDirection direction, out Vector3[] waypoints)
        {
            waypoints = null;

            if (_directionPaths == null || !_directionPaths.TryGetValue(stepIndex, out var stepOptions))
                return false;

            if (stepOptions == null || !stepOptions.TryGetValue(direction, out waypoints))
                return false;

            return waypoints != null && waypoints.Length > 0;
        }

        private bool HasChoiceStep(int stepIndex)
        {
            return _directionPaths != null
                && _directionPaths.TryGetValue(stepIndex, out var stepOptions)
                && stepOptions != null
                && stepOptions.Count > 0;
        }

        #endregion

        #region Pause / Resume

        public void Pause() => IsPaused = true;

        /// <summary>
        /// Reprend le mouvement. Sans effet tant qu'un choix de direction est attendu :
        /// utiliser ChooseDirection pour repartir dans ce cas.
        /// </summary>
        public void Resume()
        {
            if (IsWaitingForChoice) return;
            IsPaused = false;
        }

        #endregion

        #region KCC Mover Controller

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = _currentPos;
            goalRotation = _currentRot;

            if (!_initialized || _currentWaypoints == null || _currentWaypoints.Length == 0)
                return;

            // Chemin terminé : plus d'avancée, mais le bateau continue de tanguer.
            if (IsPathCompleted)
            {
                _velocity = Vector3.zero;
                ApplySeaEffects(deltaTime);
                FinalizeTransform();
                goalPosition = _currentPos;
                goalRotation = _currentRot;
                return;
            }

            // Fin du tableau de waypoints courant atteinte.
            if (_currentIndex >= _currentWaypoints.Length)
            {
                if (!_segmentEndHandled)
                {
                    _segmentEndHandled = true;
                    HandleSegmentEndReached();
                }

                _velocity = Vector3.zero;
                ApplySeaEffects(deltaTime);
                FinalizeTransform();
                goalPosition = _currentPos;
                goalRotation = _currentRot;
                return;
            }

            if (!IsPaused)
                AdvanceAlongPath(deltaTime);
            else
                _velocity = Vector3.zero;

            ApplySeaEffects(deltaTime);
            FinalizeTransform();

            goalPosition = _currentPos;
            goalRotation = _currentRot;
        }

        /// <summary>
        /// Décide de la suite quand un segment (tronc commun ou branche) se termine :
        /// demande le choix suivant s'il existe une étape configurée pour cet index,
        /// sinon bascule sur le tronc final commun.
        /// </summary>
        private void HandleSegmentEndReached()
        {
            switch (_currentSegment)
            {
                case BoatSegment.CommonTrunk:
                    RequestChoiceOrMoveToFinal(stepIndex: 0);
                    break;

                case BoatSegment.Branch:
                    RequestChoiceOrMoveToFinal(stepIndex: _currentChoiceStepIndex + 1);
                    break;

                case BoatSegment.Final:
                    CompletePathNow();
                    break;
            }
        }

        private void RequestChoiceOrMoveToFinal(int stepIndex)
        {
            _currentChoiceStepIndex = stepIndex;

            if (HasChoiceStep(stepIndex))
            {
                IsWaitingForChoice = true;
                IsPaused = true;
                OnChoiceRequired?.Invoke();
            }
            else
            {
                MoveToFinalSegment();
            }
        }

        private void MoveToFinalSegment()
        {
            _currentSegment = BoatSegment.Final;
            _currentIndex = 0;
            _segmentEndHandled = false;

            if (_finalWaypoints == null || _finalWaypoints.Length == 0)
            {
                // Rien à parcourir après le dernier choix : destination atteinte directement.
                CompletePathNow();
                return;
            }

            _currentWaypoints = _finalWaypoints;
            OnFinalPath?.Invoke();
        }

        private void AdvanceAlongPath(float deltaTime)
        {
            Vector3 targetWp = _currentWaypoints[_currentIndex];
            Vector3 flatToTarget = targetWp - _currentPos;
            flatToTarget.y = 0f;

            float distToTarget = flatToTarget.magnitude;
            float stepDist = _moveSpeed * deltaTime;

            if (distToTarget <= _waypointReachDistance || stepDist >= distToTarget)
            {
                _currentPos = targetWp;
                _currentIndex++;
                if (_currentIndex >= _currentWaypoints.Length)
                {
                    _velocity = Vector3.zero;
                    return; // Sera géré au prochain appel : choix suivant, final, ou complétion.
                }
            }
            else
            {
                Vector3 moveDir = flatToTarget.normalized;
                _currentPos += moveDir * stepDist;
            }

            int lookAtIndex = Mathf.Min(_currentIndex, _currentWaypoints.Length - 1);
            Vector3 desiredDir = _currentWaypoints[lookAtIndex] - _currentPos;
            desiredDir.y = 0f;

            if (desiredDir.sqrMagnitude < 0.0001f)
            {
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

            // Vélocité courante : direction vers le prochain waypoint, norme = vitesse configurée.
            _velocity = desiredDir * _moveSpeed;
        }

        private void ApplySeaEffects(float deltaTime)
        {
            if (_floater != null)
            {
                float waterHeight = _floater.AverageWaterHeight;
                Vector3 waterNormal = _floater.AverageWaterNormal;

                if (!IsFinite(waterHeight) || !IsFinite(waterNormal))
                {
                    // Le floater a renvoyé une valeur non-finie (ex: aucun point d'eau
                    // échantillonné ce frame -> division par zéro côté FloaterGetterController).
                    // On ignore ce frame plutôt que d'empoisonner _currentSeaHeight pour toujours :
                    // Mathf.Lerp avec un NaN en entrée reste NaN indéfiniment.
                    if (!_corruptionWarningLogged)
                    {
                        _corruptionWarningLogged = true;
                        Debug.LogWarning($"{name}: FloaterGetterController a renvoyé une valeur non-finie " +
                            $"(hauteur={waterHeight}, normale={waterNormal}). Effets de mer ignorés ce frame. " +
                            $"Vérifier la config/les points d'échantillonnage du floater.");
                    }
                }
                else
                {
                    _corruptionWarningLogged = false;

                    float targetSeaHeight = waterHeight + _waterHeightOffset;

                    _currentSeaHeight = Mathf.Lerp(
                        _currentSeaHeight,
                        targetSeaHeight,
                        1f - Mathf.Exp(-_waterHeightLerp * deltaTime)
                    );

                    Vector3 seaUp = (waterNormal.sqrMagnitude > 0.001f)
                        ? waterNormal.normalized
                        : Vector3.up;

                    Vector3 fwd = Quaternion.Euler(0f, _currentYawDeg, 0f) * Vector3.forward;
                    Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, seaUp).normalized;
                    if (fwdOnPlane.sqrMagnitude < 1e-4f)
                        fwdOnPlane = Vector3.forward;

                    Quaternion waterAlignRot = Quaternion.LookRotation(fwdOnPlane, seaUp);
                    Vector3 waterEul = waterAlignRot.eulerAngles;

                    float targetPitch = Normalize180(waterEul.x);
                    float targetRoll = Normalize180(waterEul.z);

                    _pitchDeg = Mathf.Lerp(_pitchDeg, targetPitch, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
                    _rollDeg = Mathf.Lerp(_rollDeg, targetRoll, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
                }
            }
            else
            {
                _currentSeaHeight = Mathf.Lerp(_currentSeaHeight, _currentPos.y, 1f - Mathf.Exp(-_waterHeightLerp * deltaTime));
                _pitchDeg = Mathf.Lerp(_pitchDeg, 0f, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
                _rollDeg = Mathf.Lerp(_rollDeg, 0f, 1f - Mathf.Exp(-_waterTiltLerp * deltaTime));
            }
        }

        private void FinalizeTransform()
        {
            _currentRot = Quaternion.Euler(_pitchDeg, _currentYawDeg, _rollDeg);
            _currentPos.y = _currentSeaHeight;

            // Filet de sécurité final : si malgré tout un NaN/Infinity s'est glissé
            // quelque part (path corrompu, floater externe, etc.), on restaure le
            // dernier état valide plutôt que de contaminer le Rigidbody/PhysicsMover
            // pour le reste de la partie.
            if (IsFinite(_currentPos) && IsFinite(_currentRot) && IsFinite(_currentSeaHeight))
            {
                _lastValidPos = _currentPos;
                _lastValidRot = _currentRot;
                _lastValidSeaHeight = _currentSeaHeight;
            }
            else
            {
                Debug.LogError($"{name}: position/rotation non-finie détectée " +
                    $"(pos={_currentPos}, rot={_currentRot.eulerAngles}, seaHeight={_currentSeaHeight}). " +
                    $"Restauration du dernier état valide.");

                _currentPos = _lastValidPos;
                _currentRot = _lastValidRot;
                _currentSeaHeight = _lastValidSeaHeight;
                _pitchDeg = 0f;
                _rollDeg = 0f;
                _velocity = Vector3.zero;
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool IsFinite(Vector3 value) =>
            IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

        private static bool IsFinite(Quaternion value) =>
            IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z) && IsFinite(value.w);

        #endregion

        #region Helpers

        private static float Normalize180(float deg)
        {
            deg %= 360f;
            if (deg > 180f) deg -= 360f;
            return deg;
        }

        public void ResetToStart() => InitializeOnPath();

        public void CompletePathNow()
        {
            if (IsPathCompleted) return;

            _currentSegment = BoatSegment.Final;
            IsPathCompleted = true;

            if (!_pathCompletedInvoked)
            {
                _pathCompletedInvoked = true;
                OnPathCompleted?.Invoke();
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            DrawWaypointsGizmo(_commonWaypoints, Color.cyan);

            if (_directionPaths != null)
            {
                foreach (var stepEntry in _directionPaths)
                {
                    var stepOptions = stepEntry.Value;
                    if (stepOptions == null) continue;

                    foreach (var branchEntry in stepOptions)
                    {
                        DrawWaypointsGizmo(branchEntry.Value, GetDirectionColor(branchEntry.Key));
                    }
                }
            }

            DrawWaypointsGizmo(_finalWaypoints, new Color(1f, 0.55f, 0f)); // orange : tronc final

            if (_playerSpawnTutorial != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(_playerSpawnTutorial.position, 0.5f);
                Gizmos.DrawRay(_playerSpawnTutorial.position, _playerSpawnTutorial.forward * 1.5f);
            }

            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_currentPos, 0.4f);
                Gizmos.DrawRay(_currentPos, (_currentRot * Vector3.forward) * 2f);

                if (_velocity.sqrMagnitude > 0.0001f)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(_currentPos, _velocity);
                }
            }
        }

        private static Color GetDirectionColor(BoatDirection direction)
        {
            switch (direction)
            {
                case BoatDirection.Left: return Color.red;
                case BoatDirection.Midle: return Color.blue;
                case BoatDirection.Right: return Color.green;
                default: return Color.white;
            }
        }

        /// <summary>
        /// Dessine une série de waypoints Vector3 : plus besoin de vérifier des références
        /// Transform potentiellement nulles, ce sont de simples points dans l'espace.
        /// </summary>
        private static void DrawWaypointsGizmo(Vector3[] waypoints, Color color)
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = color;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 0.3f);
                if (i < waypoints.Length - 1)
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
        }

        #endregion
    }
}