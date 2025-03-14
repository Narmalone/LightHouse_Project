using UnityEngine;
using KinematicCharacterController;

namespace Narmalone.AdvancedController.V3.StepFour
{
    public struct CharacterInput
    {
        public Quaternion Rotation;
        public Vector2 Move;
        public bool Jump;
    }

    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform cameraTarget;

        [Space]
        [SerializeField] private float _walkSpeed = 20.0f;
        [SerializeField] private float _jumpSpeed = 20.0f;
        [SerializeField] private float _gravity = -90.0f;

        private Quaternion _requestedRotation; //send request rotation for the UpdateRotation Method
        private Vector3 _requestedMovement; //send request movement for the UpdateVelocity Method
        private bool _requestedJump;

        public void Initialize()
        {
            motor.CharacterController = this;
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

            //si le joueur presse Jump et on vérifie lui-même si la méthode est appelée plusieurs fois avant 
            //le prochain tick physique donc on veut le garder le temps de check
            _requestedJump = _requestedJump || input.Jump;
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

            var forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);

            if(forward != Vector3.zero)
                currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }

        /// <summary>
        /// Called each physics tick by the character motor
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="deltaTime"></param>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (motor.GroundingStatus.IsStableOnGround)
            {
                //Snap the requested movement direction to the angle of the surface
                //the character is currently walking on
               Vector3 groundedMovement = motor.GetDirectionTangentToSurface
                 (
                 _requestedMovement,
                 motor.GroundingStatus.GroundNormal
                 ) * _requestedMovement.magnitude; //avoid speed loss when looking up or down
                
                //move along the ground in that direction
                currentVelocity = groundedMovement * _walkSpeed;
            }
            //else, in the air...
            else
            {
                //Gravity
                currentVelocity += motor.CharacterUp * _gravity * deltaTime;
            }

            //Jump
            if (_requestedJump)
            {
                _requestedJump = false;

                //Unstick the player from the ground
                motor.ForceUnground(time: 0f);

                //this part is necessary to avoid a simple slowing when jumping but to cut it and control
                //Set minimum vertical speed to the jump speed
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpSpeed);

                //Add the difference in current and target vertical speed to the character's velocity
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }

        }


        public void AfterCharacterUpdate(float deltaTime)
        {

        }

        public void BeforeCharacterUpdate(float deltaTime)
        {

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
            if(motor == null)
                motor = GetComponent<KinematicCharacterMotor>();
        }

        public Transform GetCameraTarget() => cameraTarget;
    }

}
