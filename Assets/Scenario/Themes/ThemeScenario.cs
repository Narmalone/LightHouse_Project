using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "ScenarioSystem/Themes/NewTheme")]
public class ThemeScenario : ScriptableObject
{
    public List<ActEvents> actEvents;
}
