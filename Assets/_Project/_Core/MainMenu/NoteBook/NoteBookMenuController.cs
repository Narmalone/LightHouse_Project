using Eflatun.SceneReference;
using LightHouse.Core.Save;
using UnityEngine;
using UnityEngine.UI;

public class NoteBookMenuController : MonoBehaviour
{
    [SerializeField] private Canvas _choiceCanvas;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private RaycastableMenuItem _raycastableMenuItem;
    [SerializeField] private SceneReference _gameScene;

    private void Awake()
    {
        _raycastableMenuItem.OnShowInformationsEvent += RaycastableMenuItem_OnShowInformationsEvent;
        _raycastableMenuItem.OnHideInformationsEvent += RaycastableMenuItem_OnHideInformationsEvent;
        _newGameButton.onClick.AddListener(NewGameRoutine);
    }

    private void Start()
    {
        _choiceCanvas.gameObject.SetActive(false);
    }

    private void RaycastableMenuItem_OnHideInformationsEvent()
    {
        _choiceCanvas.gameObject.SetActive(false);
    }

    private void RaycastableMenuItem_OnShowInformationsEvent()
    {
        _choiceCanvas.gameObject.SetActive(true);
    }

    private void NewGameRoutine()
    {
        //Start New Game
        SaveLoadSystem.Instance.NewGame();
        BootStrap.Instance.StartGame();
    }

    private void OnDestroy()
    {
        _raycastableMenuItem.OnShowInformationsEvent -= RaycastableMenuItem_OnShowInformationsEvent;
        _raycastableMenuItem.OnHideInformationsEvent -= RaycastableMenuItem_OnHideInformationsEvent;
        _newGameButton.onClick.RemoveListener(NewGameRoutine);
    }

}
