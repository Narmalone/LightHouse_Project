using LightHouse.Game.Computer.LEO.NightWatch;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch.Signals
{
    /// <summary>
    /// Contrôleur UI pour l'affichage en temps réel des signaux (bateaux et bouées)
    /// et de leur historique après validation ou expiration.
    /// </summary>
    public class UI_Signals : NightWatchReportWindow
    {
        #region Serialized Fields

        [Header("UI Prefabs")]
        [SerializeField] private SignalInfoElement _signalPrefab;
        [SerializeField] private SignalHistoryElement _historySignalPrefab;

        [Header("UI Containers")]
        [SerializeField] private RectTransform _signalParent;
        [SerializeField] private RectTransform _historyParent;

        [Header("Icons")]
        [SerializeField] private Sprite _boatIcon;
        [SerializeField] private Sprite _buoyIcon;
        [SerializeField] private Sprite _validIcon;
        [SerializeField] private Sprite _invalidIcon;

        [Header("Databases")]
        [SerializeField] private BoatAnomaliesDatabase _boatDatabase;
        [SerializeField] private BuyoncyAnomalyDatabase _buoyDatabase;

        #endregion

        #region Private Fields

        /// <summary>
        /// Signaux actuellement actifs dans l'UI, indexés par leur clé unique.
        /// </summary>
        private readonly Dictionary<string, SignalInfoElement> _activeSignals = new();

        /// <summary>
        /// Hook optionnel : permet à un système externe de demander une suppression forcée.
        /// Param1 = key (ex: "01"), Param2 = generateHistory (true => yes, false => no), Param3 = asValid (true => valid, false => invalid).
        /// </summary>
        public Action<string, bool, bool> TryForceRemoveSignal { get; set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            RegisterDatabaseEvents();
            TryForceRemoveSignal += OnForceRemoveSignalCalled;
        }

        private void OnDestroy()
        {
            UnregisterDatabaseEvents();
            TryForceRemoveSignal -= OnForceRemoveSignalCalled;
        }

        #endregion

        #region Event Registration

        /// <summary>
        /// Souscrit aux événements des bases d'anomalies.
        /// </summary>
        private void RegisterDatabaseEvents()
        {
            _boatDatabase.OnAnomalyAdded += OnAnomalyAdded;
            _boatDatabase.OnAnomalyRemoved += OnAnomalyRemoved;

            _buoyDatabase.OnAnomalyAdded += OnAnomalyAdded;
            _buoyDatabase.OnAnomalyRemoved += OnAnomalyRemoved;
        }

        /// <summary>
        /// Désouscrit de tous les événements des bases d'anomalies.
        /// </summary>
        private void UnregisterDatabaseEvents()
        {
            _boatDatabase.OnAnomalyAdded -= OnAnomalyAdded;
            _boatDatabase.OnAnomalyRemoved -= OnAnomalyRemoved;

            _buoyDatabase.OnAnomalyAdded -= OnAnomalyAdded;
            _buoyDatabase.OnAnomalyRemoved -= OnAnomalyRemoved;
        }

        #endregion

        #region External Forcing API

        /// <summary>
        /// Delegate à appeler quand on veux forcer un signal à s'enlever
        /// </summary>
        /// <param name="obj"> clé provenant de <see cref="ISignal"/> </param>
        /// <param name="isValid"> Si on veux que l'élément de l'historique soit perçu comme</param>
        private void OnForceRemoveSignalCalled(string obj, bool generateHistory, bool isValid)
        {
            if (!TryGetActiveSignal(obj, out var model))
            {
                Debug.LogWarning($"Le signal forcé n'existe pas {obj}");
                return;
            }

            if (generateHistory)
                CreateHistoryEntry(model, isValid ? _validIcon : _invalidIcon);

            RemoveSignalUI(model);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Ajoute un nouveau signal actif à l'UI lorsqu'une anomalie est détectée.
        /// </summary>
        private void OnAnomalyAdded(ISignal model)
        {
            if (_activeSignals.ContainsKey(model.Key))
                return;

            AddSignalUI(model);
        }

        /// <summary>
        /// Lorsqu'une anomalie expire, l'ajoute à l'historique comme invalide.
        /// </summary>
        private void HandleExpired(ISignal model)
        {
            CreateHistoryEntry(model, _invalidIcon);
            RemoveSignalUI(model);
        }

        /// <summary>
        /// Lorsqu'une anomalie est supprimée (résolue correctement), l'ajoute à l'historique comme valide.
        /// </summary>
        private void OnAnomalyRemoved(ISignal model)
        {
            CreateHistoryEntry(model, _validIcon);
            RemoveSignalUI(model);
        }

        #endregion

        #region UI Management

        private bool TryGetActiveSignal(string key, out ISignal model)
        {
            if (_activeSignals.TryGetValue(key, out var uiElement))
            {
                model = uiElement.Model;
                return true;
            }
            model = null;
            return false;
        }

        /// <summary>
        /// Ajoute un élément visuel représentant un signal actif.
        /// </summary>
        private void AddSignalUI(ISignal model)
        {
            var icon = GetSignalIcon(model);
            var uiElement = Instantiate(_signalPrefab, _signalParent);
            uiElement.Initialize(model, icon);

            if (model is BoatAnomalyDatas)
                uiElement.Icon.rectTransform.eulerAngles = new Vector3(0, 0, -90f);

            uiElement.OnTimerEnded += HandleExpired;
            _activeSignals[model.Key] = uiElement;
        }

        /// <summary>
        /// Supprime un signal actif de l'UI.
        /// </summary>
        private void RemoveSignalUI(ISignal model)
        {
            if (_activeSignals.TryGetValue(model.Key, out var uiElement))
            {
                uiElement.OnTimerEnded -= HandleExpired;
                Destroy(uiElement.gameObject);
                _activeSignals.Remove(model.Key);
            }
        }

        /// <summary>
        /// Crée une entrée dans l'historique des signaux avec l'icône de validation appropriée.
        /// </summary>
        private void CreateHistoryEntry(ISignal model, Sprite validationIcon)
        {
            var icon = GetSignalIcon(model);
            var historyElement = Instantiate(_historySignalPrefab, _historyParent);

            historyElement.SetInfos(
                icon: icon,
                arrivalDate: model.DisplayText,
                completionValidation: validationIcon
            );

            if (model is BoatAnomalyDatas)
                historyElement.Icon.rectTransform.eulerAngles = new Vector3(0, 0, -90f);
        }

        /// <summary>
        /// Retourne l'icône correspondant au type de signal.
        /// </summary>
        private Sprite GetSignalIcon(ISignal model) =>
            model switch
            {
                BoatAnomalyDatas => _boatIcon,
                BuyoncyAnomalyDatas => _buoyIcon,
                _ => null
            };

        #endregion
    }
}
