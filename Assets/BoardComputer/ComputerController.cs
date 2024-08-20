using Cinemachine;
using UnityEngine;

public class ComputerController : ItemBase
{
    [Header("CONTROLLER")]
    [SerializeField] private UiComputerController _uiComputerController;

    [Header("EVENTS")]
    [SerializeField] private CustomEvent _lockPlayerMovement;
    [SerializeField] private CustomEvent _unlockPlayerMovement;
    [SerializeField] private CustomEvent _lockCamera;
    [SerializeField] private CustomEvent _unlockCamera;

    [Header("COLLIDERS REFS")]
    [SerializeField] private BoxCollider _itemCollider;
    [SerializeField] private CinemachineVirtualCamera _computerCam;
    [SerializeField] private CanvasGroup _mainCanvasGroup;

    [SerializeField] private string _interactName = "Enter";
    public override string Name { get => _interactName; set => _interactName = value; }

    public override void Use()
    {
        base.Use();
        OpenComputer();
    }

    public void OpenComputer()
    {
        _itemCollider.enabled = false;
        _mainCanvasGroup.alpha = 1f;
        _lockPlayerMovement?.Raise();
        _lockCamera?.Raise();
        _computerCam.SetPriority(10);
    }

    public void LeaveComputer()
    {
        _itemCollider.enabled = true;
        _mainCanvasGroup.alpha = 0f;
        _unlockPlayerMovement?.Raise();
        _unlockCamera?.Raise();
        _computerCam.SetPriority(-20);
    }
}

public enum ComputerTabs
{
    Current,
    Meteo,
    Shop,
    Quest,
    IslandInfos,
    Radar
}