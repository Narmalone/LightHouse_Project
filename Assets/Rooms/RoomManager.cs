using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;
    private Dictionary<IElectricityItem, Room> _rooms = new Dictionary<IElectricityItem, Room>();
    private Dictionary<ElectricityZones, List<IElectricityItem>> _staticsItems = new Dictionary<ElectricityZones, List<IElectricityItem>>();

    private void Awake()
    {
        instance = this;
    }

    public void AddElectricityItem(ElectricityZones targetZone, IElectricityItem item)
    {
        if (!_staticsItems.ContainsKey(targetZone))
        {
            Debug.Log("error");
            return;
        }
        if (_staticsItems[targetZone].Contains(item)) return;
        _staticsItems[targetZone].Add(item);
    }

}
