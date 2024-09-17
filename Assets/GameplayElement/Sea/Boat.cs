using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SeaManager;

public class Boat : SeaElement
{
    protected override void Start()
    {
        _type = ReportedObjectType.BOAT;
        base.Start();
    }
}