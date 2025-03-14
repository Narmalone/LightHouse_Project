using UnityEngine;
using KinematicCharacterController;

namespace Narmalone.AdvancedController.V3.StepTwo
{
    public struct CharacterInput
    {
        public Quaternion Rotation;
    }

    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform cameraTarget;

        private Quaternion _requestedRotation; //send request rotation for the UpdateRotation Method

        public void Initialize()
        {
            motor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput input)
        {
            _requestedRotation = input.Rotation;
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
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }

        /// <summary>
        /// Called each physics tick by the character motor
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="deltaTime"></param>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            
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

        public Transform GetCameraTarget() => cameraTarget;
    }

}
