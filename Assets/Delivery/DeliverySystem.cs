using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverySystem : Singleton<DeliverySystem>
{
    public byte baseHoursToWait = 36;
    [SerializeField] private Transform SpawnPoint;

    //chopper le minuit du prochain jour à l'aide du middnightX de weatherfordaysManager
    //Comparer les 2 résultats entre le minuit actuel et chopper la météo dans X heures
    //
    public void GetSomething()
    {
        
    }

    public void SpawnItem(ShopItemData item)
    {
        if(item.Prefab == null) return;
        Instantiate(item.Prefab, SpawnPoint.position, Quaternion.identity);
    }
}
