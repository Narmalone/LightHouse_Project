using LightHouse.EditorTools.SuperGameManager;
using LightHouse.Features.Computer.LEO.Weather.Wind;
using UnityEngine;

namespace LightHouse.Features.Computer.LEO.Weather
{
    [CreateAssetMenu(fileName = "SO_BeaufortScale", menuName = "LightHouse/Computer/LEO/Weather/New Beaufort Config")]
    public class SO_BeaufortScale : ScriptableObject
    {
        [SgmExpose(label: "Beaufort Scale")]
        public BeaufortScale[] BeaufortDatas;

        public bool FindBeaufortDatasByWindSpeed(float windSpeed, out BeaufortScale beaufortDatas)
        {
            foreach (var data in BeaufortDatas)
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
}

