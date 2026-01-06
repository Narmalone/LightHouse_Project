using System;
using UnityEngine;

namespace LightHouse.PhysicsCharacter
{
    public enum CrouchInput { None, Toggle }
    public enum SprintInput { None, Sprinting, StopSprint, Toggle }
    public enum Stance { Stand, Crouch }

    [Serializable]
    public struct CharacterState
    {
        public bool IsGrounded;
        public Stance Stance;
        public Vector3 Velocity;
        public Vector3 AccelerationVelocity;
        public float GroundAngle;
    }

    [Serializable]
    public struct PlayerCharacterInputs
    {
        public Quaternion CameraRotation;
        public Vector2 MoveInput;
        public bool Jump;
        public SprintInput Sprint;
        public CrouchInput Crouch;
    }

    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class RigidbodyCharacterMotor : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform _meshRoot;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private Animator _animator;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 8f;
        [SerializeField] private float _sprintSpeed = 15f;
        [SerializeField] private float _crouchSpeed = 5f;

        [Tooltip("Acc�l�ration au sol (m/s� approx). Plus haut = plus 'snappy'")]
        [SerializeField] private float _groundAccel = 40f;

        [Tooltip("D�c�l�ration au sol quand pas d'input")]
        [SerializeField] private float _groundDecel = 55f;

        [Header("Air")]
        [SerializeField] private float _airAccel = 12f;
        [SerializeField] private float _airMaxSpeed = 10f;
        [SerializeField] private float _airDrag = 0.05f;

        [Header("Jump")]
        [SerializeField] private float _jumpSpeed = 10f;
        [SerializeField] private float _coyoteTime = 0.15f;      // post-ground grace
        [SerializeField] private float _jumpBuffer = 0.12f;      // pre-ground buffer

        [Header("Gravity")]
        [SerializeField] private Vector3 _gravity = new(0, -30f, 0);

        [Header("Grounding")]
        [SerializeField] private float _groundCheckDistance = 0.08f;
        [SerializeField] private float _maxSlopeAngle = 55f;
        [SerializeField] private LayerMask _groundMask = ~0;

        [Header("Crouch")]
        [SerializeField] private float _standHeight = 2f;
        [SerializeField] private float _crouchHeight = 1f;
        [SerializeField, Range(0, 1f)] private float _crouchCenterMultiplier = 0.5f;
        [SerializeField] private float _crouchHeightResponse = 15f;

        // runtime
        private Rigidbody _rb;
        private CapsuleCollider _capsule;

        private PlayerCharacterInputs _inputs;
        private Vector3 _moveWorld;
        private Vector3 _lookWorld;
        private bool _requestedSprint;
        private bool _requestedCrouch;

        private float _timeSinceGrounded;
        private float _timeSinceJumpPressed;

        private bool _jumpConsumed;
        private bool _jumpedThisFrame;

        private CharacterState _state;

        // anim hashes (comme toi)
        private int AnimIDSpeed;
        private int AnimIDGrounded;
        private int AnimIdJump;
        private int AnimIdFreeFall;
        private int AnimIDMotionSpeed;

        private float _fallTimeout = 0.15f;
        private float _fallTimeoutDelta;
        private float _animSpeedBlend;
        [SerializeField] private float _animSpeedChangeRate = 10f;
        private float _targetAnimSpeed;
        private float _lastInputMagnitude;

        private Collider[] _uncrouchOverlap = new Collider[8];

        public CharacterState State => _state;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();

            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Animator hashes
            AnimIDSpeed = Animator.StringToHash("Speed");
            AnimIDGrounded = Animator.StringToHash("Grounded");
            AnimIdJump = Animator.StringToHash("Jump");
            AnimIdFreeFall = Animator.StringToHash("FreeFall");
            AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _fallTimeoutDelta = _fallTimeout;

