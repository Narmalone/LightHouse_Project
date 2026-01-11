using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Core.Settings
{
    public class OptionEnum : MonoBehaviour
    {
        public List<string> Choices = new List<string>();
        public int CurrentChoiceIndex;
        public bool EnableConstraints;

        [SerializeField] private TextMeshProUGUI _choiceText;

        public string CurrentSelectedOption =>
            (Choices != null && Choices.Count > 0 && CurrentChoiceIndex >= 0 && CurrentChoiceIndex < Choices.Count)
            ? Choices[CurrentChoiceIndex] : string.Empty;

        public event Action<int> OnValueChanged;

        public Button LeftButton;
        public Button RightButton;

        public bool showDebugGui = true;                 // toggle dans l’inspector
        public Vector2 debugOrigin = new Vector2(10, 10); // position (x,y) de l’overlay

        private void OnGUI()
        {
            if (!showDebugGui) return;

            int x = (int)debugOrigin.x;
            int y = (int)debugOrigin.y;
            const int w = 520;

            GUI.Label(new Rect(x, y + 0, w, 20), "[OptionEnum]");
            GUI.Label(new Rect(x, y + 20, w, 20), $"Choices.Count: {Choices?.Count ?? 0}");
            GUI.Label(new Rect(x, y + 40, w, 20), $"CurrentChoiceIndex: {CurrentChoiceIndex}");
            GUI.Label(new Rect(x, y + 60, w, 20), $"CurrentSelectedOption: {CurrentSelectedOption}");
        }


        private void Awake()
        {
            if (LeftButton) LeftButton.onClick.AddListener(LeftButton_OnClick);
            if (RightButton) RightButton.onClick.AddListener(RightButton_OnClick);
        }

        // ❗️Ne pas forcer l’index ici — le controller s’occupe de l’initialisation
        private void Start()
        {
            ForceRebuildUI(); // juste rafraîchir l’affichage si Choices déjà set
            if (EnableConstraints) CheckConstraints(CurrentChoiceIndex);
        }

        private void RightButton_OnClick() => SetSelectedIndex(CurrentChoiceIndex + 1);
        private void LeftButton_OnClick() => SetSelectedIndex(CurrentChoiceIndex - 1);

        public void SetSelectedIndex(int index)
        {
            index = Wrap(index);
            CurrentChoiceIndex = index;
            _choiceText.text = CurrentSelectedOption;

            if (EnableConstraints) CheckConstraints(index);
            OnValueChanged?.Invoke(index);
        }

        public void SetValueWithoutNotify(int index)
        {
            index = Wrap(index);
            CurrentChoiceIndex = index;
            _choiceText.text = CurrentSelectedOption;

            if (EnableConstraints) CheckConstraints(index);
            // pas d'Invoke ici
        }

        public void ForceRebuildUI()
        {
            if (_choiceText) _choiceText.text = CurrentSelectedOption;
        }

        private int Wrap(int index)
        {
            int count = Choices?.Count ?? 0;
            if (count == 0) return 0;

            if (index < 0) index = count - 1;
            else if (index >= count) index = 0;

            return index;
        }

        public void CheckConstraints(int index)
        {
            int count = Choices?.Count ?? 0;

            if (!LeftButton || !RightButton) return;

            if (count <= 1)
            {
                LeftButton.interactable = false;
                RightButton.interactable = false;
                return;
            }

            // Si tu veux des bords durs, garde ça; sinon enlève EnableConstraints pour wrap infini.
            if (index <= 0)
            {
                LeftButton.interactable = false;
                RightButton.interactable = true;
            }
            else if (index >= count - 1)
            {
                LeftButton.interactable = true;
                RightButton.interactable = false;
            }
            else
            {
                LeftButton.interactable = true;
                RightButton.interactable = true;
            }
        }

        private void OnDestroy()
        {
            if (LeftButton) LeftButton.onClick.RemoveListener(LeftButton_OnClick);
            if (RightButton) RightButton.onClick.RemoveListener(RightButton_OnClick);
        }

        public void AddOptions(List<string> optionsToAdd)
        {
            Choices.AddRange(optionsToAdd);
            ForceRebuildUI();
            if (EnableConstraints) CheckConstraints(CurrentChoiceIndex);
        }
    }

}
