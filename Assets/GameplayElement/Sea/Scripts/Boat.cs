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