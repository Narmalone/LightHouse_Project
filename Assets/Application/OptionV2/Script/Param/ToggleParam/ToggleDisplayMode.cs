using UnityEngine;

public class ToggleDisplayMode : ToggleParameter
{
    new void True() => Screen.fullScreen = true;
    new void False() => Screen.fullScreen = false;
}
