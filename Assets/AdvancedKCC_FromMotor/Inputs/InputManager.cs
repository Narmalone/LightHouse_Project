using UnityEngine;
using UnityEngine.InputSystem;

namespace LightHouse.Inputs
{
    public static class InputManager
    {
        private static PlayerInputActions _player_Input_Actions;
        public static PlayerInputActions PLAYER_INPUTS_ACTIONS => _player_Input_Actions;

        //Fast references to direct acces ton InputActions / Maps
        public static PlayerInputActions.PlayerActions Player => _player_Input_Actions.Player;
        public static PlayerInputActions.UIActions UI => _player_Input_Actions.UI;

        public static InputAction Interact => _player_Input_Actions.Player.Interact;
        public static InputAction Move => _player_Input_Actions.Player.Move;
        public static InputAction Jump => _player_Input_Actions.Player.Jump;
        public static InputAction Crouch => _player_Input_Actions.Player.Crouch;

        public static void SetPlayerInputActions(PlayerInputActions pia)
        {
            if (_player_Input_Actions != null) DisposePlayerInputActions();
            _player_Input_Actions = pia;
        }

        public static void DisposePlayerInputActions()
        {
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
