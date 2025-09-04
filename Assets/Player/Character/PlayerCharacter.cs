using KinematicCharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.KinematicCharacterController
{
    //enum and not a bool if we want to press or hold...
    public enum CrouchInput
    {
        None, Toggle
    }
    public enum SprintInput
    {
        None,
        Sprinting,
        StopSprint,
        Toggle
    }

    public enum HandsState
    {
        None,
        GrabbingItem
    }

    public enum Stance
    {
        Stand,
        Crouch,
    }

    [System.Serializable]
    public struct CharacterState
    {
        public bool IsGrounded;
        public Stance Stance;
        public Vector3 Velocity;
        public Vector3 AccelerationVelocity;
    }

    [System.Serializable]
    public struct PlayerCharacterInputs
    {
        public Quaternion CameraRotation;
        public Vector2 MoveInput;
        public bool Jump; //true only when jump pressed on the curr frame
        public SprintInput Sprint;
        public CrouchInput Crouch;
    }

    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor _motor;
        public KinematicCharacterMotor Motor => _motor;

        [SerializeField] private Transform _cameraTarget;

        [Header("Stable Movement")]
        [SerializeField] private float _walkSpeed = 8f;
        [SerializeField] private float _walkSharpness = 15f;
        [SerializeField] private float _orientationSharpness = 10;

        [Header("Camera")]
        [SerializeField, Range(0, 1f)] private float _standCameraTargetHeight = 0.9f;
        [SerializeField, Range(0, 1f)] private float _crouchCameraTargetHeight = 0.7f;
        [SerializeField] private float _crouchHeightResponse = 15.0f;

        [Header("Srinting")]
        [SerializeField] private float _sprintSpeed = 15f;
        [SerializeField] private float _sprintSharpness = 15;

        [Header("Crouching")]
        [SerializeField] private float _crouchSpeed = 5.0f;
        [SerializeField] private float _crouchSharpness = 25f;
        [SerializeField] private float _standHeight = 2.0f;
        [SerializeField] private float _crouchHeight = 1.0f;
        [SerializeField, Range(0, 1f)] private float _crouchDiminutionMultiplier = 0.5f;

        [Header("Air Movement")]
        public float _maxAirMoveSpeed = 10f;
        public float _airAccelerationSpeed = 5f;
        public float _drag = 0.1f;

        [Header("Jumping")]
        public float _jumpSpeed = 10f;
        public float _jumpPreGroundingGraceTime = 0f;
        public float _jumpPostGroundingGraceTime = 0.2f;

        [Header("Misc")]
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public bool EnableScaleRoot = true;
        public Transform MeshRoot;

        [Header("Debug")]
        private float _currentSpeed;
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;

        public bool _requestedSprint = false;
        public bool _requestedCrouch = false;

        //Jump
        public bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;

        public float SprintSpeed => _sprintSpeed;
        public float CurrentHeight
        {
            get
            {
                return _state.Stance switch
                {
                    Stance.Stand => _standHeight,
                    Stance.Crouch => _crouchHeight,
                    _ => _standHeight,
                };
            }
        }
        //States
        public CharacterState _state;
        private Collider[] _uncrouchOverlapResults;

        //allow to ignore collision with specifics colliders
        private HashSet<Collider> _ignoredColliders = new HashSet<Collider>();

        //PROPERTIES
        public float CurrentSpeed
        {
            get => _currentSpeed;
        }

        public void SetPosition(Vector3 pos)
        {
            Motor.SetPosition(pos);
        }

        public void SetRotation(Quaternion rot)
        {
            Motor.SetRotation(rot);
        }

        public void Initialize()
        {
            _motor.CharacterController = this;
            _state.Stance = Stance.Stand;
            _uncrouchOverlapResults = new Collider[8];
        }

        public void ForceCutVelocity()
        {
            _state.Velocity = Vector3.zero; 
            _state.AccelerationVelocity = Vector3.zero;
            _moveInputVector = Vector3.zero;
        }

        /// <summary>
        /// Put in a list to ignore the collision with a specific object
        /// </summary>
        /// <param name="col"></param>
        public void IgnoreCollider(Collider col)
        {
            if (!_ignoredColliders.Contains(col))
            {
                _ignoredColliders.Add(col);
            }
        }

        /// <summary>
        /// Check the list and restore the collision with this object
        /// </summary>
        public void RestoreCollider(Collider col)
        {
            if(_ignoredColliders.Contains(col))
                _ignoredColliders.Remove(col);
        }

        /// <summary>
        /// This is called every frame by MyPlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveInput.x, 0f, inputs.MoveInput.y), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, _motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, _motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, _motor.CharacterUp);

            // Move and look inputs
            _moveInputVector = cameraPlanarRotation * moveInputVector;
            _lookInputVector = cameraPlanarDirection;

            if (inputs.Jump && _state.Stance == Stance.Stand)
            {
                _jumpRequested = true;
                _timeSinceJumpRequested = 0f;
            }

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
        }

        public Transform GetCameraTarget() => _cameraTarget;

        public CharacterState GetState() => _state;

        public void UpdateCapsuleMeshRoot(float deltaTime)
        {
            float currentHeight = _motor.Capsule.height;
            float cameraTargetHeight = 0f;

            switch (_state.Stance)
            {
                case Stance.Stand:
                    cameraTargetHeight = currentHeight * _standCameraTargetHeight;
                    break;
                case Stance.Crouch:
                    cameraTargetHeight = currentHeight * _crouchCameraTargetHeight;
                    break;
            }

            _cameraTarget.localPosition = Vector3.Lerp
            (
                a: _cameraTarget.localPosition,
                b: new Vector3(0f, cameraTargetHeight, 0f),
                //to make framerate independant we can use equation below
                t: 1f - Mathf.Exp(-_crouchHeightResponse * deltaTime) //same as _crouchHeightResponse * deltaTime
            );

            if (EnableScaleRoot)
            {
                float normalizedHeight = currentHeight / _standHeight;
                Vector3 rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
                MeshRoot.localScale = Vector3.Lerp
                (
                    a: MeshRoot.localScale,
                    b: rootTargetScale,
                    1f - Mathf.Exp(-_crouchHeightResponse * deltaTime)
                );
            }
        }

        #region ICharacterController Callbacks
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_lookInputVector != Vector3.zero && _orientationSharpness > 0f)
            {
                Vector3 gravityUp = -Gravity.normalized;
                Vector3 lookDirection = Vector3.ProjectOnPlane(_lookInputVector, gravityUp).normalized;

                if (lookDirection.sqrMagnitude > 0f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection, gravityUp);
                    currentRotation = Quaternion.Slerp(currentRotation, targetRotation, 1 - Mathf.Exp(-_orientationSharpness * deltaTime));
                }
            }
        }


        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            _state.AccelerationVelocity = Vector3.zero;
            Vector3 targetMovementVelocity = Vector3.zero;
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
                currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, _motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, _motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(_motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;

                float targetSpeed = 0f;
                float targetSharpness = 25f;

                //Handle the target speed and Sharpness based on Stance and other
                switch (_state.Stance)
                {
                    case Stance.Stand:
                        if (_requestedSprint)
                        {
                            targetSpeed = _sprintSpeed;
                            targetSharpness = _sprintSharpness;
                        }
                        else
                        {
                            targetSpeed = _walkSpeed;
                            targetSharpness = _walkSharpness;
                        }
                        break;
                    case Stance.Crouch:
                        targetSpeed = _crouchSpeed;
                        targetSharpness = _crouchSharpness;
                        break;
                }
                _currentSpeed = targetSpeed;
                targetMovementVelocity = reorientedInput * targetSpeed;

                _state.AccelerationVelocity = targetMovementVelocity - currentVelocity;
                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-targetSharpness * deltaTime));
            }
            //In the air...
            else
            {
                //if player is moving
                if (_moveInputVector.sqrMagnitude > 0)
                {
                    targetMovementVelocity = _moveInputVector * _maxAirMoveSpeed;

                    // Prevent climbing on un - stable slopes with air movement
                    if (_motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(-Gravity.normalized, _motor.GroundingStatus.GroundNormal), -Gravity.normalized).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * _airAccelerationSpeed * deltaTime;
                }

                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (_drag * deltaTime)));
            }

            // Handle jumping

            if (_state.Stance != Stance.Stand) return;
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;

            if (_jumpRequested)
            {
                // See if we actually are allowed to jump
                if (!_jumpConsumed && _motor.GroundingStatus.FoundAnyGround || _timeSinceLastAbleToJump <= _jumpPostGroundingGraceTime && !_jumpConsumed)
                {
                    // Calculate jump direction before ungrounding
                    Vector3 jumpDirection = -Gravity.normalized;
                    if (_motor.GroundingStatus.FoundAnyGround && !_motor.GroundingStatus.IsStableOnGround)
                    {
                        jumpDirection = _motor.GroundingStatus.GroundNormal;
                    }

                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    _motor.ForceUnground(0.1f);

                    // Add to the return velocity and reset jump state
                    currentVelocity += (jumpDirection * _jumpSpeed) - Vector3.Project(currentVelocity, -Gravity.normalized);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;
                }
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            //Handle Crouching
            {
                if (_requestedCrouch && _state.Stance != Stance.Crouch)
                {
                    _state.Stance = Stance.Crouch;
                    _motor.SetCapsuleDimensions(_motor.Capsule.radius, _crouchHeight, _crouchHeight * _crouchDiminutionMultiplier);
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            //var totalAcceleration = (_state.Velocity - _lastState.Velocity) / deltaTime;
            //_state.Acceleration = Vector3.ClampMagnitude(_state.Acceleration, totalAcceleration.magnitude);

            // Handle jump-related values
            if(_state.Stance == Stance.Stand)
            {
                // Handle jumping pre-ground grace period
                if (_jumpRequested && _timeSinceJumpRequested > _jumpPreGroundingGraceTime)
                {
                    _jumpRequested = false;
                }

                if (_motor.GroundingStatus.FoundAnyGround)
                {
                    // If we're on a ground surface, reset jumping values
                    if (!_jumpedThisFrame)
                    {
                        _jumpConsumed = false;
                    }
                    _timeSinceLastAbleToJump = 0f;
                }
                else
                {
                    // Keep track of time since we were last able to jump (for grace period)
                    _timeSinceLastAbleToJump += deltaTime;
                }
            }

            //Handle Uncrouch
            {
                //Uncrouch
                if (!_requestedCrouch && _state.Stance == Stance.Crouch)
                {
                    //Tentatively "standup" the character capsule
                    _motor.SetCapsuleDimensions
                    (
                        radius: _motor.Capsule.radius,
                        height: _standHeight,
                        yOffset: _standHeight * 0.5f
                    );

                    //Then see if the capsule overlaps any colliders before actually allowing
                    //the character to stand up
                    var pos = _motor.TransientPosition;
                    var rot = _motor.TransientRotation;
                    var mask = _motor.CollidableLayers;
                    if (_motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
                    {
                        _requestedCrouch = true;
                        _motor.SetCapsuleDimensions
                        (
                            radius: _motor.Capsule.radius,
                            height: _crouchHeight,
                            yOffset: _crouchHeight * _crouchDiminutionMultiplier
                        );
                    }
                    else
                    {
                        _state.Stance = Stance.Stand;
                    }
                }
            }

            _state.Velocity = _motor.Velocity; //we put her here because the motor called it in Phase2 / AfterCharacterUpdate
            _state.IsGrounded = _motor.GroundingStatus.IsStableOnGround;
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (_ignoredColliders.Contains(coll))
                return false;

            return true;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider) { }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

        public void PostGroundingUpdate(float deltaTime) { }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

        #endregion
    }

}
