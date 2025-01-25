using UnityEngine;
using static SeaManager;

public class Buoy : SeaElement
{
    protected override void Start()
    {
        _type = ReportedObjectType.BUOY;
        base.Start();
    }

    public void Initialize(string id)
    {
        _id = id;

    }
}
