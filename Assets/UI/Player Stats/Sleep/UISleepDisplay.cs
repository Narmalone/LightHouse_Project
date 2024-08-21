using System;

public class UISleepDisplay : UIDisplayStats
{
    protected override string EditValueText(float value)
    {
        return String.Format("{0:0.0}", value);
    }
}
