using LightHouse.Game.Computer.LEO.Weather.Wind;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_BeaufortScale", menuName = "LightHouse/Computer/LEO/Weather/New Beaufort Config")]
public class SO_BeaufortScale : ScriptableObject
{
    public BeaufortScale[] BeaufortDatas;

    public bool FindBeaufortDatasByWindSpeed(float windSpeed, out BeaufortScale beaufortDatas)
    {
        foreach(var data in BeaufortDatas)
        {
            if (data.Matches(windSpeed))
            {
                beaufortDatas = data;
                return true;
            }
        }

        beaufortDatas = new BeaufortScale();
        return false;
    }
}


