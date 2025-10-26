using LightHouse.Handlers;
using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Items.Inventory;
using LightHouse.Localization;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class BinocularItem : InventoryItemBase, IInventoryItemUsable, IInventoryItemCallback
{
    //==============================================================
    #region ► Inventory contract
    //==============================================================
    public bool CanBeUsedFromInventory { get; set; } = true;
    public float UseHoldTime { get; set; } = 0.0f;

    public event Action OnItemUsed;
    public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
    public event Action<string> UseTextSlotChanged;
    #endregion

    //==============================================================
    #region ► FOV / Zoom settings
    //==============================================================
    [Header("References")]
    [SerializeField] private MeshRenderer _mesh;

    [Header("Binocular FOV Settings")]
    [Tooltip("FOV le plus large (zoom FAIBLE). Ex: 45")]
    [SerializeField] private float _binocularMinFov = 45f;

    [Tooltip("FOV le plus étroit (zoom FORT). Ex: 15")]
    [SerializeField] private float _binocularMaxFov = 15f;

    [Tooltip("Incrément de FOV par cran de molette")]
    [SerializeField] private float _scrollStep = 1f;

    [Tooltip("FOV courant appliqué au joueur (clampé entre min et max)")]
    [SerializeField] private float _binocularCurrentFov = 30.0f;

    [Header("Debug / Exposition UI")]
    [Tooltip("0 = FOV min (large, peu zoomé), 1 = FOV max (étroit, très zoomé)")]
    [Range(0f, 1f)]
    [SerializeField] private float _zoom01 = 0f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _currentZoomValueText;
    [SerializeField] private Slider _zoomSlider;

    private string _useInInventoryText = string.Empty;
    [SerializeField] private LocalizedString _useText;
    [SerializeField] private LocalizedString _binocularAction;
    public LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;

    /// <summary>
    /// Lecture publique du zoom normalisé [0..1].
    /// 0 = angle large (peu zoomé) ; 1 = angle étroit (très zoomé)
    /// </summary>
    public float Zoom01 => _zoom01;
    #endregion

    //==============================================================
    #region ► State & Refs
    //==============================================================
    private bool _isBinocularModeUsed = false;

    [Tooltip("Canvas overlay (masque / UI jumelles)")]
    [SerializeField] private GameObject _binocularCanvas;
    #endregion

    //==============================================================
    #region ► Unity lifecycle
    //==============================================================
    protected override void Awake()
    {
        base.Awake();
        SlotManager.OnSlotSelectedChanged += SlotManager_OnSlotSelectedChanged;
        InputManager.OnInputManagerWillClear += InputManager_OnInputManagerWillClear;
    }

    protected override void InputManager_OnInitialized()
    {
        base.InputManager_OnInitialized();
        BuildUpBinocularInteractionText();
        InputManager.Player.Scroll.performed += Scroll_performed;
    }

    private void Start()
    {
        // Démarre en FOV large (zoom faible)
        _binocularCurrentFov = _binocularMinFov;
        UpdateZoom01FromFov();
        DisableBinoculars();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SlotManager.OnSlotSelectedChanged -= SlotManager_OnSlotSelectedChanged;
    }
    #endregion

    //==============================================================
    #region ► Input wiring
    //==============================================================

    private async void BuildUpBinocularInteractionText()
    {
        var action = _inventoryTextsDB.Binoculars_On;
        var key = InputManager.InteractInInventory_Bind_Name;
        _useInInventoryText = await InteractionTextBuilder.Build(
            action,
            key,
            _holdToAction
        );
    }
    private void InputManager_OnInputManagerWillClear()
    {
        // Débranche proprement avant reset des actions
        InputManager.OnInputManagerWillClear -= InputManager_OnInputManagerWillClear;
        InputManager.Player.Scroll.performed -= Scroll_performed;
    }

    private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!_isBinocularModeUsed)
            return; // ignore si les jumelles ne sont pas actives

        // Delta molette : Vector2 (x,y) -> on lit y
        float scrollY = ctx.ReadValue<Vector2>().y;

        if (scrollY > 0f)
        {
            // Molette vers le haut -> zoom avant -> réduire FOV
            SetBinocularFov(_binocularCurrentFov - _scrollStep);
        }
        else if (scrollY < 0f)
        {
            // Molette vers le bas -> zoom arrière -> augmenter FOV
            SetBinocularFov(_binocularCurrentFov + _scrollStep);
        }
    }
    #endregion

    //==============================================================
    #region ► Inventory / Slot
    //==============================================================
    private void SlotManager_OnSlotSelectedChanged()
    {
        if (_isBinocularModeUsed)
            DisableBinoculars();
    }

    public void InvokeOnCanBeUsedFromInventoryChanged()
    {
        CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
    }

    public string UseTextSlot() => _useInInventoryText;

    public void UseFromInventory()
    {
        OnItemUsed?.Invoke();

        if (_binocularCanvas.activeInHierarchy)
            DisableBinoculars();
        else
            EnableBinoculars();
    }
    #endregion

    //==============================================================
    #region ► Public controls (Enable/Disable)
    //==============================================================
    public void EnableBinoculars()
    {
        if (PlayerHandlerData.MainPlayer != null)
        {
            SetBinocularFov(_binocularCurrentFov);
            PlayerHandlerData.MainPlayer.Inventory.FreezeScrollingInventory = true;
            PlayerHandlerData.MainPlayer.Inventory.Hide();
        }

        _isBinocularModeUsed = true;
        if (_binocularCanvas) _binocularCanvas.SetActive(true);
        _mesh.enabled = false;
    }

    public void DisableBinoculars()
    {
        if (PlayerHandlerData.MainPlayer != null)
        {
            // Restaure le FOV par défaut joueur
            SetPlayerFOV(PlayerHandlerData.MainPlayer.PlayerCamera.DefaultFOV);
            PlayerHandlerData.MainPlayer.Inventory.FreezeScrollingInventory = false;
            PlayerHandlerData.MainPlayer.Inventory.Show();
        }

        _isBinocularModeUsed = false;
        if (_binocularCanvas) _binocularCanvas.SetActive(false);
        _mesh.enabled = true;
    }
    #endregion

    #region Inventory Callbacks

    public void OnItemAddedToInventory() { }

    public void OnItemRemovedFromInventory()
    {
        if (_isBinocularModeUsed)
            DisableBinoculars();
    }

    #endregion

    //==============================================================
    #region ► FOV helpers
    //==============================================================
    /// <summary>
    /// Définit le FOV des jumelles (clamp + application caméra + update du ratio 0..1).
    /// </summary>
    private void SetBinocularFov(float newFov)
    {
        _binocularCurrentFov = Mathf.Clamp(newFov, _binocularMaxFov, _binocularMinFov);
        UpdateZoom01FromFov();
        SetPlayerFOV(_binocularCurrentFov);
    }

    /// <summary>
    /// Applique un FOV à la caméra du joueur.
    /// </summary>
    public void SetPlayerFOV(float fov)
    {
        if (PlayerHandlerData.MainPlayer != null)
        {
            PlayerHandlerData.MainPlayer.PlayerCamera.SetFov(fov);
        }
    }

    /// <summary>
    /// Met à jour le ratio _zoom01 en fonction du FOV courant et des bornes.
    /// 0 = FOV min (large) ; 1 = FOV max (étroit).
    /// </summary>
    private void UpdateZoom01FromFov()
    {
        // Mapping : minFOV (large) -> 0  ;  maxFOV (étroit) -> 1
        // t = (min - current) / (min - max)
        float denom = Mathf.Max(0.0001f, (_binocularMinFov - _binocularMaxFov));
        _zoom01 = Mathf.Clamp01((_binocularMinFov - _binocularCurrentFov) / denom);
        UpdateUI(_zoom01);
    }

    private void UpdateUI(float normalizedValue)
    {
        _zoomSlider.value = normalizedValue;
        _currentZoomValueText.text = Mathf.RoundToInt(normalizedValue * 100).ToString() + "%";
    }

    
    #endregion
}
