namespace LightHouse.Game.DayNightSystem
{
    public interface ITimeCycleObserver 
    {
        void OnTimeChanged(float timeOfDay);
    }

}
