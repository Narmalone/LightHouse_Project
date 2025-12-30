namespace LightHouse.Game.Options
{
    public interface IOptionSetting
    {
        bool HasChanged();
        void Apply();
        void Revert();
    }

    public interface IOptionController
    {
        void Initialize();
    }
}
