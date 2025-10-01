using LightHouse.Game.Computer.LEO.Supplies;
using LightHouse.Game.DayNightSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField] private TimeConfiguration _timeConfig;
    [SerializeField] private DeliveryBoat _boatPrefab;
    [SerializeField] private SO_ShipmentConfiguration _shipmentConfig;
    [SerializeField] private Transform _deliveryPoint;

    [SerializeField, Range(0, 24)] private float _hoursToFillTheSupplies = 1.0f;
    [SerializeField, Range(0, 24)] private float _boatSpawnHoursBefore = 3.0f;
    [SerializeField] private VectorPathDatabase _pathDatabase;
    private List<ShipmentSystem> _preparedShipementQueue;

    private void Awake()
    {
        _shipmentConfig.ShippingPrepared += OnShipmentPrepared;
        _preparedShipementQueue = new List<ShipmentSystem>();
    }

    private void OnDestroy()
    {
        _shipmentConfig.ShippingPrepared -= OnShipmentPrepared;
    }

    /// <summary>
    /// Quand un shipment est complètement préparé pour la côte on commence la livraison
    /// </summary>
    /// <param name="system"> le shipment prêt à l'envoi </param>
    private void OnShipmentPrepared(ShipmentSystem system)
    {
        //TO DOO FAIRE ATTENDRE le Dispatch HOUR pour ensuite Start Delivery !!
        //StartDeliveryShip(system);
        //
        _preparedShipementQueue.Add(system);
    }

    private void Update()
    {
        if (_preparedShipementQueue.Count == 0) return;

        float now = TimeUtility.Normalize24h(TimeHandlerData.CurrentTime);

        for (int i = _preparedShipementQueue.Count - 1; i >= 0; i--)
        {
            var shipment = _preparedShipementQueue[i];
            if (TimeHandlerData.CurrentDay != shipment.DispatchDay) continue;

            // heure de spawn = dispatch - X heures (normalisée 0..24)
            float spawnTime = TimeUtility.Normalize24h(shipment.DispatchHour - _boatSpawnHoursBefore);
            
            // Déclenche "dès que" l'heure actuelle dépasse l'heure de spawn (⚠️ pas wrap-safe)
            if (now >= spawnTime)
            {
                Debug.Log("start delivery ship");
                StartDeliveryShip(shipment);
                _preparedShipementQueue.RemoveAt(i);
            }
        }
    }


    public void StartDeliveryShip(ShipmentSystem targetShipment)
    {
        var randomPath = _pathDatabase.GetRandomPath();
        //Générer le bateau
        DeliveryBoat instance = Instantiate(_boatPrefab, randomPath.Paths[0], Quaternion.identity);
        //le faire aller au point de livraison
        instance.Initialize(_hoursToFillTheSupplies, randomPath, targetShipment, _deliveryPoint.position);

        //Remplir le coffre de livraison des différents objets
    }
}
