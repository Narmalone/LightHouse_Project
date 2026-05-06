using Cinemachine;
using Eflatun.SceneReference;
using LightHouse.Core.Interaction;
using LightHouse.Core.Save;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Menu.Notebook
{
    /// <summary>
    /// Gère l'interaction avec le notebook (caméra, UI, navigation).
    /// </summary>
    public class NoteBookMenuController : MonoBehaviour, IClickable
    {
        #region ===== Dependencies =====

        [SerializeField] private Canvas _canvas;
        [SerializeField] private CinemachineVirtualCamera _notebookCamera;
        [SerializeField] private CinemachineBrain _brain;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueLastGameButton;
        [SerializeField] private SceneReference _gameScene;

        #endregion

        #region ===== State =====

        private bool _isInspecting;
        private bool _waitingForBlend;

        private Coroutine _blendRoutine;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            SubscribeEvents();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateBlendState();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            _notebookCamera.Priority = -1;
            SetCanvasVisible(false);
        }

        #endregion

        #region ===== Events =====

        private void SubscribeEvents()
        {
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _continueLastGameButton.onClick.AddListener(OnContinueClicked);
            PlayerControllerMenu.OnRightClickPressed += OnRightClickPressed;
        }

        private void UnsubscribeEvents()
        {
            _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            _continueLastGameButton.onClick.RemoveListener(OnContinueClicked);
            PlayerControllerMenu.OnRightClickPressed -= OnRightClickPressed;
        }

        private void OnRightClickPressed()
        {
            if (!_isInspecting) return;

            ExitNotebook();
        }

        #endregion

        #region ===== Interaction =====

        public void OnClicked()
        {
            EnterNotebook();
        }

        private void EnterNotebook()
        {
            _isInspecting = true;

            SetCameraActive(true);
            StartBlendRoutine();
        }

        private void ExitNotebook()
        {
            StopBlendRoutine();

            _isInspecting = false;
            _waitingForBlend = false;

            SetCanvasVisible(false);
            SetCameraActive(false);
        }

        #endregion

        #region ===== Camera / Blend =====

        private void StartBlendRoutine()
        {
            _blendRoutine = StartCoroutine(WaitBeforeBlend());
        }

        private void StopBlendRoutine()
        {
            if (_blendRoutine == null) return;

            StopCoroutine(_blendRoutine);
            _blendRoutine = null;
        }

        private IEnumerator WaitBeforeBlend()
        {
            yield return new WaitForSeconds(0.5f);

            if (!_isInspecting)
                yield break;

            _waitingForBlend = true;
        }

        private void UpdateBlendState()
        {
            if (!_waitingForBlend) return;

            if (_brain.IsBlending) return;

            _waitingForBlend = false;
            OnBlendCompleted();
        }

        private void OnBlendCompleted()
        {
            SetCanvasVisible(true);
        }

        private void SetCameraActive(bool active)
        {
            _notebookCamera.Priority = active ? 100 : -1;
        }

        #endregion

        #region ===== UI =====

        private void SetCanvasVisible(bool visible)
        {
            _canvas.gameObject.SetActive(visible);
        }

        #endregion

        #region ===== Gameplay =====

        private void OnNewGameClicked()
        {
            SaveLoadSystem.Instance.NewGame();
            BootStrap.Instance.StartGame();
        }

        private void OnContinueClicked()
        {
            //TODO: Lancer le chargement de la dernière partie sauvegardée et charger la scène de jeu
            SaveLoadSystem.Instance.LoadGame(SaveLoadSystem.Instance.GetSaveMetadataList()[0].Name);
            BootStrap.Instance.StartGame();
        }

        #endregion
    }
}