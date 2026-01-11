using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace LightHouse.Features.Computer.LEO.Weather.Wind
{
    #region Data

    /// <summary>
    /// Entrée d’échelle de Beaufort (vitesse du vent → niveau + libellés).
    /// Les vitesses sont exprimées dans l’unité choisie pour l’UI (ex: m/s, noeuds).
    /// </summary>
    [Serializable]
    public struct BeaufortScale
    {
        public int Level;
        public float MinWindSpeed;
        public float MaxWindSpeed;
        public string Title;
        [TextArea(1, 5)] public string Description;

        public bool Matches(float speed) => speed >= MinWindSpeed && speed <= MaxWindSpeed;

        public override string ToString() =>
            $"FCE {Level}: {Title} [{MinWindSpeed:0.#}-{MaxWindSpeed:0.#}]";
    }

    #endregion

    /// <summary>
    /// Contrôleur de la fenêtre Vent : saisie vitesse + boussole + affichage Beaufort.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UI_WindWindowController : MonoBehaviour
    {
        #region Serialized Fields — Wiring

        [Header("Inputs")]
        [SerializeField] private TMP_InputField _windSpeedInput;

        [Header("Output")]
        [SerializeField] private TextMeshProUGUI _beaufortLabel;

        [Header("Compass")]
        [SerializeField] private UI_CompassController _compassController;

        [Header("Beaufort Scale")]
        [Tooltip("Liste ordonnée (idéalement) des niveaux de Beaufort.")]
        [SerializeField] private BeaufortScale[] _beaufortScales;

        #endregion

        #region State & Events

        /// <summary>Vitesse du vent courante (unité de l’UI).</summary>
        public float CurrentWindSpeed { get; private set; }

        /// <summary>Accès public au contrôleur de boussole.</summary>
        public UI_CompassController CompassController => _compassController;

        /// <summary>Émis quand la vitesse change (après parsing & clamp éventuel).</summary>
        public event Action<float> OnWindSpeedChanged;

        /// <summary>Émis quand le niveau Beaufort correspondant change.</summary>
        public event Action<BeaufortScale?> OnBeaufortLevelChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_windSpeedInput != null)
                _windSpeedInput.onValueChanged.AddListener(OnWindSpeedInputChanged);
        }

        private void Start()
        {
            // initialise l’affichage à partir de CurrentWindSpeed (0 par défaut)
            ApplyWindSpeedToUI(CurrentWindSpeed);
        }

        private void OnDestroy()
        {
            if (_windSpeedInput != null)
                _windSpeedInput.onValueChanged.RemoveListener(OnWindSpeedInputChanged);
        }

        private void OnValidate()
        {
            if (_windSpeedInput == null)
                _windSpeedInput = GetComponentInChildren<TMP_InputField>(true);

            if (_beaufortLabel == null)
                _beaufortLabel = GetComponentInChildren<TextMeshProUGUI>(true);

            // sécurité: s’assurer que les plages sont cohérentes (Min <= Max)
            if (_beaufortScales != null)
            {
                for (int i = 0; i < _beaufortScales.Length; i++)
                {
                    if (_beaufortScales[i].MaxWindSpeed < _beaufortScales[i].MinWindSpeed)
                    {
                        var b = _beaufortScales[i];
                        float tmp = b.MinWindSpeed;
                        b.MinWindSpeed = b.MaxWindSpeed;
                        b.MaxWindSpeed = tmp;
                        _beaufortScales[i] = b;
                    }
                }
            }
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Parse l’input utilisateur (supporte '.' et ',') puis applique.
        /// </summary>
        private void OnWindSpeedInputChanged(string raw)
        {
            if (TryParseFloatFlexible(raw, out float value))
            {
                SetWindSpeed(value);
            }
            else
            {
                Debug.LogWarning($"[UI_WindWindowController] Valeur non valide: '{raw}'");
                // on laisse l’état inchangé; on pourrait aussi vider le label
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Définit la vitesse du vent depuis le code (et met à jour l’UI).
        /// </summary>
        public void SetWindSpeed(float windSpeed)
        {
            if (Mathf.Approximately(CurrentWindSpeed, windSpeed))
                return;

            CurrentWindSpeed = windSpeed;
            ApplyWindSpeedToUI(CurrentWindSpeed);

            OnWindSpeedChanged?.Invoke(CurrentWindSpeed);

            var level = FindBeaufortLevel(CurrentWindSpeed);
            OnBeaufortLevelChanged?.Invoke(level);
        }

        /// <summary>
        /// Calcule le niveau de Beaufort correspondant à une vitesse donnée.
        /// </summary>
        public BeaufortScale? FindBeaufortLevel(float windSpeed)
        {
            if (_beaufortScales == null || _beaufortScales.Length == 0)
                return null;

            // On prend le premier match (assure un ordre logique dans l’inspecteur)
            for (int i = 0; i < _beaufortScales.Length; i++)
                if (_beaufortScales[i].Matches(windSpeed))
                    return _beaufortScales[i];

            return null;
        }

        #endregion

        #region Internals

        /// <summary>
        /// Met à jour le label Beaufort en fonction de la vitesse courante.
        /// </summary>
        private void ApplyWindSpeedToUI(float windSpeed)
        {
            var matched = FindBeaufortLevel(windSpeed);

            if (_beaufortLabel == null)
                return;

            if (matched.HasValue)
            {
                var scale = matched.Value;
                // Titre court (comme ton code), on peut afficher la description si souhaité
                _beaufortLabel.text = $"FCE {scale.Level}: {scale.Title}";
            }
            else
            {
                _beaufortLabel.text = "Out of Beaufort scale";
            }
        }

        /// <summary>
        /// Parse un float en tolérant '.' et ',' comme séparateurs décimaux.
        /// </summary>
        private static bool TryParseFloatFlexible(string s, out float value)
        {
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return true;

            var replaced = s?.Replace(',', '.');
            return float.TryParse(replaced, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        #endregion
    }
}
