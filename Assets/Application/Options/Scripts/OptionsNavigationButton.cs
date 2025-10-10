using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace LightHouse.Game.Options
{
    public class OptionsNavigationButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private OptionCategory targetCategory;
        
        public Button Button => _button;
        public OptionCategory TargetCategory => targetCategory;
        public event Action<OptionsNavigationButton> OnCliqued;

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
            if(this._button == null)
                _button = this.GetComponent<Button>();
        }

        private void OnDestroy()
        {
            this._button.onClick.RemoveListener(OnClicked);
        }
    }
}
