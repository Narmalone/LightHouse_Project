using LightHouse.Core.Localization;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightHouse.Core.Inputs
{
    public enum InputNameEnum
    {
        Jump,
        Move,
        Pickup,
        InteractInInventory
    }

    public static class InputManager
    {
        public static bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Vrai dès que Dispose a été appelé, jusqu'à la prochaine Initialize.
        /// Bloque explicitement toute recréation automatique via le getter PIA pendant ce laps de temps.
        /// </summary>
        public static bool IsShuttingDown { get; private set; } = false;

        public static event Action OnInputManagerWillClear;
        private static PlayerInputActions _player_Input_Actions;

        public static PlayerInputActions PIA
        {
            get
            {
                if (!Application.isPlaying) return null;

                // Garde explicite : si on est en train de disposer/déjà disposé,
                // on ne tente JAMAIS de recréer, quel que soit l'état de _player_Input_Actions.
                if (IsShuttingDown) return _player_Input_Actions;

                if (!IsInitialized && _player_Input_Actions == null)
                {
                    Debug.LogWarning("PlayerInputActions n'est pas défini dans InputManager ! Création automatique...");
                    Initialize();
                }

                return _player_Input_Actions;
            }
        }

        public static string Interact_Bind_Name;
        public static string Pickup_Bind_Name;
        public static string Drop_Bind_Name;
        public static string Crouch_Bind_Name;
        public static string Scroll_Bind_Name;
        public static string Select_Bind_Name;
        public static string InteractInInventory_Bind_Name;
        public static string Jump_Bind_Name;
        public static string Move_Bind_Name;

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
            _player_Input_Actions?.Disable();
            _player_Input_Actions?.Dispose();

            _player_Input_Actions = new PlayerInputActions();
            _player_Input_Actions.Enable();
            IsInitialized = true;
            IsShuttingDown = false;
            UpdateAllBindNames(_player_Input_Actions);
        }
        public static void DisposePlayerInputActions()
        {
            IsShuttingDown = true;
            OnInputManagerWillClear?.Invoke();
            IsInitialized = false;
            _player_Input_Actions?.Disable();
            _player_Input_Actions?.Dispose();
        }

        public static string GetBindingName(InputAction action, int bindingIndex = 0)
        {
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.LogWarning("Binding invalide !");
                return "Unknown";
            }

            InputBinding binding = action.bindings[bindingIndex];
            InputControl control = action.controls.Count > 0 ? action.controls[0] : null;

            return control != null ? control.displayName : binding.path;
        }

        public static string GetBindingName(InputNameEnum inputName)
        {
            switch (inputName)
            {
                case InputNameEnum.Jump:
                //return LocalizationManager.Current.GetStringRoutine();
                case InputNameEnum.Move:
                    return Move_Bind_Name;
                case InputNameEnum.Pickup:
                    return Pickup_Bind_Name;
                case InputNameEnum.InteractInInventory:
                    return InteractInInventory_Bind_Name;
                default:
                    return "Unknown";
            }
        }

        public static void GetBindInputName()
        {
        }
    }
}