using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public PlayerManager _manager;

    [Header("Components")]
    public CharacterController controller;
    public PlayerInventory playerInventory;
    public Camera playerCamera;

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
