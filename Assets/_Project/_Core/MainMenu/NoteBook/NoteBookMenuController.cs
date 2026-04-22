using Cinemachine;
using Eflatun.SceneReference;
using LightHouse.Core.Save;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NoteBookMenuController : MonoBehaviour, IRaycastable
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private CinemachineVirtualCamera _noteBookCamera;
    [SerializeField] private CinemachineBrain _brain;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private SceneReference _gameScene;

    private bool _requestedBlendToNotebook = false;
    private bool _isPlayerInspectingNotebook = false;
    private Coroutine _blendToNotebookRoutine = null;

    public void OnClicked() 
    {
        OnNoteBookInteracted();
    }

    public void OnClickReleased() { }

    public void OnRaycastEnter() { }

    public void OnRaycastLeave() { }

    private void Awake()
    {
        _newGameButton.onClick.AddListener(OnNewGameClicked);
        PlayerControllerMenu.OnRightClickPressed += PlayerControllerMenu_OnRightClickPressed;
    }

    private void PlayerControllerMenu_OnRightClickPressed()
    {
        if (!_isPlayerInspectingNotebook) return;
        OnNoteBookLeave();
    }

    private void OnDestroy()
    {
        _newGameButton.onClick.RemoveListener(OnNewGameClicked);
        PlayerControllerMenu.OnRightClickPressed -= PlayerControllerMenu_OnRightClickPressed;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _noteBookCamera.Priority = -1;
        _canvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_requestedBlendToNotebook)
        {
            if(_brain.IsBlending == false)
            {
                //La caméra a fait son chemin et ne blend plus
                _requestedBlendToNotebook = false;
                OnBlendToNoteBookCompleted();
            }
        }
    }

    private void OnNewGameClicked()
    {
        SaveLoadSystem.Instance.NewGame();
        BootStrap.Instance.StartGame();
    }

    private void OnNoteBookInteracted()
    {
        _isPlayerInspectingNotebook = true;
        _noteBookCamera.Priority = 100;
        _blendToNotebookRoutine = StartCoroutine(RequestBlendToNotebookRoutine());
    }

    private IEnumerator RequestBlendToNotebookRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (!_isPlayerInspectingNotebook)
            yield break;
        _requestedBlendToNotebook = true;
    }

    private void OnBlendToNoteBookCompleted()
    {
        _canvas.gameObject.SetActive(true);
    }

    private void OnNoteBookLeave()
    {
        if (_blendToNotebookRoutine != null)
        {
            StopCoroutine(_blendToNotebookRoutine);
            _blendToNotebookRoutine = null;
        }
        _requestedBlendToNotebook = false;
        _isPlayerInspectingNotebook = false;
        _canvas.gameObject.SetActive(false);
        _noteBookCamera.Priority = -1;
    }

}
