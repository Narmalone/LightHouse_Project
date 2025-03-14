using UnityEngine;
using KinematicCharacterController;

namespace LightHouse.AdvancedController
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
        StopSprint
    }

    public enum Stance
    {
        Stand,
        Crouch,
    }

    public struct CharacterState
    {
        public bool Grounded;
        public Stance Stance;
        public Vector3 Velocity;
        public Vector3 Acceleration;
    }

    public struct CharacterInput
    {
        public Quaternion Rotation;
        public Vector2 Move;
        public bool Jump; //true only when jump pressed on the curr frame
        public SprintInput Sprint;
        public CrouchInput Crouch;
    }

    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor _motor;
        [SerializeField] private Transform _cameraTarget;

        [Header("Mesh Scale"), Space]
        [SerializeField] private bool _canScaleRoot = true;
        [SerializeField] private Transform _root;

        [Header("Speeds and lerp reponse"), Space]
        [SerializeField] private float _walkSpeed = 8.0f;
        [SerializeField] private float _crouchSpeed = 5.0f;
        [SerializeField] private float _sprintSpeed = 12.0f;

        [SerializeField] private float _walkResponse = 25f; //Kind of acceleration instead of just setting walkSpeed
        [SerializeField] private float _crouchResponse = 20f; //Kind of acceleration instead of just setting walkSpeed
        [SerializeField] private float _sprintResponse = 20f; //Kind of acceleration instead of just setting walkSpeed

        [Header("Jump & Gravity"), Space]
        [SerializeField] private float _jumpSpeed = 20.0f;
        [SerializeField] private float _coyoteTime  = 0.2f;
        [SerializeField] private float _gravity = -90.0f;

        [Header("Air Control"), Space]
        [SerializeField] private float _airSpeed = 15f;
        [SerializeField] private float _airAcceleration = 70f;
        public bool PreventAirClimbing = true;

        [Header("Crouching & Heights"), Space]
        [SerializeField] private float _standHeight = 2.0f;
        [SerializeField] private float _crouchHeight = 1.0f;
        [SerializeField] private float _crouchHeightResponse = 15f;
        [SerializeField, Range(0, 1)] private float _crouchHeightDiminution = 0.5f;

        //A quelles distance entre le bas et le haut de la capsule la cam target est positionnée
        [Range(0, 1), SerializeField] private float _standCameraTargetHeight = 0.9f;
        [Range(0, 1), SerializeField] private float _crouchCameraTargetHeight = 0.7f;

        [SerializeField] private CharacterState _state;
        [SerializeField] private CharacterState _lastState;
        [SerializeField] private CharacterState _tempState;

        private Quaternion _requestedRotation; //send request rotation for the UpdateRotation Method
        private Vector3 _requestedMovement; //send request movement for the UpdateVelocity Method
        private bool _requestedJump;
        private bool _requestedCrouch;
        private bool _requestedCrouchInAir;
        private bool _requestedSprint;
        private float _timeSinceUngrounded;
        private float _timeSinceJumpRequest;
        private bool _ungroundedDueToJump;

        private Collider[] _uncrouchOverlapResults;

        public void Initialize()
        {
            _state.Stance = Stance.Stand;
            _lastState = _state;
            _uncrouchOverlapResults = new Collider[8];
            _motor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput input)
        {
            _requestedRotation = input.Rotation;

            //take 2D input vec and create 3D movement vec on the XZ plane
            _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);

            //Clamp the length to 1 to prevent moving faster diagonally
            _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);

            //orient the input so it's relative to the direction the player is facing
            _requestedMovement = input.Rotation * _requestedMovement;

            var wasRequestingJump = _requestedJump;
            //si le joueur presse Jump et on vérifie lui-même si la méthode est appelée plusieurs fois avant 
            //le prochain tick physique donc on veut le garder le temps de check
            _requestedJump = _requestedJump || input.Jump;

            if(_requestedJump && !wasRequestingJump)
            {
                _timeSinceJumpRequest = 0f;
            }

            _requestedSprint = input.Sprint switch
            {
                SprintInput.None => _requestedSprint,
                SprintInput.Sprinting => true,
                SprintInput.StopSprint => false,
                _ => _requestedSprint
            };

            #region CROUCH
            bool wasRequestingCrouch = _requestedCrouch;

            _requestedCrouch = input.Crouch switch
            {
                CrouchInput.None => _requestedCrouch, //if nothing let it
                CrouchInput.Toggle => !_requestedCrouch, //flip request when toggle
                _ => _requestedCrouch //default
                //you can also here put Hold for crouch like CrounchInput.Crouch => true, CrounchInput.Uncrouch => false
            };

            if(_requestedCrouch && !wasRequestingCrouch)
            {
                _requestedCrouchInAir = !_state.Grounded;
            }
            else if(!_requestedCrouch && wasRequestingCrouch)
            {
                _requestedCrouchInAir = false;
            }
            #endregion
        }

        /// <summary>
        /// Fonction pour set le transform cam target en fonction de notre standing
        /// </summary>
        public void UpdateBody(float deltaTime)
        {
            float currentHeight = _motor.Capsule.height;
            float cameraTargetHeight = currentHeight * (_state.Stance is Stance.Stand ? _standCameraTargetHeight : _crouchCameraTargetHeight);
            _cameraTarget.localPosition = Vector3.Lerp
            (
                a: _cameraTarget.localPosition, 
                b: new Vector3(0f, cameraTargetHeight, 0f),
                //to make framerate independant we can use equation below
                t: 1f - Mathf.Exp(-_crouchHeightResponse * deltaTime) //same as _crouchHeightResponse * deltaTime
            );

            if (_canScaleRoot)
            {
                float normalizedHeight = currentHeight / _standHeight;
                Vector3 rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
                _root.localScale = Vector3.Lerp
                (
                    a: _root.localScale,
                    b: rootTargetScale,
                    1f - Mathf.Exp(-_crouchHeightResponse * deltaTime)
                );
            }
        }

        #region ICharacterController Callbacks

        /// <summary>
        /// Called each physics tick by the character motor
        /// </summary>
        /// <param name="currentRotation"></param>
        /// <param name="deltaTime"></param>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            //Update the character's rotation to face in the same direction as the
            //requested rotation (camera rotation)

            //we don't want the character to pitch / rotate up and down axis (vertical), so the direction of the character
            //looks should always be "flattened"

            //it's done by projecting a vector pointing in the same direction that the player
            //is looking onto a flat ground plane (ProjectOnPlane)

            var forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, _motor.CharacterUp);

            if(forward != Vector3.zero)
                currentRotation = Quaternion.LookRotation(forward, _motor.CharacterUp);
        }

        /// <summary>
        /// Called each physics tick by the character motor
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="deltaTime"></param>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            _state.Acceleration = Vector3.zero;

            if (_motor.GroundingStatus.IsStableOnGround)
            {
                _timeSinceUngrounded = 0f;
                _ungroundedDueToJump = false;
                //Snap the requested movement direction to the angle of the surface
                //the character is currently walking on
                Vector3 groundedMovement = _motor.GetDirectionTangentToSurface
                  (
                  _requestedMovement,
                  _motor.GroundingStatus.GroundNormal
                  ) * _requestedMovement.magnitude; //avoid speed loss when looking up or down

                //MOVE.
                if(_state.Stance == Stance.Stand || _state.Stance == Stance.Crouch)
                {
                    //Crouch -- Calculate responsiveness of movement based on the character's stance
                    float speed = _state.Stance is Stance.Stand
                        ? _walkSpeed
                        : _crouchSpeed;


                    float response = _state.Stance is Stance.Stand
                        ? _walkResponse
                        : _crouchResponse;

                    if (_requestedSprint && _state.Stance == Stance.Stand)
                    {
                        speed = _sprintSpeed;
                        response = _sprintResponse;
                    }

                    //And smoothly move along the ground in that direction
                    Vector3 targetVelocity = groundedMovement * speed;

                    var moveVelocity = Vector3.Lerp
                    (
                        a: currentVelocity,
                        b: targetVelocity,
                        t: 1 - Mathf.Exp(-response * deltaTime)
                    );
                    _state.Acceleration = moveVelocity - currentVelocity;
                    currentVelocity = moveVelocity;
                }
            }
            //else, in the air...
            else
            {
                _timeSinceUngrounded += deltaTime;

                if (_requestedMovement.sqrMagnitude > 0)
                {
                    //planar movement is juste the movement on XZ plan
                    //Requested movement projected onto movement plane with preserved magnitude
                    Vector3 planarMovement = Vector3.ProjectOnPlane
                    (
                        vector: _requestedMovement,
                        planeNormal: _motor.CharacterUp
                    ) * _requestedMovement.magnitude;

                    //current velocity on movement plane
                    //Speed where character is moving by the speed where he wants to move
                    Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane
                    (
                        vector: currentVelocity,
                        planeNormal: _motor.CharacterUp
                    );

                    //calculate movementForce
                    Vector3 movementForce = planarMovement * _airAcceleration * deltaTime;

                    //If moving slower than the max air speed, treat movementForce as a simple steering force
                    if(currentPlanarVelocity.magnitude < _airSpeed)
                    {
                        //Add it to the current planar velocity for a target velocity
                        Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;

                        //Limit target velocity to air speed
                        targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, _airSpeed);

                        movementForce = targetPlanarVelocity - currentPlanarVelocity;
                    }
                    //Otherwise, nerf the movement force when it is in the direction of the current planar velocity
                    //to prevent accelerating further beyong the max air speed
                    else if(Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                    {
                        //Project movement force onto the plane whose normal is the current planar velocity
                        Vector3 constrainedMovementForce = Vector3.ProjectOnPlane
                        (
                            vector: movementForce,
                            planeNormal: currentPlanarVelocity.normalized
                        );

                        movementForce = constrainedMovementForce;
                    }

                    //Prevent Air Climbing steep slopes
                    //so if we can found a grind beyond while in air
                    if (PreventAirClimbing)
                    {
                        if (_motor.GroundingStatus.FoundAnyGround)
                        {
                            //if moving in the same direction in the slope
                            //if true player try to goes in the direction of the ground
                            //and if so he could climp a steepy slope
                            if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                            {
                                //Calculate obstruction normal
                                //Get the perpendicular vector of the ground and character up
                                Vector3 obstructionNormal = Vector3.Cross
                                (
                                    lhs: _motor.CharacterUp,
                                    //Get flattened ground version
                                    rhs: Vector3.Cross
                                    (
                                        _motor.CharacterUp,
                                        _motor.GroundingStatus.GroundNormal
                                    )
                                ).normalized;
                            }
                        }
                    }
                    currentVelocity += movementForce;
                }

                //Gravity
                float effetiveGravity = _gravity;
                float verticalSpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
                currentVelocity += _motor.CharacterUp * effetiveGravity * deltaTime;
            }

            //Jump
            if (_requestedJump)
            {
                bool canCoyoteJump = _timeSinceUngrounded < _coyoteTime && !_ungroundedDueToJump;
                if (_motor.GroundingStatus.IsStableOnGround || canCoyoteJump)
                {
                    _requestedJump = false; //unset jump request
                    _requestedCrouch = false; //unset crouch request
                    _requestedCrouchInAir = false;

                    //Unstick the player from the ground
                    _motor.ForceUnground(time: 0.1f);
                    _ungroundedDueToJump = true;

                    //this part is necessary to avoid a simple slowing when jumping but to cut it and control
                    //Set minimum vertical speed to the jump speed
                    var currentVerticalSpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
                    var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpSpeed);

                    //Add the difference in current and target vertical speed to the character's velocity
                    currentVelocity += _motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                }
                else
                {
                    _timeSinceJumpRequest += deltaTime;

                    //Defer the jump request until coyote time has passed
                    var canJumpLayer = _timeSinceJumpRequest < _coyoteTime;
                    _requestedJump = canJumpLayer;
                }
            }

        }

        /// <summary>
        /// appelée avant que la vélocité, rotations se mettent à jour
        /// </summary>
        /// <param name="deltaTime"></param>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            _tempState = _state;
            //Crouch
            if (_requestedCrouch && _state.Stance == Stance.Stand)
            {
                _state.Stance = Stance.Crouch;
                _motor.SetCapsuleDimensions
                (
                    radius: _motor.Capsule.radius,
                    height: _crouchHeight,
                    yOffset: _crouchHeight * _crouchHeightDiminution
                );
            }
        }

        /// <summary>
        /// Appelée une fois que la vélocité, rotations ... se soient mis à jour
        /// </summary>
        /// <param name="deltaTime"></param>
        public void AfterCharacterUpdate(float deltaTime)
        {
            var totalAcceleration = (_state.Velocity - _lastState.Velocity) / deltaTime;
            _state.Acceleration = Vector3.ClampMagnitude(_state.Acceleration, totalAcceleration.magnitude);

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
                if(_motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
                {
                    _requestedCrouch = true;
                    _motor.SetCapsuleDimensions
                    (
                        radius: _motor.Capsule.radius,
                        height: _crouchHeight,
                        yOffset: _crouchHeight * _crouchHeightDiminution
                    );
                }
                else
                {
                    _state.Stance = Stance.Stand;
                }
            }

            //Update state to reflect relevant motor properties
            _state.Grounded = _motor.GroundingStatus.IsStableOnGround;
            _state.Velocity = _motor.Velocity;

            //And update the _lastState to store the characterState snapshot taken at 
            //the beginning of this character update
            _lastState = _tempState;
        }
        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {

        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {

        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            _state.Acceleration = Vector3.ProjectOnPlane(_state.Acceleration, hitNormal);
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {

        }
        #endregion

        private void OnValidate()
        {
            if(_motor == null)
                _motor = GetComponent<KinematicCharacterMotor>();
        }

        public Transform GetCameraTarget() => _cameraTarget;

        public CharacterState GetCharacterState() => _state;
        public CharacterState GetLastState() => _lastState;

        public void SetPosition(Vector3 position, bool killVelocity = true)
        {
            _motor.SetPosition(position);
            if (killVelocity)
            {
                _motor.BaseVelocity = Vector3.zero;
            }
        }
    }

}
