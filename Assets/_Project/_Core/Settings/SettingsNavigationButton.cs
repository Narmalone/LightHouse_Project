using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Core.Settings
{
    public class SettingsNavigationButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private CategorySetting _targetCategory;

        public Button Button => _button;
        public CategorySetting TargetCategory => _targetCategory;
        public event Action<SettingsNavigationButton> OnCliqued;

        private void Awake()
        {
            this._button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            OnCliqued?.Invoke(this);
        }

        private void OnValidate()
        {
            if (this._button == null)
                _button = this.GetComponent<Button>();
        }

        private void OnDestroy()
        {
            this._button.onClick.RemoveListener(OnClicked);
        }
    }
}