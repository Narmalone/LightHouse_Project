using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.LEO.NightWatch.Buoys
{
    /// <summary>
    /// Contrôle l'affichage et l'état d'une bouée dans l'UI.
    /// Gère les interactions de clic et l'affichage des différents états.
    /// </summary>
    public class UI_Buoy : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Événement déclenché lorsque l'utilisateur clique sur la bouée.
        /// </summary>
        public event Action<UI_Buoy> OnBuoyCliqued;

        /// <summary>
        /// Événement déclenché lorsque l'état de la bouée change.
        /// </summary>
        public event Action<UI_Buoy> OnBuoyStateChanged;
        #endregion

        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _buoyIDText;

        [Header("State Colors")]
        [SerializeField] private Color _uncheckedColor = Color.grey;
        [SerializeField] private Color _invalidColor = Color.red;
        [SerializeField] private Color _validColor = Color.green;
        #endregion

        #region Private Fields
        [SerializeField] private UI_BuoyState _currentState;

        private int _id = -1;
        #endregion

        #region Public Properties
        /// <summary>
        /// Identifiant unique de la bouée.
        /// </summary>
        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                UpdateBuoyIdText();
            }
        }

        /// <summary>
        /// Accès direct au bouton associé à la bouée.
        /// </summary>
        public Button Button => _button;

        /// <summary>
        /// État actuel de la bouée.
        /// </summary>
        public UI_BuoyState CurrentState => _currentState;

        /// <summary>
        /// Indique si la bouée a déjà été reportée / expirée aujourd'hui.
        /// </summary>
        public bool HasBeenReportedToday { get; set; }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void Start()
        {
            SwitchTo(UI_BuoyState.Unchecked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleButtonClick);
        }
        #endregion

        #region Private Methods - UI Updates
        /// <summary>
        /// Met à jour l'affichage du texte d'ID.
        /// </summary>
        private void UpdateBuoyIdText()
        {
            _buoyIDText.text = $"Buoy: {ID:00}";
        }

        /// <summary>
        /// Gère le clic sur la bouée et le passage à l'état suivant.
        /// </summary>
        private void HandleButtonClick()
        {
            UI_BuoyState nextState = GetNextState();
            SwitchTo(nextState);
            OnBuoyCliqued?.Invoke(this);
        }

        /// <summary>
        /// Détermine l'état suivant lors du clic utilisateur.
        /// </summary>
        private UI_BuoyState GetNextState()
        {
            int stateCount = Enum.GetValues(typeof(UI_BuoyState)).Length;

            // Évite de passer à Failed via le cycle de clic
            if ((int)_currentState + 1 >= stateCount || _currentState + 1 == UI_BuoyState.Failed)
                return UI_BuoyState.Unchecked;

            return _currentState + 1;
        }
        #endregion

        #region Public Methods - State Changes
        /// <summary>
        /// Change l'état visuel et logique de la bouée.
        /// </summary>
        public void SwitchTo(UI_BuoyState nextState)
        {
            ApplyStateVisuals(nextState);
            _currentState = nextState;
            OnBuoyStateChanged?.Invoke(this);
        }
        #endregion

        #region Private Methods - State Visuals
        private void ApplyStateVisuals(UI_BuoyState state)
        {
            switch (state)
            {
                case UI_BuoyState.Unchecked:
                    SetButtonState(true, _uncheckedColor, "Unchecked");
                    break;

                case UI_BuoyState.Valid:
                    SetButtonState(true, _validColor, "Valid");
                    break;

                case UI_BuoyState.Invalid:
                    SetButtonState(true, _invalidColor, "Invalid");
                    break;

                case UI_BuoyState.Failed:
                    SetButtonState(false, _button.targetGraphic.color, "Failed");
                    break;

                case UI_BuoyState.Reported:
                    SetButtonState(false, _button.targetGraphic.color, "Reported");
                    break;

                case UI_BuoyState.Expired:
                    SetButtonState(false, _button.targetGraphic.color, "Expired");
                    break;
            }
        }

        /// <summary>
        /// Applique une couleur, un texte et l'interactivité au bouton.
        /// </summary>
        private void SetButtonState(bool interactable, Color color, string status)
        {
            _button.interactable = interactable;
            _button.targetGraphic.color = color;
            _statusText.text = status;
        }
        #endregion
    }

}
