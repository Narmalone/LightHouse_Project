using LightHouse.Features.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightHouse.Features.Computer.OS
{
    public class ShortcutButtonsManager : MonoBehaviour
    {
        [field: SerializeField] public List<UI_CustomButton> Buttons { get; private set; }

        private UI_CustomButton _lastCliquedButton;

        private void Awake()
        {
            Buttons = GetComponentsInChildren<UI_CustomButton>().ToList();
            foreach (var button in Buttons)
            {
                button.OnClick += Btn_OnClick;
            }
        }

        public void Register(UI_CustomButton btn)
        {
            if (Buttons.Contains(btn))
            {
                //Debug.LogWarning("Le bouton existait déjà");
                return;
            }
            Buttons.Add(btn);
            btn.OnClick += Btn_OnClick;
        }

        public void Unregister(UI_CustomButton btn)
        {
            btn.OnClick -= Btn_OnClick;
            Buttons.Remove(btn);
        }

        private void Btn_OnClick(UI_CustomButton cliquedBtn)
        {
            if (cliquedBtn != null && cliquedBtn.PreventSelection)
            {
                return;
            }

            SwitchSelectedButton(cliquedBtn);
        }

        /// <summary>
        /// The button you want to be selected
        /// </summary>
        /// <param name="selectButton"> if "null" it just unselect the last, or put the ref of the shortcut button you want to select </param>
        public void SwitchSelectedButton(UI_CustomButton selectButton)
        {
            // Si on clique sur le même bouton → toggle OFF
            if (_lastCliquedButton != null && _lastCliquedButton == selectButton)
            {
                _lastCliquedButton.Unselect();
                _lastCliquedButton = null;
                return;
            }

            // Désélectionne l'ancien
            if (_lastCliquedButton != null)
            {
                _lastCliquedButton.Unselect();
            }

            // Si null → clear
            if (selectButton == null)
            {
                _lastCliquedButton = null;
                return;
            }

            // Nouvelle sélection
            _lastCliquedButton = selectButton;
            _lastCliquedButton.Select();
        }

        public void ForceUnselect()
        {
            Debug.Log(_lastCliquedButton);
            if (_lastCliquedButton != null)
            {
                _lastCliquedButton.Unselect();
                _lastCliquedButton = null;
            }
        }

        private void OnDestroy()
        {
            for (int i = Buttons.Count - 1; i >= 0; i--)
            {
                var btn = Buttons[i];
                if (btn != null) btn.OnClick -= Btn_OnClick;
            }
            Buttons.Clear();
            _lastCliquedButton = null;
        }
    }

}
