using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

public class SpottingBoatView : ItemBase
{
    [Header("Events")]
    [SerializeField] private CustomEvent _eventActiveSpottingView;
    [SerializeField] private CustomEvent _eventFreezePlayer;
    [SerializeField] private CustomEvent _eventUnfreezePlayer;
    [SerializeField] private CustomEvent_Color _eventSetFadeColor;
    [SerializeField] private CustomEvent_2Float _eventFade;

    [Header("Components")]
    [SerializeField] private CinemachineVirtualCamera _cameraView;

    [Header("Stats")]
    [SerializeField] private float zoomFOV;
    [SerializeField] private float timeSwitchFOV;
    [SerializeField] private float lookXLimit;

    [Header("Fade")]
    [SerializeField] private Color _fadeColor;
    [SerializeField] private float _fadeInDuration;
    [SerializeField] private float _fadeDuration;


    private Coroutine _coroutineFadeFOV;

    private Transform transformCamera;
    private Transform parentTransformCamera;
    private PIA _inputs;

    private Vector2 lookInput;
    private float currentLookSpeedMultiplier;
    private float lookZoomMultiplier;
    private float rotationX;
    private float lookSpeed;
    private float initialFOV;
    private bool enableInteractInput;
    private bool _isActive;
    private bool IsActive
    {
        get { return _isActive; }
        set 
        {
            var targetCameraPriority = 100;
            if (value)
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
                isUsable = false;
                _eventFreezePlayer.Raise();
            }
            else
            {
                targetCameraPriority = -1;
                isUsable = true;
                _eventUnfreezePlayer.Raise();
            }
            
            SwitchCameraView(targetCameraPriority);
            _isActive = value; 
        }
    }

    private void Awake()
    {
        Name = "Spotting Boat";

        _inputs = new PIA();
        _inputs.Enable();

        _inputs.Game.Look.performed += OnLook;
        _inputs.Game.Interact.performed += OnInteract;
        _inputs.Game.UseInInventory.performed += OnZoom;

        _eventActiveSpottingView.handle += OnActiveView;

        transformCamera = _cameraView.transform;
        parentTransformCamera = transformCamera.parent;
    }

    private void Start()
    {
        IsActive = false;
        enableInteractInput = true;
        lookSpeed = PlayerManager.Instance._controller.lookSpeed;
        initialFOV = _cameraView.m_Lens.FieldOfView;
        lookZoomMultiplier = zoomFOV / initialFOV;
        currentLookSpeedMultiplier = 1;
    }

    private void Update()
    {
        if (IsActive == false) return;

        HandleLook();
    }

    private void OnDestroy()
    {
        _inputs.Disable();

        _inputs.Game.Look.performed -= OnLook;
        
        _eventActiveSpottingView.handle -= OnActiveView;
    }

    public override bool Use()
    {
        if (enableInteractInput == false || IsActive) return false;
        IsActive = true;

        OnEnter();
        HandleFade();
        HandleInteraction();

        return false;
    }
    private void OnActiveView()
    {
        Use();
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        if (IsActive)
        {
            ToggleZoom();
            return;
        }
    }
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (enableInteractInput == false) return;
        if (IsActive)
        {
            OnExit();
            HandleFade();
            HandleInteraction();
            return;
        }
    }

    private void OnEnter()
    {
        currentLookSpeedMultiplier = 1;
        _cameraView.m_Lens.FieldOfView = initialFOV;
    }

    private void OnExit()
    {
        IsActive = false;
        gameObject.layer = LayerMask.NameToLayer("Items");
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        if (IsActive == false)
        {
            lookInput = Vector2.zero;
            return;
        }
        lookInput = context.ReadValue<Vector2>();
    }

    private void ToggleZoom()
    {
        if (_coroutineFadeFOV != null) return;
        _coroutineFadeFOV = StartCoroutine(FadeFOV());
;    }

    private void SwitchCameraView(int targetCameraPriority)
    {
        _cameraView.Priority = targetCameraPriority;
    }

    private void HandleLook()
    {
        rotationX += -lookInput.y * lookSpeed * currentLookSpeedMultiplier;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        transformCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
        parentTransformCamera.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed * currentLookSpeedMultiplier, 0);
    }

    private void HandleFade()
    {
        _eventSetFadeColor.Raise(_fadeColor);
        _eventFade.Raise(_fadeInDuration, _fadeDuration);
    }
    
    private void HandleInteraction()
    {
        enableInteractInput = false;
        DOVirtual.DelayedCall(_fadeInDuration + _fadeDuration, () => { enableInteractInput = true; });
    }

    IEnumerator FadeFOV()
    {
        var FOV = _cameraView.m_Lens.FieldOfView;
        var targetFOV = FOV == initialFOV ? zoomFOV : initialFOV;
        currentLookSpeedMultiplier = targetFOV == initialFOV ? 1 : lookZoomMultiplier;
        var time = 0f;

        while (time < timeSwitchFOV)
        {
            time += Time.deltaTime;
            _cameraView.m_Lens.FieldOfView = Mathf.Lerp(FOV, targetFOV, time / timeSwitchFOV);
            yield return null;
        }

        _coroutineFadeFOV = null;
    }
}