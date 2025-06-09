using Cinemachine;
using UnityEngine;

namespace LightHouse.Game.Computer
{
    public class LightHouseComputerCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _camera;
        public void EnableComputerCamera()
        {
            _camera.Priority = 100;
        }

        public void DisableComputerCamera()
        {
            _camera.Priority = -1;
        }
    }
}
