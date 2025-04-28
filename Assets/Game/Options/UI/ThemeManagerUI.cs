using UnityEngine;
using UnityEngine.UIElements;

public class ThemeManagerUI : MonoBehaviour
{
    public VisualTreeAsset uiAsset; // <-- Peut être supprimé en fait
    public StyleSheet lightTheme;
    public StyleSheet darkTheme;

    private VisualElement root;
    private bool isDark = false;

    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.Add(new Button());
        // Pas besoin de faire Add(CloneTree()) ici

        ApplyTheme(lightTheme);

        Button switchButton = root.Q<Button>("SwitchThemeButton");
        if (switchButton != null)
        {
            switchButton.clicked += SwitchTheme;
        }
    }

    void SwitchTheme()
    {
        isDark = !isDark;
        ApplyTheme(isDark ? darkTheme : lightTheme);
        Debug.Log("switch cliqued");
    }

    void ApplyTheme(StyleSheet theme)
    {
        root.styleSheets.Clear();
        root.styleSheets.Add(theme);
    }
}
