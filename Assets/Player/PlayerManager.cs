using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] public PlayerController _controller;
    [SerializeField] public PlayerInventory _inventory;
    [SerializeField] public PlayerInteraction _interaction;
    [SerializeField] public PlayerData _data;

    protected override void Awake()
    {
        _inventory?.Initialize(this);
        _controller?.Initialize(this);
        _interaction?.Initialize(this);
        _data?.Initialize(this);    
    }
}