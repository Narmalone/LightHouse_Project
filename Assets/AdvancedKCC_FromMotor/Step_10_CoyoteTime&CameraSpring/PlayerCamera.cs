using UnityEngine;

namespace Narmalone.AdvancedController.V3.StepTen
{
    [System.Serializable]
    public struct CameraInput
    {
        public Vector2 Look;
    }

    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private float _sensiX = 0.1f, _sensiY = 0.1f;
        [SerializeField] private bool _invertAxis = false;

        //au lieu de rotate le transform directement on utilise cette variable
        //et les eulerangles sont un wrapper dans le transform
        private Vector3 _eulerAngles;
        public void Initialize(Transform target)
        {
            transform.position = target.position;
            transform.eulerAngles = _eulerAngles = target.eulerAngles;
        }

        /// <summary>
        /// Rotation basée sur l'entrée de la souris
        /// </summary>
        public void UpdateRotation(CameraInput input)
        {
            _eulerAngles += new Vector3(!_invertAxis ? -input.Look.y * _sensiY : input.Look.y * _sensiY, input.Look.x * _sensiX);
            transform.eulerAngles = _eulerAngles;
        }

        /// <summary>
        /// Elle dépassera une cible de caméra puis y alignera sa position
        /// </summary>
        /// <param name="target"></param>
        public void UpdatePosition(Transform target)
        {
            transform.position = target.position;
        }
    }

}
