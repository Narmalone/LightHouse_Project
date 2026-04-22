using Cinemachine;
using UnityEngine;

public class RadioMenuController : MonoBehaviour, IRaycastable, ILeavableMenuItem
{
    [SerializeField] private BoxCollider _mainCollider;
    [SerializeField] private RadioDial _radioDial;
    [SerializeField] private RadioSystemBinder _radioBinder;
    [SerializeField] private RadioFrequencyController _frequencyController;
    [SerializeField] private RadioAudioController _radioAudioController;
    [SerializeField] private RadioOnOffController _radioOnOffController;
    [SerializeField] private CinemachineVirtualCamera _radioCamera;

    private bool _isPlayerInsideTheRadio = false;

    private void Awake()
    {
        _radioOnOffController.Toggle.IsOnChanged += Toggle_IsOnChanged;
        PlayerControllerMenu.OnRightClickPressed += PlayerControllerMenu_OnRightClickPressed;
    }

    private void PlayerControllerMenu_OnRightClickPressed()
    {
        if (!_isPlayerInsideTheRadio) return;
        OnRadioLeave();
    }

    private void OnDestroy()
    {
        _radioOnOffController.Toggle.IsOnChanged -= Toggle_IsOnChanged;
        PlayerControllerMenu.OnRightClickPressed -= PlayerControllerMenu_OnRightClickPressed;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _radioCamera.Priority = -1;
        DisableRadio();
        OnRadioLeave();
    }

    private void DisableRadio()
    {
        _radioBinder.SetEnable(false);
        _radioAudioController.StopAudio();
        _frequencyController.HideFrequencyText();
    }

    private void EnableRadio()
    {
        _radioBinder.SetEnable(true);
        _radioAudioController.PlayAudio();
        _frequencyController.ShowFrequencyText();
        _radioDial.ForceUpdateValue();
    }

    private void Toggle_IsOnChanged(bool obj)
    {
        //couper ou lancer la radio
        if (obj)
        {
            EnableRadio();
        }
        else
        {
            DisableRadio();
        }
    }

    private void OnRadioInteracted()
    {
        _isPlayerInsideTheRadio = true;
        _mainCollider.enabled = false;
        _radioCamera.Priority = 100;
    }

    private void OnRadioLeave()
    {
        _isPlayerInsideTheRadio = false;
        _mainCollider.enabled = true;
        _radioCamera.Priority = -1;
    }

    public void OnRaycastEnter() { }

    public void OnRaycastLeave() { }

    public void OnClicked() 
    {
        OnRadioInteracted();
    }

    public void OnClickReleased() { }

    public void OnLeaveSend()
    {
        OnRadioLeave();
    }
}
