using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.LEO.Supplies 
{
    [RequireComponent(typeof(Button))]
    public class SupplyCategoryButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private E_SupplyCategory _targetCategory;

        public event Action<E_SupplyCategory> OnSupplyCategoryClicked;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonCliqued);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonCliqued);
        }

        private void OnValidate()
        {
            if (_button == null) _button = GetComponent<Button>();
        }

        private void OnButtonCliqued()
        {
            OnSupplyCategoryClicked?.Invoke(_targetCategory);
        }
    }

}
