using LightHouse.Game.Computer.Calendar;
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
        [SerializeField] private CalendarEventDatabase _calendarEventsDatabase;

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
        //private readonly List<ShipmentSystem> _shipments = new List<ShipmentSystem>();

        private uint _currentTicket = 0;

        #endregion

        #region ===== Cycle Unity =====

        protected override void Awake()
        {
            base.Awake();
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

            _shipmentConfig.Shipments.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
            for (int i = _shipmentConfig.Shipments.Count - 1; i >= 0; i--)
                _shipmentConfig.Shipments[i].Tick(Time.deltaTime);
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

            // 4) Trouver un shipment “ouvert” (Preparing) ou en créer un nouveau
            var open = FindLatestOpenShipment();
            if(open != null)
            {
                scheduledDay = open.ScheduledDay;
                scheduledTime = open.ScheduledHour;
            }
            if (open == null)
            {
                _currentTicket++;
                open = CreateShipment(scheduledDay, scheduledTime, shouldBeDelayed, _currentTicket);
            }
            open.AddItems(lines);
            // 5) Ajouter les lignes au payload du shipment sélectionné
            //EnsurePayloadList(open);

            // 6) Paiement et reset du panier
            PlayerCurrency.Add(-_totalOrderValue);
            OnResetOrderCliqued();

            // 7) Fermer le pop-up
            ClosePopup();

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

        private void OnShopPlus(SupplyItemDatas d)
        {
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

            // 3) Shipment existant (ou null)
            var existing = FindLatestOpenShipment();

            // 4) Quantité de la commande en cours (PANIER)
            int pendingQty = 0;
            if (hasItems)
            {
                var lines = BuildOrderLines();
                foreach (var l in lines) pendingQty += l.Quantity; // somme, pas écrasement
            }

            // 5) Quantité déjà engagée dans le shipment existant
            int committedQty = existing != null ? existing.GetTotalQuantity() : 0;

            // 6) Quantité après ajout si on validait maintenant
            int prospectiveQty = committedQty + pendingQty;

            // 7) Capacité
            int max = _shipmentConfig.MaxItemsPerShipment;
            bool atOrOverCapacity = prospectiveQty > max;

            // 8) UI message "shipment plein"
            if (atOrOverCapacity)
            {
                if (!_shipmentIsFullTxt.isActiveAndEnabled)
                    _shipmentIsFullTxt.gameObject.SetActive(true);

                int excess = Mathf.Max(0, prospectiveQty - max); // ← clé: on compare au prospectif
                _shipmentIsFullTxt.text = excess > 0
                    ? $"Your shipment is full. Please remove {excess} item(s) from your order."
                    : "Your shipment is full. No additional items can be added.";
            }
            else
            {
                if (_shipmentIsFullTxt.isActiveAndEnabled)
                    _shipmentIsFullTxt.gameObject.SetActive(false);
            }

            // 9) Etat du bouton
            _confirmOrderButton.interactable = canAfford && hasItems && !atOrOverCapacity;
        }


        #endregion

        #region ===== Shipments : recherche / création / callbacks =====

        /// <summary>
        /// Retourne le shipment le plus récent encore “ouvert” (avant départ).
        /// </summary>
        private ShipmentSystem FindLatestOpenShipment()
        {
            for (int i = _shipmentConfig.Shipments.Count - 1; i >= 0; i--)
            {
                var s = _shipmentConfig.Shipments[i];
                if (s.Phase == ShipmentPhase.Preparing)
                    return s;
            }
            return null;
        }

        private ShipmentSystem CreateShipment(byte scheduledDay, float scheduledHour, bool isShipmentDelayed, uint ticketOrderNumber)
        {
            var newShipment = new ShipmentSystem(
                cfg: _timeConfig,
                leadTimeHours: _shipmentConfig.ShipmentSchedule,         // ex: 48h in-game
                shouldBeDelayed: isShipmentDelayed,
                scheduledDay,
                scheduledHour,
                additionalDelayHoursIfDelayed: _shipmentConfig.ShipmentDelayTime, // ex: +24h
                dispatchHour: _shipmentConfig.ShipmentDeliveryHour,      // ex: 9f
                ticketNumber: ticketOrderNumber
            );

            _shipmentConfig.Shipments.Add(newShipment);

            CalendarEvent evt = CalendarEventBuilder.New("Supply prepared (delayed ?)")
                .Once(scheduledDay)
                .At(scheduledHour)
                .Build();
            _calendarEventsDatabase.AddEvent(evt);
            //EnsurePayloadList(newShipment);

            // Abonnements (pas d’unsubscribe de lambdas inline)
            newShipment.OnInitialShipmentDelayCompleted += () => OnShipmentDelayConfirmed(newShipment);
            newShipment.OnPrepared += () => OnShipmentPrepared(newShipment);
            newShipment.OnArrived += () => OnShipmentArrived(newShipment);

            return newShipment;
        }


        private void OnShipmentDelayConfirmed(ShipmentSystem s)
        {
            //A TESTER
            TimeUtility.GetDateAfterHours(_shipmentConfig.ShipmentDelayTime, out byte day, out float time);
            MailDatas mail = MailGenerator.BuildShipmentDelayNotice(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "Dev-00",
                ticketNumber: s.TicketNumber,
                newDeliveryDay: day,
                newDeliveryHour: time,
                arrivalDay: TimeHandlerData.CurrentDay,
                arrivalTime: TimeHandlerData.CurrentTime
            );

            CalendarEvent evt = CalendarEventBuilder.New("Shipment Prepared - Delayed")
                .Once(day)
                .At(time)
                .Build();
            SendMailRequest?.Invoke(mail);
        }

        private void OnShipmentPrepared(ShipmentSystem s)
        {
            MailDatas mail = MailGenerator.BuildSupplyDeliverySent(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "Dev-00",
                ticketNumber: s.TicketNumber,
                etaHour: _shipmentConfig.ShipmentDeliveryHour,
                arrivalDay: TimeHandlerData.CurrentDay,
                arrivalTime: TimeHandlerData.CurrentTime
            );

           /* CalendarEvent evt = CalendarEventBuilder.New("Shipment Prepared - Delayed")
                .Once(day)
                .At(time)
                .Build();
*/
            SendMailRequest?.Invoke(mail);
            _shipmentConfig.ShippingPrepared?.Invoke(s);
        }

        private void OnShipmentArrived(ShipmentSystem s)
        {
            // Récupère le payload
            if (_shipmentConfig.Shipments.Contains(s))
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
            //_payloadByShipment.Remove(s);
            _shipmentConfig.Shipments.Remove(s);
        }

        #endregion

        #region ===== Helpers (mails, météo, payload, runtime config) =====

        private List<MailGenerator.SupplyOrderDatas> BuildOrderLines()
        {
            var lines = new List<MailGenerator.SupplyOrderDatas>();
            foreach (var itemType in _orderController.OrderItems.Values)
            {
                lines.Add(new MailGenerator.SupplyOrderDatas(
                    itemType.Mydatas.UniqueID,
                    itemType.Mydatas.Name,
                    itemType.Mydatas.SelectedAmountToBuy,
                    itemType.Mydatas.Cost,
                    itemType.Mydatas.Prefab
                ));
                //Debug.Log(itemType.Mydatas.Prefab.name);
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
/*
        private void EnsurePayloadList(ShipmentSystem s)
        {
            if (_shipmentConfig.Shipments.Contains(s))
            {
                s.SupplyOrderLines = new List<MailGenerator.SupplyOrderLine>();
            }
            if (!_payloadByShipment.TryGetValue(s, out var _))
                _payloadByShipment[s] = new List<MailGenerator.SupplyOrderLine>();
        }*/

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
                        Prefab = d.Prefab,
                        SelectedAmountToBuy = 0
                    };
                }
                _runtimeConfig[kvp.Key] = clone;
            }
        }

        #endregion
    }
}

