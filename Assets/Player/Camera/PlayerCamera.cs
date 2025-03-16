using UnityEngine;

namespace LightHouse.KinematicCharacterController
{
    [System.Serializable]
    public struct CameraInput
    {
        public Vector2 Look;
    }
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private float _sensiX = 0.1f, _sensiY = 0.1f;
        [SerializeField] private float _maxDownLookAngle = 60f;
        [SerializeField] private float _maxUpLookAngle = -60f;
        [SerializeField] private bool _invertYAxis = false;
        [SerializeField] private bool _invertXAxis = false;

        //au lieu de rotate le transform directement on utilise cette variable
        //et les eulerangles sont un wrapper dans le transform
        private Vector3 _eulerAngles;
        public void Initialize(Transform target)
        {
            transform.position = target.position;
            transform.eulerAngles = _eulerAngles = target.eulerAngles;
        }

        /// <summary>
        /// Rotation bas�e sur l'entr�e de la souris
        /// </summary>
        public void UpdateRotation(ref CameraInput input)
        {
            _eulerAngles.x += !_invertYAxis ? -input.Look.y * _sensiY : input.Look.y * _sensiY;
            _eulerAngles.y += !_invertXAxis ? input.Look.x * _sensiX : -input.Look.x * _sensiX;

            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, _maxUpLookAngle, _maxDownLookAngle);

            transform.eulerAngles = _eulerAngles;
        }

        /// <summary>
        /// Elle d�passera une cible de cam�ra puis y alignera sa position
        /// </summary>
        /// <param name="target"></param>
        public void UpdatePosition(Transform target)
        {
            transform.position = target.position;
        }
    }

}


