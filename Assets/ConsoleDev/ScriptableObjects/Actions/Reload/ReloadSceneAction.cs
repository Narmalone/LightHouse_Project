using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "NewReloadSceneAction", menuName = "CustomConsole/Action/ReloadSceneAction")]
public class ReloadSceneAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController i)
    {
        if (args.Length != 1)
        {
            i.SendChatMessage("Usage: /reload {sceneName}", ChatTabs.Dev, "Console", logLevel: LogLevel.Warning);
            return;
        }
        string sceneName = args[0];
        if (!i.GetAllSceneNames().Contains(sceneName))
        {
            i.SendChatMessage($"Scene '{sceneName}' not found!", ChatTabs.Dev, logLevel: LogLevel.Error);
            return;
        }
        if(SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName).completed += (s) =>
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            };
            return;
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
        

    }
}
