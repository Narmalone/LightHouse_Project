using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public PlayerManager _manager;

    public CharacterController controller;
    public ItemOptionController optionController;
    public PlayerInventory playerInventory;
    public Camera playerCamera;


    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }
}
