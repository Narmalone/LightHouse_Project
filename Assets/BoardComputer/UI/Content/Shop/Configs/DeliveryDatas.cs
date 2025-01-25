using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryDatas : ScriptableObject
{
    [Range(0, 23)] public int SendHour = 0;
    //faire un min max de combien de jours / heures et minutes ça peut prendre la livraison
    public byte minHoursToWait = 48;
    public byte maxHoursToWait = 72;

    public byte baseHoursToWait = 36;
}
