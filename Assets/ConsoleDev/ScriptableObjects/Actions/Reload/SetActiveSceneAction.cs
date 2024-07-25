using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SetActiveSceneAction", menuName = "CustomConsole/Action/SetActiveSceneAction")]
public class SetActiveSceneAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController i)
    {
        string sceneName = args[0];
        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if (targetScene != null && targetScene.isLoaded)
        {
            SceneManager.SetActiveScene(targetScene);
        }
    }
}
