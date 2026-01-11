using UnityEngine;

namespace LightHouse.Features.Nightwatch
{
    [CreateAssetMenu(fileName = "Nightwatch_", menuName = "LightHouse/Computer/LEO/NightWatch/New Config")]
    public class SO_NightWatchConfiguration : ScriptableObject
    {
        [Header(" --- GLOBAL CONFIG --- ")]
        [Range(0, 24)] public float StartHour = 19.5f;
        [Range(0, 24)] public float EndHour = 4.5f;

        [Header(" --- BOATS CONFIG --- ")]
        [Range(0, 24)] public float BoatsSpawnStartHour = 20f;
        [Range(0, 24)] public float BoatsSpawnEndHour = 24.0f;

        [Header(" --- BUYONCYS CONFIG --- ")]
        [Range(0, 24)] public float BuyoncysDecayStartHour = 20f;
        [Range(0, 24)] public float BuyoncysDecayEndHour = 1.0f;
    }

}
