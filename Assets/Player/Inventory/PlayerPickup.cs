using LightHouse.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private void Start()
    {
        InputManager.PickUp.performed += PickUp_performed;
    }

    private void OnDestroy()
    {
        InputManager.PickUp.performed -= PickUp_performed;
    }

    private void PickUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }
}
