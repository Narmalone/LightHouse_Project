using UnityEngine;

public class FunctionsConsole : MonoBehaviour
{
    [ConsoleFunction("TimeSpeed")]
    public void ChangeTimeSpeed(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }

    [ConsoleFunction]
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    [ConsoleFunction]
    public void UnPauseGame()
    {
        Time.timeScale = 1f;
    }

    [ConsoleFunction]
    public void Load()
    {
        
    }

    [ConsoleFunction]
    public void Save()
    {
        
    }

    [ConsoleFunction]
    public void Quit()
    {
        Application.Quit();
    }
}
