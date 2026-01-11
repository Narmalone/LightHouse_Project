namespace LightHouse.Core.Settings
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
