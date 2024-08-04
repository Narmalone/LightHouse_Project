using UnityEditor.Experimental.RestService;
using UnityEngine;

public class PlayerManager : Singleton<MonoBehaviour>
{
    [SerializeField] internal PlayerController _controller;
    [SerializeField] internal PlayerInventory _inventory;
    [SerializeField] internal PlayerInteraction _interaction;
    [SerializeField] internal PlayerData _data;

    protected override void Awake()
    {
        _inventory?.Initialize(this);
        _controller?.Initialize(this);
        _interaction?.Initialize(this);
        _data?.Initialize(this);
    }
}
