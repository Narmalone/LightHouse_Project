using UnityEngine;

[CreateAssetMenu(fileName = "MoneyConfig_", menuName = "LightHouse/LEO/NightWatch/ New Money Config")]
public class SO_NightWatchMoneyResult : ScriptableObject
{
    public int AmountWhenSuccess = 75;
    public int AmountOverTime = 100;
    public int AmountLostPerMissmatch = 15;
    public AnimationCurve DecreaseCurveOverTime = AnimationCurve.EaseInOut(0, 1, 1, 0);
}
