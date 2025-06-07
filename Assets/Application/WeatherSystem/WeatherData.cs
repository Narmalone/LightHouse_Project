using UnityEngine;

namespace LightHouse.Weather
{
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Rainy,
        Windy,
        Foggy,
        Snowy,
        Stormy,
    }

    public enum WindOrientationType
    {
        North,
        North_East,
        East,
        South_East,
        South,
        South_West,
        West,
        North_West
    }

    [System.Serializable]
    public class WeatherData
    {
        /// <summary>
        /// Automatically determined
        /// </summary>
        public WeatherType WeatherType;

        /// <summary>
        /// Automatically Determined by the <see cref="WindOrientation"/>
        /// </summary>
        public WindOrientationType WindOrientationType;

        /// <summary>
        /// The time whenn the meteo start
        /// </summary>
        public float StartTimeInSeconds;

        /// <summary>
        /// The total duration of this Meteo
        /// </summary>
        public float DurationInSeconds;

        /// <summary>
        /// The humidity value from 0 to 100
        /// </summary>
        public float Humidity; 

        /// <summary>
        /// The Atmospheric pressure from 980 to 1030
        /// </summary>
        public float AtmosphericPressure; // hPa

        /// <summary>
        /// Wind speed in km/h
        /// </summary>
        public float WindSpeed;

        /// <summary>
        /// Wind Orientation degree 0 to 360°
        /// </summary>
        public float WindOrientation; //360°

        /// <summary>
        /// Water Temperature
        /// </summary>
        public float WaterTemperature; //°C

        /// <summary>
        /// Air Temperature
        /// </summary>
        public float AirTemperature; //°C
    }

}
