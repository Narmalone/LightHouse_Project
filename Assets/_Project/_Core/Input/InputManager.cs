using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightHouse.Core.Inputs
{
    public static class InputManager
    {
        public static bool IsInitialized { get; private set; } = false; 
        public static event Action OnInputManagerWillClear;
        private static PlayerInputActions _player_Input_Actions;
        public static PlayerInputActions PLAYER_INPUTS_ACTIONS
        {
            get
            {
                if (!InputManager.IsInitialized && _player_Input_Actions == null)
                {
                    Debug.LogWarning("PlayerInputActions n'est pas défini dans InputManager ! Création automatique...");
                    Initialize();
                }
                return _player_Input_Actions;
            }
        }

        //Fast references to direct acces ton InputActions / Maps
        public static PlayerInputActions.PlayerActions Player => PLAYER_INPUTS_ACTIONS.Player;
        public static PlayerInputActions.UIActions UI => PLAYER_INPUTS_ACTIONS.UI;

        public static InputAction Interact => Player.Interact;
        public static InputAction Select => Player.Select;
        public static InputAction InteractInInventory => Player.InteractInInventory;
        public static InputAction Move => Player.Move;
        public static InputAction Jump => Player.Jump;
        public static InputAction Crouch => Player.Crouch;
        public static InputAction PickUp => Player.Pickup;
        public static InputAction Drop => Player.Drop;
        public static InputAction Scroll => Player.Scroll;

        public static string Interact_Bind_Name;
        public static string Pickup_Bind_Name;
        public static string Drop_Bind_Name;
        public static string Crouch_Bind_Name;
        public static string Scroll_Bind_Name;
        public static string Select_Bind_Name;
        public static string InteractInInventory_Bind_Name;
        public static string Jump_Bind_Name;
        public static string Move_Bind_Name;

        public static void SetPlayerInputActions(PlayerInputActions pia)
        {
            if (_player_Input_Actions != null) DisposePlayerInputActions();
            _player_Input_Actions = pia;
            UpdateAllBindNames(pia);
        }

        public static void UpdateAllBindNames(PlayerInputActions pia)
        {
            Interact_Bind_Name = GetBindingName(pia.Player.Interact);
            Pickup_Bind_Name = GetBindingName(pia.Player.Pickup);
            Drop_Bind_Name = GetBindingName(pia.Player.Drop);
            Crouch_Bind_Name = GetBindingName(pia.Player.Crouch);
            Scroll_Bind_Name = GetBindingName(pia.Player.Scroll);
            Select_Bind_Name = GetBindingName(pia.Player.Select);
            InteractInInventory_Bind_Name = GetBindingName(pia.Player.InteractInInventory);
            Jump_Bind_Name = GetBindingName(pia.Player.Jump);
            Move_Bind_Name = GetBindingName(pia.Player.Move);
        }

        public static void Initialize()
        {
            _player_Input_Actions = new PlayerInputActions();
            _player_Input_Actions.Enable();
            IsInitialized = true;
        }

        public static void DisposePlayerInputActions()
        {
            OnInputManagerWillClear?.Invoke();
            IsInitialized = false;
            _player_Input_Actions?.Dispose();
            _player_Input_Actions = null;
        }

        public static string GetBindingName(InputAction action, int bindingIndex = 0)
        {
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.LogWarning("Binding invalide !");
                return "Unknown";
            }

            //Take inputs datas and values to have the bind Name
            InputBinding binding = action.bindings[bindingIndex];
            InputControl control = action.controls.Count > 0 ? action.controls[0] : null;

            //Get the display name == "E" for example -> binding path == "<Keyboard>/e"
            return control != null ? control.displayName : binding.path;
        }


    }

}
