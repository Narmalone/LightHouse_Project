using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Narmalone.AdvancedController.V4
{
    public class CameraSpring : MonoBehaviour
    {
        [Min(0.01f), SerializeField] private float _halfLife = 0.075f;
        [Space]
        [SerializeField] private float _frequency = 18f;
        [Space]
        [SerializeField] private float _angularDisplacement = 2.0f;
        [SerializeField] private float _linearDisplacement = 0.05f;

        private Vector3 _springPosition;
        private Vector3 _springVelocity;
        public void Initialize()
        {
            _springPosition = transform.position;
            _springVelocity = Vector3.zero;
        }

        public void UpdateSpring(float deltaTime, Vector3 up)
        {
            transform.localPosition = Vector3.zero;
            Spring(ref _springPosition, ref _springVelocity, transform.position, _halfLife, _frequency, deltaTime);
            Vector3 relativeSpringPosition = _springPosition - transform.position;
            float springHeight = Vector3.Dot(relativeSpringPosition, up);
            transform.localEulerAngles = new Vector3(-springHeight * _angularDisplacement, 0f, 0f);
            transform.localPosition += relativeSpringPosition * _linearDisplacement;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _springPosition);
            Gizmos.DrawSphere(_springPosition, 0.1f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="velocity"></param>
        /// <param name="target"></param>
        /// <param name="halfLife">Si on met 2 par exemple chaque 2 secondes l'oscillation sera amortie de 50%</param>
        /// <param name="frequency"></param>
        /// <param name="timeStep"></param>
        private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
        {
            float dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
            float f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
            float oo = frequency * frequency;
            float hoo = timeStep * oo;
            float hhoo = timeStep * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector3 detX = f * current + timeStep * velocity + hhoo * target;
            Vector3 detV = velocity + hoo * (target - current);
            current = detX * detInv;
            velocity = detV * detInv;
        }
    }

}
