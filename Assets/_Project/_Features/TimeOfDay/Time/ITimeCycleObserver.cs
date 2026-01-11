namespace LightHouse.Features.TimeOfDay.TimeCore
{
    public interface ITimeCycleObserver 
    {
        void OnTimeChanged(float timeOfDay);
    }

}
