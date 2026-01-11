using LightHouse.Core.Extensions;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.LEO.Weather.Temperature
{
    #region Data

    /// <summary>
    /// Conseil contextuel affiché selon une plage de températures.
    /// </summary>
    [System.Serializable]
    public struct TemperatureTip
    {
        public string Title;
        [TextArea(1, 3)] public string Description;
        public float TipMinTemperature;
        public float TipMaxTemperature;

        public bool Matches(float temp) =>
            temp >= TipMinTemperature && temp <= TipMaxTemperature;
    }

    #endregion

    /// <summary>
    /// Contrôleur d’UI pour la température (air/eau).
    /// - Parse l’input utilisateur
    /// - Alimente un shader (fill du thermomètre)
    /// - Affiche un tip selon la température
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UI_WeatherTemperatureController : MonoBehaviour
    {
        #region Serialized Fields — Wiring

        [Header("Input")]
        [SerializeField] private TMP_InputField _temperatureInputField;

        [Header("Thermometer Visual")]
        [SerializeField] private Image _thermometerImage;
        [SerializeField] private Material _thermometerMaterial;
        [Tooltip("Nom de la propriété shader utilisée pour remplir le thermomètre (0..1).")]
        [SerializeField] private string _fillAmountProperty = "_FillAmount";

        [Header("Tips UI")]
        [SerializeField] private TextMeshProUGUI _tipTitleText;
        [SerializeField] private TextMeshProUGUI _tipDescriptionText;

        [Header("Config")]
        [SerializeField] private float _minTemperature = -10f;
        [SerializeField] private float _maxTemperature = 30f;
        [SerializeField] private TemperatureTip[] _tips;

        #endregion

        #region State

        /// <summary>Température actuellement sélectionnée (clampée dans [_minTemperature; _maxTemperature]).</summary>
        public float CurrentTemperature { get; private set; } = 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_temperatureInputField != null)
                _temperatureInputField.onValueChanged.AddListener(OnTemperatureInputChanged);
        }

        private void Start()
        {
            // Initialiser l’affichage à partir de la valeur courante
            ApplyTemperatureToUI(CurrentTemperature);
        }

        private void OnDestroy()
        {
            if (_temperatureInputField != null)
                _temperatureInputField.onValueChanged.RemoveListener(OnTemperatureInputChanged);
        }

        private void OnValidate()
        {
            // Si omis dans l’inspecteur, on tente de récupérer.
            if (_temperatureInputField == null)
                _temperatureInputField = GetComponentInChildren<TMP_InputField>(true);

            if (_thermometerImage == null)
                _thermometerImage = GetComponentInChildren<Image>(true);

            // clamp cohérent des bornes
            if (_maxTemperature < _minTemperature)
                _maxTemperature = _minTemperature;
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Parse l’entrée utilisateur (gère point/virgule), puis met à jour l’UI.
        /// </summary>
        private void OnTemperatureInputChanged(string raw)
        {
            if (FloatExtension.TryParse(raw, out float value))
            {
                SetTemperature(value);
            }
            else
            {
                Debug.LogWarning($"[UI_WeatherTemperatureController] Valeur non valide: '{raw}'");
                // Feedback visuel neutre si parsing KO (facultatif)
                SetThermometerFill(1f);
                ClearTip();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Définit la température depuis du code (valeur clampée) et rafraîchit l’UI.
        /// </summary>
        public void SetTemperature(float value)
        {
            CurrentTemperature = Mathf.Clamp(value, _minTemperature, _maxTemperature);
            ApplyTemperatureToUI(CurrentTemperature);

            // Optionnel : synchroniser le champ texte si besoin
            if (_temperatureInputField != null)
                _temperatureInputField.SetTextWithoutNotify(CurrentTemperature.ToString("0.#", CultureInfo.InvariantCulture));
        }

        #endregion

        #region Internals

        /// <summary>
        /// Met à jour le fill du thermomètre & le tip associé.
        /// </summary>
        private void ApplyTemperatureToUI(float value)
        {
            // Fill 0..1
            float t01 = Mathf.InverseLerp(_minTemperature, _maxTemperature, value);
            SetThermometerFill(t01);

            // Tip
            if (TryGetTip(value, out var tip))
                ShowTip(tip);
            else
                ClearTip();
        }

        /// <summary>Essaye de trouver le premier tip correspondant (l’ordre du tableau fait foi).</summary>
        private bool TryGetTip(float temperature, out TemperatureTip tip)
        {
            for (int i = 0; i < _tips.Length; i++)
            {
                if (_tips[i].Matches(temperature))
                {
                    tip = _tips[i];
                    return true;
                }
            }
            tip = default;
            return false;
        }

        private void ShowTip(TemperatureTip tip)
        {
            if (_tipTitleText) _tipTitleText.text = tip.Title;
            if (_tipDescriptionText) _tipDescriptionText.text = tip.Description;
        }

        private void ClearTip()
        {
            if (_tipTitleText) _tipTitleText.text = string.Empty;
            if (_tipDescriptionText) _tipDescriptionText.text = string.Empty;
        }

        private void SetThermometerFill(float normalized)
        {
            if (_thermometerMaterial == null) return;
            _thermometerMaterial.SetFloat(_fillAmountProperty, Mathf.Clamp01(normalized));
        }

        #endregion
    }
}
