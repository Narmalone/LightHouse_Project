using Cinemachine;
using System.Collections;
using UnityEngine;

#region ENUMS
public enum ComputerTabs
{
    None,
    Current,
    Meteo,
    Shop,
    Quest,
    IslandInfos,
    Radar
}
#endregion

public class ComputerController : ElectricItem
{

    #region SERIALIZED FIELDS
    [Header("CONTROLLER")]
    [SerializeField] private UiComputerController _uiComputerController;

    [SerializeField] private Renderer _screenButton;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent _onComputerOpen;
    [SerializeField] private CustomEvent _onComputerLeft;

    [Header("LISTENERS")]
    [Header("Player Movements")]
    [SerializeField] private CustomEvent _lockPlayerMovement;
    [SerializeField] private CustomEvent _unlockPlayerMovement;

    [Header("Player Cam")]
    [SerializeField] private CustomEvent _lockCamera;
    [SerializeField] private CustomEvent _unlockCamera;

    [Header("Buttons & Display")]
    [SerializeField] private CustomEvent _onLeftButtonCliqued;

    [Header("COLLIDERS REFS")]
    [SerializeField] private BoxCollider _itemCollider;
    [SerializeField] private CinemachineVirtualCamera _computerCam;
    [SerializeField] private CanvasGroup _mainCanvasGroup;

    [SerializeField] private string _interactName = "Enter";
    #endregion
    public override string Name { get => _interactName; set => _interactName = value; }

    private float _intensityPower = 0f;
    private float _minIntensity = 0.2f;
    private float _maxIntensity = 1f;
    private float _speed = 1f; // adjust the speed of the pulsing effect

    void Start()
    {
        StartCoroutine(PulseRoutine());

        //_screenButton.material.SetFloat("_EmissiveIntensity", _maxIntensity);
        //_screenButton.material.SetFloat("_EmissiveExposureWeight", 1f);

    }

    private float AnimDuration = .5f;

    IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return DecreaseIntensity();
            yield return IncreaseIntensity();
        }
    }


    IEnumerator DecreaseIntensity()
    {
        float time = 0.0f;

        while (time < AnimDuration)
        {
            time += Time.deltaTime * _speed;

            _intensityPower = Mathf.Lerp(_maxIntensity, _minIntensity, time / AnimDuration);
            _screenButton.material.SetFloat("_EmissiveExposureWeight", _intensityPower);

            yield return null;
        }
    }

    IEnumerator IncreaseIntensity()
    {
        float time = 0.0f;

        while (time < AnimDuration)
        {
            time += Time.deltaTime * _speed;

            _intensityPower = Mathf.Lerp(_minIntensity, _maxIntensity, time / AnimDuration);
            _screenButton.material.SetFloat("_EmissiveExposureWeight", _intensityPower);

            yield return null;
        }
    }

    #region MONO CALLBACKS
    private void Awake()
    {
        _onLeftButtonCliqued.handle += _onLeftButtonCliqued_handle;
    }

    private void OnDestroy()
    {
        _onLeftButtonCliqued.handle -= _onLeftButtonCliqued_handle;
    }
    #endregion

    private void _onLeftButtonCliqued_handle()
    {
        LeaveComputer();
    }

    public override void Use()
    {
        base.Use();
        OpenComputer();
    }

    private void Update()
    {
        
    }

    #region ELEC DELEGATES

    public override void OnElecEnabled()
    {
        
    }

    public override void OnElecDisabled()
    {
        
    }

    #endregion

    #region COMPUTER FUNCS

    public void OpenComputer()
    {
        _itemCollider.enabled = false;
        _mainCanvasGroup.alpha = 1f;
        _lockPlayerMovement?.Raise();
        _lockCamera?.Raise();
        _computerCam.SetPriority(10);
        _onComputerOpen?.Raise();
    }

    public void LeaveComputer()
    {
        _itemCollider.enabled = true;
        _mainCanvasGroup.alpha = 0f;
        _unlockPlayerMovement?.Raise();
        _unlockCamera?.Raise();
        _computerCam.SetPriority(-20);
        _onComputerLeft?.Raise();
    }
    #endregion
}

