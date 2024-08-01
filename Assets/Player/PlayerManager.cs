using UnityEditor.Experimental.RestService;
using UnityEngine;

public class PlayerManager : Singleton<MonoBehaviour>
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private PlayerData _data;

}
