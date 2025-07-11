using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Computer.NightWatch.Sonar
{
    public class SonarHandlerData
    {
        public static List<ISonarable> SonarItems = new List<ISonarable>();
        public static Sonar Sonar;
        public static void Register(ISonarable b) => SonarItems.Add(b);
        public static void Unregister(ISonarable b) => SonarItems.Remove(b);
    }

}
