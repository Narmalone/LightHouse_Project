using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHungerDisplay : UIDisplayStats
{
    protected override string EditValueText(float value)
    {
        return value > 100 ? "100+" : String.Format("{0:0.0}", value);
    }
}
