using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDeCon : ItemBase
{
    public override string Name => "CubeDeCon";

    public override List<IOption> GetOptions()
    {
        return new List<IOption>
        {
            new GrabOptionBase(),
            new HoldOptionBase(),
        };
    }
}
