using Cinemachine;
using System.Collections;
using UnityEngine;

#region ENUMS
public enum ComputerTabs
{
    None,
    Messagerie,
    Meteo,
    VeilleDeNuit,
    Maintenance,
    Ravitaillement
}
#endregion

public class ComputerController : ElectricItem
{
    //système pour empecher le joueur d'aller au pc quand pas d'électricité
    //revoir l'ui SHOP et commencer à faire tout le reste OH GOD
    private const string EMISSIVE_EXPOSUREWEIGHT_KEY = "_EmissiveExposureWeight";

    #region SERIALIZED FIELDS
    [Header("CONTROLLER")]
    [SerializeField] public UiComputerController _uiComputerController;
    [SerializeField] private BoxCollider _boxCollider;

    [Header("COMPUTER BUTTON")]
    [SerializeField] private Renderer _screenButton;
    //obligé d'utiliser un systeme de mat pour l'instant car l'uptade des materials marche pas très bien
    [SerializeField] private Material _offMaterial;
    [SerializeField] private Material _onMaterial;
    [SerializeField] private float _animDuration = .5f;
    [SerializeField, Range(0, 1f)] private float _minIntensity = 0.2f;
    [SerializeField, Range(0, 1f)] private float _maxIntensity = 1f;
    [SerializeField] private float _screenMotionSpeed = 1f;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent _onComputerOpen;
    [SerializeField] private CustomEvent _inventoryHide;
    [SerializeField] private CustomEvent _inventoryShow;
    [SerializeField] private CustomEvent _onComputerLeft;
    [SerializeField] private CustomEvent _crosshairShow;
    [SerializeField] private CustomEvent _crosshairHide;

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

    [SerializeField] private string _interactName = "Enter";

    private Material _instanceMat;
    private bool _isInComputer = false;

    #endregion

    #region PROPERTIES
    public override string Name { get => _interactName; set => _interactName = value; }

    #endregion

    #region PRIVATE VAR

    //Pulse routine / anim button
    private float _intensityPower = 0f;
    private bool _isScreenButtonPlaying = false;
    private IEnumerator _changeEmissiveRoutine;

    #endregion

    #region MONO CALLBACKS
    private void Awake()
    {
        _onLeftButtonCliqued.handle += _onLeftButtonCliqued_handle;
        _changeEmissiveRoutine = PulseRoutine();
    }

    private void OnDestroy()
    {
        _onLeftButtonCliqued.handle -= _onLeftButtonCliqued_handle;
    }

    private void Start()
    {
        _uiComputerController.SwitchTab(ComputerTabs.Messagerie);
    }

    #endregion

    #region UI DELEGATES

    private void _onLeftButtonCliqued_handle()
    {
        LeaveComputer();
    }

    #endregion

    #region OVERRIDES ITEM BASE
    public override bool Use()
    {
        base.Use();
        OpenComputer();

        return false;
    }
    #endregion

    #region COROUTINES
    IEnumerator PulseRoutine()
    {
        float time = 0.0f;
        bool isIncreasing = true;

        while (_isScreenButtonPlaying)
        {
            while (time < _animDuration)
            {
                time += Time.deltaTime * _screenMotionSpeed;

                if (isIncreasing)
                {
                    _intensityPower = Mathf.Lerp(_minIntensity, _maxIntensity, time / _animDuration);
                }
                else
                {
                    _intensityPower = Mathf.Lerp(_maxIntensity, _minIntensity, time / _animDuration);
                }

                _instanceMat?.SetFloat(EMISSIVE_EXPOSUREWEIGHT_KEY, _intensityPower);

                yield return null;
            }

            time = 0.0f;
            isIncreasing = !isIncreasing;
        }
    }
    #endregion

    #region ELEC DELEGATES

    public override void OnElecEnabled()
    {
        _boxCollider.enabled = false;
        _isInComputer = true;
        if (_instanceMat == null)
        {
            _screenButton.material = _onMaterial;
            _instanceMat = _screenButton.material;
        }
        else
        {
            _screenButton.material = _instanceMat;
        }

        _isScreenButtonPlaying = true;
        StartCoroutine(_changeEmissiveRoutine);
    }

    public override void OnElecDisabled()
    {
        _isScreenButtonPlaying = false;
        StopCoroutine(_changeEmissiveRoutine);
        _boxCollider.enabled = true;
        DisableButton();
        _uiComputerController.Hide();

        if (_isInComputer)
        {
            LeaveComputer();
        }
        _isInComputer = false;
    }

    public void DisableButton()
    {
        _screenButton.material = _offMaterial;
    }

    #endregion

    #region COMPUTER FUNCS

    public void OpenComputer()
    {
        _itemCollider.enabled = false;
        _uiComputerController.Show();

        _inventoryHide?.Raise();
        _crosshairHide?.Raise();
        _lockPlayerMovement?.Raise();
        _lockCamera?.Raise();
        _computerCam.SetPriority(10);
        _onComputerOpen?.Raise();
    }

    public void LeaveComputer()
    {
        _itemCollider.enabled = true;
        _uiComputerController.Hide();
        _inventoryShow?.Raise();
        _crosshairShow?.Raise();
        _unlockPlayerMovement?.Raise();
        _unlockCamera?.Raise();
        _computerCam.SetPriority(-20);
        _onComputerLeft?.Raise();
    }
    #endregion
}

