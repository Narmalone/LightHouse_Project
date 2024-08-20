using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public PlayerManager _manager;

    [Header("Events")]
    public CustomEvent _eventLockMovement;
    public CustomEvent _eventLockCameraMovement;
    public CustomEvent _eventUnlockMovement;
    public CustomEvent _eventUnlockCameraMovement;

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
    public float _currencyShop = 0;

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }
}
