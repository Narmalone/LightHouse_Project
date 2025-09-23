using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.DayNightSystem;
using LightHouse.Money;
using LightHouse.Weather;
using LightHouse.Weather.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    public class SupplyManager : LEOWindow
    {
        #region ===== Config & références =====

        [Header("Config")]
        [SerializeField] private SO_SupplyConfigurator _configurator;
        [SerializeField] private WeatherTimeline _timeline;
        [SerializeField] private TimeConfiguration _timeConfig;
        [SerializeField] private SO_ShipmentConfiguration _shipmentConfig;

        [Header("Controllers")]
        [SerializeField] private SupplyPopUp _popupPreafb;
        [SerializeField] private ShopController _shopController;
        [SerializeField] private OrderController _orderController;

        [SerializeField] private Button _confirmOrderButton;
        [SerializeField] private TextMeshProUGUI _shipmentIsFullTxt;
        [SerializeField] private Button _resetOrderButton;

        #endregion

        #region ===== État runtime =====

        private float _totalOrderValue;
        private Dictionary<E_SupplyCategory, SupplyItemDatas[]> _runtimeConfig;

        public ShopController ShopController => _shopController;
        public OrderController OrderController => _orderController;

        private SupplyPopUp _currentInstance;

        public event Action<MailDatas> SendMailRequest;

        // Plusieurs shipments simultanés
        private readonly List<ShipmentSystem> _shipments = new List<ShipmentSystem>();

        // Payload (lignes) par shipment
        private readonly Dictionary<ShipmentSystem, List<MailGenerator.SupplyOrderLine>> _payloadByShipment
            = new Dictionary<ShipmentSystem, List<MailGenerator.SupplyOrderLine>>();

        private uint _currentTicket = 0;

        #endregion

        #region ===== Cycle Unity =====

        private void Awake()
        {
            BuildRuntimeConfig(_configurator);

            // Brancher les events des controllers
            _shopController.OnShopPlus += OnShopPlus;
            _shopController.OnShopMinus += OnShopMinus;

            _orderController.OnOrderPlus += OnOrderPlus;
            _orderController.OnOrderMinus += OnOrderMinus;

            _confirmOrderButton.interactable = false;
            _confirmOrderButton.onClick.AddListener(OnConfirmOrderCliqued);
            _resetOrderButton.onClick.AddListener(OnResetOrderCliqued);

            if (_shipmentIsFullTxt.isActiveAndEnabled)
                _shipmentIsFullTxt.gameObject.SetActive(false);

            // Construire le shop à partir des datas runtime
            _shopController.BuildShop(_runtimeConfig);

            PlayerCurrency.OnBalanceChanged += PlayerCurrency_OnBalanceChanged;

            _shopController.SwitchTo(E_SupplyCategory.Tools);
        }

        protected void OnDestroy()
        {
            // Débrancher proprement
            _shopController.OnShopPlus -= OnShopPlus;
            _shopController.OnShopMinus -= OnShopMinus;
            _orderController.OnOrderPlus -= OnOrderPlus;
            _orderController.OnOrderMinus -= OnOrderMinus;
            _confirmOrderButton.onClick.RemoveListener(OnConfirmOrderCliqued);
            _resetOrderButton.onClick.RemoveListener(OnResetOrderCliqued);
            PlayerCurrency.OnBalanceChanged -= PlayerCurrency_OnBalanceChanged;

            _shopController.Clear();
            _orderController.Clear();
        }

        private void LateUpdate()
        {
            // Debug crédit rapide
            if (Input.GetKeyDown(KeyCode.G))
                PlayerCurrency.Add(150);
        }

        private void Update()
        {
            // Tick chaque shipment
            for (int i = _shipments.Count - 1; i >= 0; i--)
                _shipments[i].Tick(Time.deltaTime);
        }

        #endregion

        #region ===== Flux UI (pop-up, boutons) =====

        private void PlayerCurrency_OnBalanceChanged(float obj)
        {
            //UpdateOrderUI();
        }

        private void OnConfirmOrderCliqued()
        {
            _currentInstance = Instantiate(_popupPreafb, this.transform as RectTransform);
            _currentInstance.ConfirmOrderButton.onClick.AddListener(OnOrderConfirmedCliqued);
            _currentInstance.CancelOrderButton.onClick.AddListener(OnCancelPopupCliqued);

            // ETA affiché : si un shipment est déjà en préparation, on montre son temps restant
            var currentShipment = FindLatestOpenShipment();
            float missingHoursToNextShipment = _shipmentConfig.ShipmentSchedule;
            if (currentShipment != null) missingHoursToNextShipment = currentShipment.RemainingGameHours;

            _currentInstance.Initialize(_totalOrderValue, missingHoursToNextShipment);
        }

        private void OnCancelPopupCliqued() => ClosePopup();

        private void OnOrderConfirmedCliqued()
        {
            // 1) Lignes de la commande courante
            var lines = BuildOrderLines();

            // 2) Drapeau “retard météo” pour CETTE commande
            bool shouldBeDelayed = ComputeShouldBeDelayedForEta();

            // 3) Ticket + mail de récap programmé sur la date/heure après lead
            TimeUtility.GetDateAfterHours(_shipmentConfig.ShipmentSchedule, out byte scheduledDay, out float scheduledTime);

            var recapMail = MailGenerator.GenerateMailFromSupplyOrderTemplate(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "Dev-00",
                items: lines,
                deliveryDay: scheduledDay,
                deliveryHour: scheduledTime,
                arrivalDay: TimeHandlerData.CurrentDay,
                arrivalTime: TimeHandlerData.CurrentTime,
                ticketNumber: _currentTicket,
                isDelayed: shouldBeDelayed
            );
            SendMailRequest?.Invoke(recapMail);

            // 4) Trouver un shipment “ouvert” (Preparing) ou en créer un nouveau
            var open = FindLatestOpenShipment();
            if (open == null)
            {
                _currentTicket++;
                open = CreateShipment(shouldBeDelayed, _currentTicket);
                open.AddItems(lines);
            }

            // 5) Ajouter les lignes au payload du shipment sélectionné
            EnsurePayloadList(open);
            _payloadByShipment[open].AddRange(lines);

            // 6) Paiement et reset du panier
            PlayerCurrency.Add(-_totalOrderValue);
            OnResetOrderCliqued();

            // 7) Fermer le pop-up
            ClosePopup();
        }

        private void ClosePopup()
        {
            if (_currentInstance == null) return;
            _currentInstance.ConfirmOrderButton.onClick.RemoveListener(OnOrderConfirmedCliqued);
            _currentInstance.CancelOrderButton.onClick.RemoveListener(OnCancelPopupCliqued);
            Destroy(_currentInstance.gameObject);
            _currentInstance = null;
        }

        #endregion

        #region ===== Commande : + / - / reset / total =====

        //CHECKER SI ON DEPASSE DANS LE LAST OPENED SHIPMENT SI ON A TROP D'OBJETS DANS LE STACK ET EMPECHER LE CONFIRM

        private void OnShopPlus(SupplyItemDatas d)
        {
            //attention si c'est null, voir pour créer un shipment temporaire ?


            d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy + 1);
            _shopController.RefreshShopItem(d);
            _orderController.UpdateOrderFor(d);
            _totalOrderValue += d.Cost;
            UpdateOrderUI();
        }

        private void OnShopMinus(SupplyItemDatas d)
        {
            d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy - 1);
            _shopController.RefreshShopItem(d);
            _orderController.UpdateOrderFor(d);
            _totalOrderValue -= d.Cost;
            UpdateOrderUI();
        }

        private void OnOrderPlus(SupplyItemDatas d)
        {
            d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy + 1);
            _shopController.RefreshShopItem(d);
            _orderController.UpdateOrderFor(d);
            _totalOrderValue += d.Cost;
            UpdateOrderUI();
        }

        private void OnOrderMinus(SupplyItemDatas d)
        {
            d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy - 1);
            _shopController.RefreshShopItem(d);
            _orderController.UpdateOrderFor(d);
            _totalOrderValue -= d.Cost;
            UpdateOrderUI();
        }

        private void OnResetOrderCliqued()
        {
            _totalOrderValue = 0;
            _orderController.Clear();
            _shopController.RefreshAllShopItem();
            UpdateOrderUI();
        }

        private void UpdateOrderUI()
        {
            // 1) Mettre à jour le total
            _orderController.UpdateTotalOrderValue(_totalOrderValue);

            // 2) Conditions de base
            bool canAfford = _totalOrderValue <= PlayerCurrency.Balance;
            bool hasItems = _orderController.NumberOfItemsInOrder > 0;

            // 3) Récupérer (ou créer) le shipment
            var existing = FindLatestOpenShipment();
            bool createdNew = existing == null;

            var shipment = existing != null ? existing : new ShipmentSystem(_timeConfig, 9999f, false, 0f);
            int prospectiveQuantity = 0;

            // 4) Si on vient de le créer, ajouter les lignes en attente
            if (createdNew && OrderController.OrderItems.Count > 0)
            {
                var lines = BuildOrderLines();
                foreach(var line in lines)
                {
                    prospectiveQuantity = line.Quantity;
                }
                if (lines.Count > 0) shipment.AddItems(lines);
            }
            if(existing != null)
            {
                var lines = BuildOrderLines();
                int totalQtty = 0;
                foreach(var line in lines)
                {
                    totalQtty += line.Quantity;
                }
                prospectiveQuantity = existing.GetTotalQuantity() + totalQtty;
                Debug.Log($"{existing.GetTotalQuantity()}");
            }

            // 5) Capacité du shipment
            bool underCapacity = prospectiveQuantity > _shipmentConfig.MaxItemsPerShipment;
            if (underCapacity)
            {
                if(!_shipmentIsFullTxt.isActiveAndEnabled)
                    _shipmentIsFullTxt.gameObject.SetActive(true);
                _shipmentIsFullTxt.text = $"Your shipment is full please remove: {(shipment.GetTotalQuantity() > _shipmentConfig.MaxItemsPerShipment ? shipment.GetTotalQuantity() - _shipmentConfig.MaxItemsPerShipment : _shipmentConfig.MaxItemsPerShipment - shipment.GetTotalQuantity()) } items in your order";
            }
            else
            {
                if (_shipmentIsFullTxt.isActiveAndEnabled)
                    _shipmentIsFullTxt.gameObject.SetActive(false);
            }

            // 6) Un seul endroit qui décide l'état du bouton
            _confirmOrderButton.interactable = canAfford && hasItems && !underCapacity;
        }


        #endregion

        #region ===== Shipments : recherche / création / callbacks =====

        /// <summary>
        /// Retourne le shipment le plus récent encore “ouvert” (avant départ).
        /// </summary>
        private ShipmentSystem FindLatestOpenShipment()
        {
            for (int i = _shipments.Count - 1; i >= 0; i--)
            {
                var s = _shipments[i];
                if (s.Phase == ShipmentPhase.Preparing)
                {
                    Debug.Log($"shipment finded: {s.TicketNumber} , {s.GetTotalQuantity()}");
                    return s;
                }
                    
            }
            return null;
        }

        private ShipmentSystem CreateShipment(bool isShipmentDelayed, uint ticketOrderNumber)
        {
            var newShipment = new ShipmentSystem(
                cfg: _timeConfig,
                leadTimeHours: _shipmentConfig.ShipmentSchedule,         // ex: 48h in-game
                shouldBeDelayed: isShipmentDelayed,
                additionalDelayHoursIfDelayed: _shipmentConfig.ShipmentDelayTime, // ex: +24h
                dispatchHour: _shipmentConfig.ShipmentDeliveryHour,      // ex: 9f
                ticketNumber: ticketOrderNumber
            );

            _shipments.Add(newShipment);
            EnsurePayloadList(newShipment);

            // Abonnements (pas d’unsubscribe de lambdas inline)
            newShipment.OnInitialShipmentDelayCompleted += () => OnShipmentDelayConfirmed(newShipment);
            newShipment.OnPrepared += () => OnShipmentPrepared(newShipment);
            newShipment.OnArrived += () => OnShipmentArrived(newShipment);

            return newShipment;
        }

        private void OnShipmentDelayConfirmed(ShipmentSystem s)
        {
            var mail = MailGenerator.BuildShipmentDelayNotice(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "Dev-00",
                ticketNumber: s.TicketNumber,
                newDeliveryDay: (byte)(TimeHandlerData.CurrentDay + 1),
                newDeliveryHour: TimeHandlerData.CurrentTime,
                arrivalDay: TimeHandlerData.CurrentDay,
                arrivalTime: TimeHandlerData.CurrentTime
            );
            SendMailRequest?.Invoke(mail);
        }

        private void OnShipmentPrepared(ShipmentSystem s)
        {
            var mail = MailGenerator.BuildSupplyDeliverySent(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "Dev-00",
                ticketNumber: s.TicketNumber,
                etaHour: _shipmentConfig.ShipmentDeliveryHour,
                arrivalDay: TimeHandlerData.CurrentDay,
                arrivalTime: TimeHandlerData.CurrentTime
            );
            SendMailRequest?.Invoke(mail);
        }

        private void OnShipmentArrived(ShipmentSystem s)
        {
            // Récupère le payload
            if (_payloadByShipment.TryGetValue(s, out var lines))
            {
                // TODO: appliquer la livraison à l’inventaire ici
                // Inventory.Add(lines);

                var mail = MailGenerator.BuildSupplyDeliveryCompleted(
                    dateFormat: TimeUtility.FormatCurrentDate(),
                    keeperName: "Dev-00",
                    ticketNumber: s.TicketNumber,
                    arrivalDay: TimeHandlerData.CurrentDay,
                    arrivalTime: TimeHandlerData.CurrentTime
                );
                SendMailRequest?.Invoke(mail);
            }

            // Nettoyage
            _payloadByShipment.Remove(s);
            _shipments.Remove(s);
        }

        #endregion

        #region ===== Helpers (mails, météo, payload, runtime config) =====

        private List<MailGenerator.SupplyOrderLine> BuildOrderLines()
        {
            var lines = new List<MailGenerator.SupplyOrderLine>();
            foreach (var itemType in _orderController.OrderItems.Values)
            {
                lines.Add(new MailGenerator.SupplyOrderLine(
                    itemType.Mydatas.UniqueID,
                    itemType.Mydatas.Name,
                    itemType.Mydatas.SelectedAmountToBuy,
                    itemType.Mydatas.Cost
                ));
            }
            return lines;
        }

        private bool ComputeShouldBeDelayedForEta()
        {
            // Heuristique simple au moment de la commande : J+time à l’heure actuelle
            TimeUtility.GetDateAfterHours(_shipmentConfig.ShipmentSchedule, out byte targetDay, out float targetTime);
            var wdAtEta = WeatherUtils.GetWeatherAt(
                day: targetDay,
                hour: targetTime,
                _timeline,
                _timeConfig
            );

            return wdAtEta != null &&
                   (wdAtEta.WindSpeed >= 75f ||
                    wdAtEta.WeatherType == WeatherType.Stormy ||
                    wdAtEta.WeatherType == WeatherType.Windy);
        }

        private void EnsurePayloadList(ShipmentSystem s)
        {
            if (!_payloadByShipment.TryGetValue(s, out var _))
                _payloadByShipment[s] = new List<MailGenerator.SupplyOrderLine>();
        }

        private void BuildRuntimeConfig(SO_SupplyConfigurator src)
        {
            _runtimeConfig = new Dictionary<E_SupplyCategory, SupplyItemDatas[]>();
            foreach (var kvp in src.SupplyConfig)
            {
                var srcArray = kvp.Value;
                var clone = new SupplyItemDatas[srcArray.Length];
                for (int i = 0; i < srcArray.Length; i++)
                {
                    var d = srcArray[i];
                    clone[i] = new SupplyItemDatas
                    {
                        UniqueID = d.UniqueID,
                        Name = d.Name,
                        Cost = d.Cost,
                        SelectedAmountToBuy = 0
                    };
                }
                _runtimeConfig[kvp.Key] = clone;
            }
        }

        #endregion
    }
}

