using Cinemachine;
using LightHouse.Core.Interaction;
using LightHouse.Features.Computer.OS;
using UnityEngine;

namespace LightHouse.Features.Menu.Computer
{
    /// <summary>
    /// Gère l'entrée et la sortie du joueur dans l'ordinateur (caméra).
    /// </summary>
    public class ComputerMenuController : MonoBehaviour, IClickable
    {
        #region ===== Dependencies =====

        [SerializeField] private CinemachineVirtualCamera _computerCamera;
        [SerializeField] private OS _os;

        #endregion

        #region ===== State =====

        private bool _isActive;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            Initialize();
            _os.OnLeftComputerCalled += ExitComputer;
        }

        private void OnDestroy()
        {
            _os.OnLeftComputerCalled -= ExitComputer;
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            SetCameraActive(false);
        }

        #endregion

        #region ===== Interaction =====

        public void OnClicked()
        {
            EnterComputer();
        }

        private void EnterComputer()
        {
            if (_isActive) return;

            _isActive = true;
            SetCameraActive(true);
            _os.BootOS();
        }

        private void ExitComputer()
        {
            if (!_isActive) return;

            _isActive = false;
            SetCameraActive(false);
            _os.LeaveOS();
        }

        #endregion

        #region ===== Camera =====

        private void SetCameraActive(bool active)
        {
            _computerCamera.Priority = active ? 100 : -1;
        }

        #endregion
    }
}