using TMPro;
using UnityEngine;

public class CurrentModeScreen : MonoBehaviour
{
    public TextMeshProUGUI modeText;

    private void Update()
    {
        modeText.text = Screen.fullScreenMode.ToString();
    }
}
