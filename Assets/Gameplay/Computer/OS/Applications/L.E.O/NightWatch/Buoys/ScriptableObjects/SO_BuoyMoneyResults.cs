using UnityEngine;

[CreateAssetMenu(fileName = "Money_Buoy_Config_", menuName = "LightHouse/LEO/NightWatch/Buoys/New Money Config")]
public class SO_BuoyMoneyResults : ScriptableObject
{
    public int CorrectValidBuoyReport = 10;
    public int CorrectInvalidReport = 15;
    public int AmountLostPerMissmatch = 5;
    public AnimationCurve DecreaseCurveOverTime = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public int MaxMoneyOnOverTime = 20;

}
