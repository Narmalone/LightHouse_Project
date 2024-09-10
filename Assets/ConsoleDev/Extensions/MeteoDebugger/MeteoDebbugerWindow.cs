using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoDebbugerWindow : MonoBehaviour
{
    [SerializeField] private CustomEvent _onDayWeatherInitialized;
    [SerializeField] private GameSettings _gameSettings;

    [Header("HUMIDITY")]
    [SerializeField] private GraphMeteoDebugger humidityGraph;

    [Header("AIR TEMP")]
    [SerializeField] private GraphMeteoDebugger airTempGraph;

    [Header("WATER TEMP")]
    [SerializeField] private GraphMeteoDebugger waterTempGraph;

    [Header("ATMOSPHERIC PRESSURE")]
    [SerializeField] private GraphMeteoDebugger atmosGraph;

    private WeatherManager _weatherManager;
    private WeatherForDaysManager _dayWeathersManager;

    public List<float> humiditys = new List<float>();
    public List<float> atmosphericsPressures = new List<float>();
    public List<float> airTemps = new List<float>();
    public List<float> waterTemps = new List<float>();

    private void Awake()
    {
        _onDayWeatherInitialized.handle += _onDayWeatherInitialized_handle;
    }

    private void OnDestroy()
    {
        _onDayWeatherInitialized.handle -= _onDayWeatherInitialized_handle;
    }

    private void _onDayWeatherInitialized_handle()
    {
        for (int i = 0; i < _dayWeathersManager.MorningX.Count; i++)
        {
            humiditys.Add(_dayWeathersManager.MorningX[i].humidity);
            atmosphericsPressures.Add(_dayWeathersManager.MorningX[i].atmosphericPressure);
            airTemps.Add(_dayWeathersManager.MorningX[i].airTemperature);
            waterTemps.Add(_dayWeathersManager.MorningX[i].waterTemperature);
        }

        InitValues(humiditys, _gameSettings.TotalDays, humidityGraph.Grid, humidityGraph.Line, _weatherManager.MinHumidity, _weatherManager.MaxHumidity);
        InitValues(airTemps, _gameSettings.TotalDays, airTempGraph.Grid, airTempGraph.Line, _weatherManager.MinAirTemperature, _weatherManager.MaxAirTemperature);
        InitValues(waterTemps, _gameSettings.TotalDays, waterTempGraph.Grid, waterTempGraph.Line, _weatherManager.MinWaterTemperature, _weatherManager.MaxWaterTemperature);
        InitValues(atmosphericsPressures, _gameSettings.TotalDays, atmosGraph.Grid, atmosGraph.Line, _weatherManager.MinAtmosphericPressure, _weatherManager.MaxAtmosphericPressure);
    }

    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
        _dayWeathersManager= WeatherForDaysManager.Instance;

        InitScales(humidityGraph, _weatherManager.MinHumidity, _weatherManager.MaxHumidity / 2f, _weatherManager.MaxHumidity, "%");
        InitScales(airTempGraph, _weatherManager.MinAirTemperature, _weatherManager.MaxAirTemperature / 2f, _weatherManager.MaxAirTemperature, "°C");
        InitScales(waterTempGraph, _weatherManager.MinWaterTemperature, _weatherManager.MaxWaterTemperature / 2f, _weatherManager.MaxWaterTemperature, "°C");
        InitScales(atmosGraph, _weatherManager.MinAtmosphericPressure, (_weatherManager.MinAtmosphericPressure +_weatherManager.MaxAtmosphericPressure) / 2, _weatherManager.MaxAtmosphericPressure, "hPA");
    }

    private void InitScales(GraphMeteoDebugger graph, float min, float median, float max, string suffix, Color background = default, Color lines = default)
    {
        graph.ScaleMinTxt.text = min.ToString() + suffix;
        graph.ScaleMedianTxt.text = median.ToString() + suffix;
        graph.ScaleMaxTxt.text = max.ToString() + suffix;
        graph.MaxDayTxt.text = _gameSettings.TotalDays.ToString();

        //set aussi les couleurs
    }

    private void InitValues(List<float> values, int totalDays, UiGridRenderer grid, UiLineRenderer line, float expectedMinValue, float expectedMaxValue)
    {
        grid.gridSize = new Vector2Int(totalDays, 2);
        line.gridSize = new Vector2Int(totalDays, 2);
        line.points = new List<Vector2>();

        int yGridSize = grid.gridSize.y;

        for (int i = 0; i < values.Count; i++)
        {
            // Normaliser en tenant compte de la plage [expectedMinValue, expectedMaxValue]
            float normalizedValue = ((values[i] - expectedMinValue) / (expectedMaxValue - expectedMinValue)) * yGridSize;
            line.points.Add(new Vector2(i, normalizedValue));
        }

        line.SetVerticesDirty(); // Forcer la mise à jour des vertices
    }


}
