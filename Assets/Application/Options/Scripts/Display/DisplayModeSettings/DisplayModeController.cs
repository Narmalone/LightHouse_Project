using LightHouse.Game.Options;
using System.Collections.Generic;

public class DisplayModeController : IOptionController
{
    private readonly EnumBinding<DisplayModeOption> _binding;
    private readonly DisplayModeSetting _setting;

    public DisplayModeController(OptionEnum widget, DisplayModeSetting setting)
    {
        _setting = setting;

        var order = new List<DisplayModeOption>
        {
            DisplayModeOption.Fullscreen,
            DisplayModeOption.Borderless,
            DisplayModeOption.MaximizedWindow,
            DisplayModeOption.Windowed
        };

        _binding = new EnumBinding<DisplayModeOption>(
            widget,
            getSelected: () => _setting.HasChanged() ? _setting.GetType().GetField("_selected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_setting) is DisplayModeOption s ? s : DisplayModeOption.Windowed : DisplayModeOption.Windowed, // (si tu veux, expose un getter Selected dans le setting)
            setSelected: v => _setting.SetSelected(v),
            toLabel: v => v switch
            {
                DisplayModeOption.Fullscreen => "Fullscreen",
                DisplayModeOption.Borderless => "Borderless",
                DisplayModeOption.MaximizedWindow => "Maximized Window",
                _ => "Windowed"
            },
            order: order
        );
    }

    public void Initialize() => _binding.Initialize();
    public void UpdateLanguage() => _binding.UpdateLanguage();
}
