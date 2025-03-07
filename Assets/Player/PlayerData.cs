using System;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public PlayerManager _manager;

    [Header("Events")]
    public Action _eventLockMovement;
    public Action _eventLockCameraMovement;
    public Action _eventUnlockMovement;
    public Action _eventUnlockCameraMovement;
    public Action _eventFreeze;
    public Action _eventUnfreeze;

    [Header("Components")]
    public CharacterController controller;
    public Rigidbody _rb;
    public Transform playerCamera;

    [Header("Drop Item")]
    public float _maxStrengthThrowItem;
    public float _timeToAchieveMaxStrength;
    public AnimationCurve _curveStrengthGrow;

    [Header("Drop Item")]
    [SerializeField] private float _currencyShop = 0;
    [SerializeField] private Action<float> _onCurrencyShopChanged;

    public float CurrencyShop { get { return _currencyShop; } set
        {
            _currencyShop = value;
            _onCurrencyShopChanged?.Invoke(value);
        }
    }

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }
}