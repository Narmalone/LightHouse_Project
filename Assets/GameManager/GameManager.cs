using System;
using UnityEngine;

#region ENUMS
public enum GameZone
{
    //MAIN ZONES
    OutsideLocal,
    Bathroom,
    BedRoom,
    Kitchen,
    Office,
    Lens,
    RDC,
    OutsideLightHouse,
    Beach
}

#endregion


public class GameManager : Singleton<GameManager>
{
    [SerializeField] public GameSettings gameSettings;
    [SerializeField] private CustomEvent eventStartTimeCycle;
    [SerializeField] private CustomEvent eventNextDay;
    [SerializeField] private CustomEvent_GameZone _onGameZoneChanged;
    [SerializeField] private GameZoneTypeInfo _gameZoneTypeInfoSettings;
    public static GameZone CurrentPlayerZone;

    public static event Action OnDayCycleEnd;
    public static event Action<int> OnNightCycleEnd; //int = NewDayValue
    public static event Action<int> OnWeekStart; //int = NewDayValue

    [SerializeField] private int currentDay = 0;
    public int CurrentDay => currentDay;
    private float cycleTimeLeft;

    protected override void Awake()
    {
        base.Awake();
        eventNextDay.handle += OnDayEnd;
        _onGameZoneChanged.handle += _onGameZoneChanged_handle;
    }

    private void OnDestroy()
    {
        eventNextDay.handle -= OnDayEnd;
        _onGameZoneChanged.handle -= _onGameZoneChanged_handle;
    }

    private void Start()
    {
        CurrentPlayerZone = GameZone.OutsideLightHouse;
    }

    public TimeSpan GetCurrentInGameTime()
    {
        return TimeSpan.FromSeconds(cycleTimeLeft);
    }

    private void OnDayEnd()
    {
        currentDay++;
    }

    public GMTypeInfo GetPlayerLocation()
    {
        foreach(GameZone outsideZones in _gameZoneTypeInfoSettings.OutsideZones)
        {
            if(outsideZones == CurrentPlayerZone)
            {
                return GMTypeInfo.Outside;
            }
        }

        foreach(GameZone insideZones in _gameZoneTypeInfoSettings.InsideZones)
        {
            if(insideZones == CurrentPlayerZone)
            {
                return GMTypeInfo.Inside;
            }
        }

        return GMTypeInfo.Outside;
    }

    private void _onGameZoneChanged_handle(GameZone obj)
    {
        if (CurrentPlayerZone != obj)
            CurrentPlayerZone = obj;
    }

}