            // init stance
            _state.Stance = Stance.Stand;
            ApplyCapsuleHeight(_standHeight);
        }

        // Appel� par ton "MyPlayer" (comme SetInputs)
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            _inputs = inputs;

            // move input clamp
            var move = Vector3.ClampMagnitude(new Vector3(inputs.MoveInput.x, 0, inputs.MoveInput.y), 1f);
            _lastInputMagnitude = move.magnitude;

            // camera planar
            Vector3 camForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Vector3.up).normalized;
            if (camForward.sqrMagnitude < 1e-6f) camForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Vector3.up).normalized;
            Quaternion camRot = Quaternion.LookRotation(camForward, Vector3.up);

            _moveWorld = camRot * move;
            _lookWorld = camForward;

            if (inputs.Jump)
                _timeSinceJumpPressed = 0f;

            _requestedSprint = inputs.Sprint switch
            {
                SprintInput.None => false,
                SprintInput.Sprinting => true,
                SprintInput.StopSprint => false,
                SprintInput.Toggle => !_requestedSprint,
                _ => _requestedSprint
            };

            _requestedCrouch = inputs.Crouch switch
            {
                CrouchInput.None => _requestedCrouch,
                CrouchInput.Toggle => !_requestedCrouch,
                _ => _requestedCrouch
            };

            float baseSpeed = _state.Stance == Stance.Crouch ? _crouchSpeed : (_requestedSprint ? _sprintSpeed : _walkSpeed);
            _targetAnimSpeed = (_lastInputMagnitude < 0.01f) ? 0f : baseSpeed * _lastInputMagnitude;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            _jumpedThisFrame = false;

            UpdateGrounding(dt);
            HandleCrouch(dt);

            // face direction (tu peux aussi faire un yaw-only)
            if (_lookWorld.sqrMagnitude > 1e-6f)
            {
                var targetRot = Quaternion.LookRotation(_lookWorld, Vector3.up);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, 1f - Mathf.Exp(-12f * dt)));
            }

            Vector3 v = _rb.linearVelocity;

            if (_state.IsGrounded)
            {
                // mouvement sol : on vise une vitesse planare
                float baseSpeed = _state.Stance == Stance.Crouch ? _crouchSpeed : (_requestedSprint ? _sprintSpeed : _walkSpeed);
                Vector3 desired = _moveWorld * (baseSpeed * _lastInputMagnitude);

                // projeter sur plan du sol pour suivre pente
                desired = Vector3.ProjectOnPlane(desired, _groundNormal);

                Vector3 planar = Vector3.ProjectOnPlane(v, Vector3.up);
                Vector3 diff = desired - planar;

                float accel = (_lastInputMagnitude > 0.01f) ? _groundAccel : _groundDecel;
                Vector3 change = Vector3.ClampMagnitude(diff, accel * dt);

                v += change;

                // coller au sol : annule la petite vitesse verticale r�siduelle vers le haut
                if (v.y > 0f) v.y = 0f;
            }
            else
            {
                // air control
                if (_moveWorld.sqrMagnitude > 1e-6f)
                {
                    Vector3 desired = _moveWorld * _airMaxSpeed;
                    Vector3 planar = Vector3.ProjectOnPlane(v, Vector3.up);
                    Vector3 diff = desired - planar;

                    Vector3 change = Vector3.ClampMagnitude(diff, _airAccel * dt);
                    v += change;
                }

                // gravit� + drag
                v += _gravity * dt;
                v *= (1f / (1f + (_airDrag * dt)));
            }

            HandleJump(ref v, dt);

            _state.AccelerationVelocity = (v - _rb.linearVelocity) / Mathf.Max(dt, 1e-6f);
            _rb.linearVelocity = v;

            // state
            _state.Velocity = _rb.linearVelocity;

            SyncAnimator(dt);
            UpdateMeshScale(dt);
        }

        // -------- Grounding --------
        [Header("Ground Sphere Check")]
        [SerializeField] private float _groundSphereRadius = 0.18f;   // ~ 80-90% du radius capsule en général
        [SerializeField] private float _groundSphereOffsetUp = 0.02f;  // remonte un poil la sphère pour éviter l'auto-contact
        [SerializeField] private float _groundNormalProbeDistance = 0.35f; // pour récupérer la normale

        private readonly Collider[] _groundHits = new Collider[16];

        private Vector3 _groundSphereCenterWS;
        private bool _groundSphereHit;

        private Vector3 _groundNormal = Vector3.up;

        [SerializeField] private float _groundSkin = 0.02f;     // petit margin
        [SerializeField] private float _maxGroundSnapUp = 2f;   // tolérance vitesse verticale
        private void UpdateGrounding(float dt)
        {
            _timeSinceJumpPressed += dt;

            Quaternion rot = transform.rotation;
            Vector3 up = rot * Vector3.up;

            // --- calcule le bas world de la capsule ---
            // center world
            Vector3 centerWS = _rb.position + rot * _capsule.center;

            float radius = Mathf.Max(0.01f, _capsule.radius);
            float height = Mathf.Max(_capsule.height, radius * 2f);

            // bas de capsule (centre - (halfHeight - radius))
            float half = height * 0.5f;
            float bottomOffset = Mathf.Max(0f, half - radius);
            Vector3 bottomWS = centerWS - up * bottomOffset;

            // centre de la sphère : légèrement au-dessus du bas
            float sphereR = Mathf.Max(0.01f, _groundSphereRadius);
            _groundSphereCenterWS = bottomWS + up * (_groundSphereOffsetUp + sphereR);

            // --- overlap ---
            int count = Physics.OverlapSphereNonAlloc(
                _groundSphereCenterWS,
                sphereR,
                _groundHits,
                _groundMask,
                QueryTriggerInteraction.Ignore
            );

            // filtre: ignore notre capsule (au cas où) + ignore triggers
            bool found = false;
            Collider foundCol = null;

            for (int i = 0; i < count; i++)
            {
                var c = _groundHits[i];
                if (!c) continue;
                if (c == _capsule) continue;
                found = true;
                foundCol = c;
                break;
            }

            _groundSphereHit = found;

            if (found)
            {
                // probe normale : petit SphereCast / Raycast vers le bas
                RaycastHit rh;
                bool probeHit = Physics.SphereCast(
                    origin: _groundSphereCenterWS + up * 0.05f,
                    radius: sphereR * 0.6f,
                    direction: -up,
                    hitInfo: out rh,
                    maxDistance: _groundNormalProbeDistance,
                    layerMask: _groundMask,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore
                );

                if (probeHit && rh.collider != _capsule)
                {
                    _groundNormal = rh.normal;
                    _lastGroundHit = rh;      // si tu veux garder ton debug hit
                    _lastCastHit = true;
                }
                else
                {
                    _groundNormal = up;       // fallback
                    _lastCastHit = false;
                }

                float angle = Vector3.Angle(_groundNormal, up);
                _state.GroundAngle = angle;

                bool slopeOk = angle <= _maxSlopeAngle;

                float upVel = Vector3.Dot(_rb.linearVelocity, up);
                _state.IsGrounded = slopeOk && upVel <= _maxGroundSnapUp;

                if (_state.IsGrounded) _timeSinceGrounded = 0f;
                else _timeSinceGrounded += dt;
            }
            else
            {
                _state.IsGrounded = false;
                _state.GroundAngle = 90f;
                _groundNormal = Vector3.up;   // ou up
                _timeSinceGrounded += dt;

                _lastCastHit = false;
            }
        }


        // -------- Jump (coyote + buffer) --------
        private void HandleJump(ref Vector3 v, float dt)
        {
            bool wantsJump = _timeSinceJumpPressed <= _jumpBuffer;

            bool canJump =
                !_jumpConsumed &&
                (_state.IsGrounded || _timeSinceGrounded <= _coyoteTime);

            if (wantsJump && canJump && _state.Stance == Stance.Stand)
            {
                // reset vertical then apply jump
                v = Vector3.ProjectOnPlane(v, Vector3.up);
                v += Vector3.up * _jumpSpeed;

                _jumpConsumed = true;
                _jumpedThisFrame = true;

                // consomme le buffer
                _timeSinceJumpPressed = float.PositiveInfinity;
            }

            // reset quand on retouche le sol (et qu�on n�est pas sur le frame du jump)
            if (_state.IsGrounded && !_jumpedThisFrame)
                _jumpConsumed = false;
        }

        // -------- Crouch / Uncrouch --------
        private void HandleCrouch(float dt)
        {
            // 1) Demande d'entrer en crouch
            if (_requestedCrouch && _state.Stance != Stance.Crouch)
            {
                _state.Stance = Stance.Crouch;
                ApplyCapsuleHeight(_crouchHeight);
                return;
            }

            // 2) Demande de sortir du crouch
            if (!_requestedCrouch && _state.Stance == Stance.Crouch)
            {
                // On vérifie si on peut se relever sans collision
                if (CanStandUp())
                {
                    _state.Stance = Stance.Stand;
                    ApplyCapsuleHeight(_standHeight);
                }
                else
                {
                    // Toujours bloqué : on force le maintien en crouch
                    _requestedCrouch = true;
                }
            }
        }


        private void ApplyCapsuleHeight(float height)
        {
            height = Mathf.Max(height, _capsule.radius * 2f);
            _capsule.height = height;
            _capsule.center = new Vector3(_capsule.center.x, height * _crouchCenterMultiplier, _capsule.center.z);
        }

        // -------- Anim --------
        private void SyncAnimator(float dt)
        {
            if (_animator == null) return;

            _animator.SetBool(AnimIDGrounded, _state.IsGrounded);

            if (_jumpedThisFrame) _animator.SetBool(AnimIdJump, true);
            else if (_state.IsGrounded) _animator.SetBool(AnimIdJump, false);

            if (_state.IsGrounded)
            {
                _fallTimeoutDelta = _fallTimeout;
                _animator.SetBool(AnimIdFreeFall, false);
            }
            else
            {
                if (_fallTimeoutDelta > 0f)
                {
                    _fallTimeoutDelta -= dt;
                    if (_fallTimeoutDelta <= 0f)
                        _animator.SetBool(AnimIdFreeFall, true);
                }
            }

            _animSpeedBlend = Mathf.Lerp(_animSpeedBlend, _targetAnimSpeed, dt * _animSpeedChangeRate);
            if (_animSpeedBlend < 0.01f) _animSpeedBlend = 0f;

            _animator.SetFloat(AnimIDSpeed, _animSpeedBlend);
            _animator.SetFloat(AnimIDMotionSpeed, _lastInputMagnitude);
        }

        private void UpdateMeshScale(float dt)
        {
            if (_meshRoot == null) return;

            float h = _capsule.height;
            float normalized = h / Mathf.Max(0.001f, _standHeight);
            Vector3 targetScale = new Vector3(1f, normalized, 1f);

            _meshRoot.localScale = Vector3.Lerp(
                _meshRoot.localScale,
                targetScale,
                1f - Mathf.Exp(-_crouchHeightResponse * dt)
            );
        }


        [Header("Uncrouch")]
        [SerializeField] private LayerMask _uncrouchObstructionMask = ~0; // mets "Default + Props + Walls" etc, MAIS PAS Ground
        [SerializeField] private float _uncrouchSkin = 0.02f;
        private bool CanStandUp()
        {
            Quaternion rot = transform.rotation;
            Vector3 up = rot * Vector3.up;

            float radius = Mathf.Max(0.01f, _capsule.radius);
            float standHeight = Mathf.Max(_standHeight, radius * 2f);

            // On veut tester la capsule "debout" AU MEME ENDROIT AU SOL.
            // Donc on calcule le bottom actuel (en crouch), puis on reconstruit la capsule debout en gardant ce bottom.
            Vector3 centerWS_Current = _rb.position + rot * _capsule.center;

            float currentHeight = Mathf.Max(_capsule.height, radius * 2f);
            float currentHalf = currentHeight * 0.5f;
            float currentSegment = Mathf.Max(0f, currentHalf - radius);

            // centre de l'hémisphère du bas actuel
            Vector3 bottomHemisphereCenterWS = centerWS_Current - up * currentSegment;

            // pour une capsule debout, segment debout
            float standHalf = standHeight * 0.5f;
            float standSegment = Mathf.Max(0f, standHalf - radius);

            // centreWS debout = bottomHemisphereCenter + up * standSegment
            Vector3 centerWS_Stand = bottomHemisphereCenterWS + up * standSegment;

            // endpoints capsule debout (centres des hémisphères)
            Vector3 p1 = centerWS_Stand + up * standSegment;
            Vector3 p2 = centerWS_Stand - up * standSegment;

            // un petit skin pour éviter les faux positifs au contact
            float r = radius - _uncrouchSkin;
            if (r < 0.001f) r = radius;

            int count = Physics.OverlapCapsuleNonAlloc(
                p1, p2, r,
                _uncrouchOverlap,
                _uncrouchObstructionMask,
                QueryTriggerInteraction.Ignore
            );

            // filtre sécurité : ignore notre capsule si jamais
            for (int i = 0; i < count; i++)
            {
                var c = _uncrouchOverlap[i];
                if (!c) continue;
                if (c == _capsule) continue;
                return false; // obstruction
            }

            return true; // ok pour se relever
        }

        [Header("Debug Ground Cast")]
        [SerializeField] private bool _drawGroundCastGizmos = true;
        [SerializeField] private bool _logGroundHit = false;

        private RaycastHit _lastGroundHit;
        private bool _lastCastHit;
        private Vector3 _dbgP1, _dbgP2, _dbgUp;
        private float _dbgRadius, _dbgCastDist;

        private void OnDrawGizmosSelected()
        {
            if (!_drawGroundCastGizmos) return;

            var rb = _rb != null ? _rb : GetComponent<Rigidbody>();
            var cap = _capsule != null ? _capsule : GetComponent<CapsuleCollider>();
            if (!rb || !cap) return;

            Quaternion rot = transform.rotation;
            Vector3 up = rot * Vector3.up;

            float radius = Mathf.Max(0.01f, cap.radius);
            float height = Mathf.Max(cap.height, radius * 2f);

            Vector3 centerWS = rb.position + rot * cap.center;
            float half = height * 0.5f;
            float bottomOffset = Mathf.Max(0f, half - radius);
            Vector3 bottomWS = centerWS - up * bottomOffset;

            float sphereR = Mathf.Max(0.01f, _groundSphereRadius);
            Vector3 sphereCenter = bottomWS + up * (_groundSphereOffsetUp + sphereR);

            Gizmos.color = (_state.IsGrounded ? Color.green : (_groundSphereHit ? Color.yellow : Color.red));

            // Sphère overlap
            Gizmos.DrawWireSphere(sphereCenter, sphereR);

            // Ligne vers le bas pour illustrer la probe de normale
            Gizmos.DrawLine(sphereCenter, sphereCenter - up * _groundNormalProbeDistance);

            // Normale (si on en a une)
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(sphereCenter, _groundNormal * 0.5f);
            }
        }


    }
}
