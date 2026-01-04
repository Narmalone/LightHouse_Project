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
        public void SetInputs(in PlayerCharacterInputs inputs)
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
        private Vector3 _groundNormal = Vector3.up;

        private void UpdateGrounding(float dt)
        {
            _timeSinceJumpPressed += dt;

            // capsule cast down
            Vector3 up = Vector3.up;
            float radius = Mathf.Max(0.01f, _capsule.radius);
            float height = Mathf.Max(_capsule.height, radius * 2f);
            Vector3 center = transform.position + _capsule.center;

            float half = Mathf.Max(0f, (height * 0.5f) - radius);
            Vector3 p1 = center + up * half;
            Vector3 p2 = center - up * half;

            bool hit = Physics.CapsuleCast(
                p1, p2, radius,
                Vector3.down,
                out RaycastHit rh,
                _groundCheckDistance,
                _groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (hit)
            {
                _groundNormal = rh.normal;
                float angle = Vector3.Angle(_groundNormal, Vector3.up);
                _state.GroundAngle = angle;

                bool slopeOk = angle <= _maxSlopeAngle;
                _state.IsGrounded = slopeOk && Vector3.Dot(_rb.linearVelocity, Vector3.up) <= 2f;

                if (_state.IsGrounded)
                    _timeSinceGrounded = 0f;
                else
                    _timeSinceGrounded += dt;
            }
            else
            {
                _state.IsGrounded = false;
                _state.GroundAngle = 90f;
                _groundNormal = Vector3.up;
                _timeSinceGrounded += dt;
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
            if (_requestedCrouch && _state.Stance != Stance.Crouch)
            {
                _state.Stance = Stance.Crouch;
                ApplyCapsuleHeight(_crouchHeight);
            }

            if (!_requestedCrouch && _state.Stance == Stance.Crouch)
            {
                // test overlap stand
                float radius = _capsule.radius;
                Vector3 centerWorld = transform.position + _capsule.center;

                float standHalf = Mathf.Max(0f, (_standHeight * 0.5f) - radius);
                Vector3 p1 = centerWorld + Vector3.up * standHalf;
                Vector3 p2 = centerWorld - Vector3.up * standHalf;

                int count = Physics.OverlapCapsuleNonAlloc(
                    p1, p2, radius,
                    _uncrouchOverlap,
                    _groundMask,
                    QueryTriggerInteraction.Ignore
                );

                if (count > 0)
                {
                    _requestedCrouch = true; // reste accroupi
                    return;
                }

                _state.Stance = Stance.Stand;
                ApplyCapsuleHeight(_standHeight);
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
    }
}
