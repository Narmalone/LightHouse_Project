using UnityEngine;

[CreateAssetMenu(fileName = "SO_ShipmentConfig", menuName = "LightHouse/Shipment/New Shipment Config")]
public class SO_ShipmentConfiguration : ScriptableObject
{
    [Range(0.5f, 150f)] public float ShipmentSchedule = 48f;
    [Range(0.5f, 150f)] public float ShipmentDelayTime = 24f;
    [Range(0f, 23.9f)] public float ShipmentDeliveryHour = 9.0f;
}
