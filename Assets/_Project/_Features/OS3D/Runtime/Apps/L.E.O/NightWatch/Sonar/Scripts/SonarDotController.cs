using System;
using LightHouse.Features.Sonar.Core;
using LightHouse.Features.UI;

using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.LEO.NightWatch.Sonar
{
    public class SonarDotController : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _dotImage;
        [SerializeField] private UI_CustomButton _myButton;

        private ISonarable _sonarElement;
        public ISonarable SonarElement
        {
            get
            {
                return _sonarElement;
            }
        }

        public Image DotImage => _dotImage;
        public RectTransform RectTransform => _rectTransform;
        private Material _instanceMaterial;

        /// <summary>
        /// Sonar infos, dot image
        /// </summary>
        public event Action<string, Image> SonarDotClicked;
        public event Action<Image> SonarDotForcedUpdated;

        void Awake()
        {
            if (_dotImage != null && _dotImage.material != null)
            {
                _instanceMaterial = new Material(_dotImage.material); // Clone
                _dotImage.material = _instanceMaterial;
                _instanceMaterial.SetColor("_GlowColor", Color.cyan);
            }

            _myButton.OnClick += OnButtonCliqued;
        }

        private void OnButtonCliqued(UI_CustomButton button)
        {
            SonarDotClicked?.Invoke(_sonarElement.SonarInfo, _dotImage);
        }

        private void OnDestroy()
        {
            UnregisterFromSonarable();
            _myButton.OnClick -= OnButtonCliqued;
        }

        public void SetSonarElement(ISonarable sonarElement)
        {
            if (_sonarElement != null)
            {
                UnregisterFromSonarable();
            }
            _sonarElement = sonarElement;
            RegisterToSonarable();
        }

        public void RegisterToSonarable()
        {
            SonarElement.ForceDotUpdate += SonarElement_ForceDotUpdate;
        }

        private void SonarElement_ForceDotUpdate()
        {
            SonarDotForcedUpdated?.Invoke(DotImage);
            SetDotColor(SonarElement.DotColor);
            SetDotSprite(SonarElement.DotSprite);
            SetDotSize(SonarElement.DotSize);
        }

        public void UnregisterFromSonarable()
        {
            SonarElement.ForceDotUpdate -= SonarElement_ForceDotUpdate;
        }

        public void SetDotSize(Vector2 size)
        {
            if (_rectTransform != null)
                _rectTransform.sizeDelta = size;
        }

        public void SetDotSize(float size)
        {
            SetDotSize(new Vector2(size, size));
        }

        public void SetDotRotation(Vector3 eulers)
        {
            if (_rectTransform == null) return;

            float zRotation = -eulers.y; // Inversion pour l'UI
            _rectTransform.localRotation = Quaternion.Euler(0, 0, zRotation);
        }

        public void SetDotColor(Color color)
        {
            if (_instanceMaterial != null)
                _instanceMaterial.SetColor("_GlowColor", color);

            _dotImage.color = color; // (optionnel selon visuel)
        }


        public void SetDotSprite(Sprite dotSprite)
        {
            if (dotSprite == null) return;
            _dotImage.sprite = dotSprite;
        }
    }

}
