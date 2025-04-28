namespace LightHouse.Game.Options
{
    public interface IOptionSetting
    {
        bool HasChanged();
        void Apply();
        void Revert();
        IOptionSetting GetSetting();
    }
}
