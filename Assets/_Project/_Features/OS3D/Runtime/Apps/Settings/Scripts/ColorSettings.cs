using UnityEngine;

[CreateAssetMenu(fileName = "ColorSettings_", menuName = "LightHouse/Computer/Settings/New Color settings")]
public class ColorSettings : ScriptableObject
{
    // --- Buttons & Interactables ---
    [Header("🔘 Buttons & Interactables")]
    public Color ButtonIconFill = new Color32(0xCF, 0xE0, 0xC3, 255); // center icon click
    public Color ButtonCloseBorder = new Color32(0xD6, 0x49, 0x33, 255); // close button outline
    public Color ButtonCloseFill = new Color32(0x7B, 0x08, 0x28, 255); // close button center
    public Color ButtonBorder = new Color32(0x52, 0x2B, 0x47, 255); // other buttons border

    // --- Iconography ---
    [Header("🖼️ Icons")]
    public Color IconTint = new Color32(0xCF, 0xD6, 0xEA, 255); // general tint

    // --- Text & Backgrounds ---
    [Header("📝 Text & Background")]
    public Color InputBackground = new Color32(0xF9, 0xF9, 0xF9, 255);
    public Color TextColor = new Color32(0x00, 0x00, 0x00, 255);

    // --- App Window (UI Panels) ---
    [Header("🪟 App Window")]
    public Color WindowBackground = new Color32(0x71, 0x76, 0x8E, 255); // window base
    public Color WindowBorder = new Color32(0x50, 0x56, 0x70, 255); // outline border

    // --- Scrollbars ---
    [Header("🖱️ Scrollbars")]
    public Color ScrollbarBackground = new Color32(0xB8, 0xBE, 0xD1, 255);
    public Color ScrollbarBorder = new Color32(0x78, 0x80, 0x98, 255);


    [Header(" --- GLOBAL CONFIGS --- ")]

    [Header(" -- ELEMENTS -- ")]
    public Color Window = new Color32(0x63, 0x4F, 0x34, 255);

    [Header(" -- TEXTS -- ")]
    public Color Headers = new Color32(0xFF, 0xB6, 0x49, 255);
    public Color HeaderWindows = Color.white;
}
