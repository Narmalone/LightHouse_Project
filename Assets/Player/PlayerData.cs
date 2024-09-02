using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public PlayerManager _manager;

    [Header("Events")]
    public CustomEvent _eventLockMovement;
    public CustomEvent _eventLockCameraMovement;
    public CustomEvent _eventUnlockMovement;
    public CustomEvent _eventUnlockCameraMovement;
    public CustomEvent _eventFreeze;
    public CustomEvent _eventUnfreeze;

    [Header("Components")]
    public CharacterController controller;
    public Rigidbody _rb;
    public PlayerInventory playerInventory;
    public Transform playerCamera;

    [Header("Drop Item")]
    public float _maxStrengthThrowItem;
    public float _timeToAchieveMaxStrength;
    public AnimationCurve _curveStrengthGrow;

    [Header("Drop Item")]
    [SerializeField] private float _currencyShop = 0;
    [SerializeField] private CustomEvent_Float _onCurrencyShopChanged;

    public float CurrencyShop { get { return _currencyShop; } set
        {
            _currencyShop = value;
            _onCurrencyShopChanged?.Raise(value);
        }
    }

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }
}