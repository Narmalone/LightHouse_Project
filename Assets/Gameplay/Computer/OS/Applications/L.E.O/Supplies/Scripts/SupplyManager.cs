using LightHouse.Game.Computer.LEO;
using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.Computer.LEO.Supplies;
using LightHouse.Game.DayNightSystem;
using LightHouse.Money;
using LightHouse.Weather;
using LightHouse.Weather.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SupplyManager : LEOWindow
{
    [Header("Config")]
    [SerializeField] private SupplyConfigurator _configurator;
    [SerializeField] private WeatherTimeline _timeline;
    [SerializeField] private TimeConfiguration _timeConfig;
    [SerializeField] private SO_ShipmentConfiguration _shipmentConfig;

    [Header("Controllers")]
    [SerializeField] private SupplyPopUp _popupPreafb;
    [SerializeField] private ShopController _shopController;
    [SerializeField] private OrderController _orderController;

    [SerializeField] private Button _confirmOrderButton;
    [SerializeField] private Button _resetOrderButton;

    private float _totalOrderValue;

    // Copie runtime des datas (recommandé)
    private Dictionary<E_SupplyCategory, SupplyItemDatas[]> _runtimeConfig;

    public ShopController ShopController => _shopController;
    public OrderController OrderController => _orderController;

    private SupplyPopUp _currentInstance;

    public event Action<MailDatas> SendOrderRecapMail;

    // Remplace l’unique shipment par une collection
    private readonly List<ShipmentSystem> _shipments = new List<ShipmentSystem>();

    // Associe à chaque shipment son payload (les lignes à livrer)
    private readonly Dictionary<ShipmentSystem, List<MailGenerator.SupplyOrderLine>> _payloadByShipment
        = new Dictionary<ShipmentSystem, List<MailGenerator.SupplyOrderLine>>();

    private uint _currentTicket = 0;



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
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayerCurrency.Add(150);
        }

    }

    private void Update()
    {
        // Tick chaque shipment (pas besoin d’autre manager)
        for (int i = _shipments.Count - 1; i >= 0; i--)
        {
            var s = _shipments[i];
            s.Tick(Time.deltaTime);

            // Option : si tu préfères retirer ici quand Completed,
            // tu peux le faire dans l’handler OnArrived (recommandé)
        }
    }


    /// <summary>
    /// Quand la currency change on vérifie déjà si il y'a quelque chose dans le panier
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PlayerCurrency_OnBalanceChanged(float obj)
    {
        UpdateOrderUI();
    }

    private void OnConfirmOrderCliqued()
    {
        _currentInstance = Instantiate(_popupPreafb, this.transform as RectTransform);
        _currentInstance.ConfirmOrderButton.onClick.AddListener(OkConfirmCliqued);
        _currentInstance.CancelOrderButton.onClick.AddListener(OnCancelPopupCliqued);

        var currentShipment = FindLatestOpenShipment();
        float missingHoursToNextShipment = _shipmentConfig.ShipmentSchedule;
        if (currentShipment != null) missingHoursToNextShipment = currentShipment.RemainingGameHours;
        Debug.Log(missingHoursToNextShipment);
        _currentInstance.Initialize(_totalOrderValue, missingHoursToNextShipment);
    }

    private void OnCancelPopupCliqued()
    {
        _currentInstance.CancelOrderButton.onClick.RemoveListener(OnCancelPopupCliqued);
        _currentInstance.CancelOrderButton.onClick.RemoveListener(OnCancelPopupCliqued);
        Destroy(_currentInstance.gameObject);
        _currentInstance = null;
    }

    private void OkConfirmCliqued()
    {

        // 2) Construit le payload de CETTE commande
        var lines = new List<MailGenerator.SupplyOrderLine>();
        foreach (var itemType in _orderController.OrderItems.Values)
            lines.Add(new MailGenerator.SupplyOrderLine(
                itemType.Mydatas.UniqueID,
                itemType.Mydatas.Name,
                itemType.Mydatas.SelectedAmountToBuy,
                itemType.Mydatas.Cost));

        // 3) Météo pour flag delay
        var wdAtEta = WeatherUtils.GetWeatherAt(
            (byte)(TimeHandlerData.CurrentDay + 2),
            TimeHandlerData.CurrentTime,
            _timeline,
            _timeConfig
        );
        bool shouldBeDelayed =
            wdAtEta.WindSpeed >= 75f ||
            wdAtEta.WeatherType == WeatherType.Stormy ||
            wdAtEta.WeatherType == WeatherType.Windy;

        // 4) Ticket + mail de récap (immédiat)
        _currentTicket++;
        var recapMail = MailGenerator.GenerateMailFromSupplyOrderTemplate(
            dateFormat: TimeUtility.FormatCurrentDate(),
            keeperName: "Dev-00",
            items: lines,
            deliveryInDays: 2,
            deliveryHour: TimeHandlerData.CurrentTime,
            arrivalDay: (byte)(TimeHandlerData.CurrentDay + 2),
            arrivalTime: TimeHandlerData.CurrentTime,
            ticketNumber: _currentTicket,
            isDelayed: shouldBeDelayed
        );
        SendOrderRecapMail?.Invoke(recapMail);

        _currentTicket++;
        // 5) Choisir un shipment "ouvert" (le plus récent) OU en créer un
        var open = FindLatestOpenShipment();
        if (open == null)
        {
            open = CreateShipment(shouldBeDelayed, _currentTicket);
            Debug.Log("No shipment in queue, Generating a new shipment");

        }

        // 6) Fusionner le payload dans le shipment choisi (IMPORTANT: AddRange, pas Add)
        EnsurePayloadList(open);
        _payloadByShipment[open].AddRange(lines);

        //Paiement
        PlayerCurrency.Add(-_totalOrderValue);

        // 7) Reset UI panier
        OnResetOrderCliqued();

        _currentInstance.CancelOrderButton.onClick.RemoveListener(OnCancelPopupCliqued);
        _currentInstance.CancelOrderButton.onClick.RemoveListener(OnCancelPopupCliqued);
        Destroy(_currentInstance.gameObject);
        _currentInstance = null;
    }


    // Retourne le shipment le PLUS RÉCENT encore "ouvert" (avant départ) => Phase == Preparing.
    // Si tu veux autoriser la fusion tant qu'il n'est pas "arrivé", remplace par: s.Phase != ShipmentPhase.Completed
    private ShipmentSystem FindLatestOpenShipment()
    {
        for (int i = _shipments.Count - 1; i >= 0; i--)
        {
            var s = _shipments[i];
            if (s.Phase != ShipmentPhase.Completed)
                return s;
        }
        return null;
    }

    private ShipmentSystem CreateShipment(bool isShipmentDelayed, uint ticketOrderNumber)
    {
        var newShipment = new ShipmentSystem(
            cfg: _timeConfig,
            leadTimeHours: _shipmentConfig.ShipmentSchedule,     // ex: 48h in-game
            shouldBeDelayed: isShipmentDelayed,
            additionalDelayHoursIfDelayed: _shipmentConfig.ShipmentDelayTime,    // ex: 24h in-game si delayed
            dispatchHour: _shipmentConfig.ShipmentDeliveryHour,  // ex: 9f
            ticketNumber: ticketOrderNumber
        );

        _shipments.Add(newShipment);
        EnsurePayloadList(newShipment);

        // Abonnements
        newShipment.OnInitialShipmentDelayCompleted += () => OnShipmentDelayConfirmed(newShipment);
        newShipment.OnPrepared += () => OnShipmentPrepared(newShipment);
        newShipment.OnArrived += () => OnShipmentArrived(newShipment);

        return newShipment;
    }

    private void EnsurePayloadList(ShipmentSystem s)
    {
        if (!_payloadByShipment.TryGetValue(s, out var list))
            _payloadByShipment[s] = new List<MailGenerator.SupplyOrderLine>();
    }

    private void OnShipmentDelayConfirmed(ShipmentSystem s)
    {
        // Ici : le lead initial est fini ET le retard a été confirmé.
        // Tu peux envoyer un mail “Delay confirmed” pour CETTE commande.
        // (Si tu veux, génère un mail dédié avec MailGenerator.)
        MailGenerator.BuildShipmentDelayNotice(
            dateFormat: TimeUtility.FormatCurrentDate(),
            keeperName: "Dev-00",
            ticketNumber: s.TicketNumber,
            newDeliveryDay: (byte)(TimeHandlerData.CurrentDay + 1),
            newDeliveryHour: TimeHandlerData.CurrentTime,
            arrivalDay: TimeHandlerData.CurrentDay,
            arrivalTime: TimeHandlerData.CurrentTime);
        s.OnInitialShipmentDelayCompleted -= () => OnShipmentDelayConfirmed(s);
    }

    private void OnShipmentPrepared(ShipmentSystem s)
    {
        // Ici : la phase 1 (lead + éventuel retard) est terminée,
        // on attend maintenant 09:00 du jour suivant.
        // Envoie un mail “Shipment dispatched – arriving next day at 09:00”
        //byte nextDeliveryDay = TimeHandlerData.CurrentTime < _shipmentConfig.ShipmentDeliveryHour ? TimeHandlerData.CurrentDay : (byte)(TimeHandlerData.CurrentDay + 1);

        MailGenerator.BuildSupplyDeliverySent(
            dateFormat: TimeUtility.FormatCurrentDate(),
            keeperName: "Dev-00",
            ticketNumber: s.TicketNumber,
            etaHour: _shipmentConfig.ShipmentDeliveryHour,
            arrivalDay: TimeHandlerData.CurrentDay,
            arrivalTime: TimeHandlerData.CurrentTime);
        s.OnPrepared -= () => OnShipmentPrepared(s); // si tu veux éviter des re-subscriptions accidentelles
    }

    private void OnShipmentArrived(ShipmentSystem s)
    {
        // Récupère le payload de CETTE commande
        if (_payloadByShipment.TryGetValue(s, out var lines))
        {
            // Applique la livraison : add to inventory ici
            // Inventory.Add(lines) ... (à adapter à ton système d’inventaire)

            // Mail d’arrivée / reçu de livraison
            // (tu peux réutiliser MailGenerator pour un "Delivery Receipt" si tu veux)
        }

        // Nettoyage
        s.OnArrived -= () => OnShipmentArrived(s);
        //s.OnPrepared -= () => OnShipmentPrepared(s);
        //s.OnInitialShipmentDelayCompleted -= () => OnShipmentDelayConfirmed(s);

        _payloadByShipment.Remove(s);
        _shipments.Remove(s);
    }


    private void BuildRuntimeConfig(SupplyConfigurator src)
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

    // ======== Gestion des clics émis par les controllers ========

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
        _orderController.UpdateTotalOrderValue(_totalOrderValue);
        if(_totalOrderValue > PlayerCurrency.Balance || _orderController.NumberOfItemsInOrder <= 0)
        {
            _confirmOrderButton.interactable = false;
        }
        else
        {
            _confirmOrderButton.interactable = true;
        }
    }
}
