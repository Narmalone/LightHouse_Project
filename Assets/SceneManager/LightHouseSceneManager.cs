using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightHouseSceneManager : Singleton<LightHouseSceneManager>
{
    public enum BuildScenes
    {
        Game,
        Credits
    }

    public const string GAME_SCENE = "Game";
    public const string CREDITS_SCENE = "Credits";

    public void LoadAsync(string name)
    {
        SceneManager.LoadSceneAsync(name);
    }

    public void LoadAsync(BuildScenes target)
    {
        switch (target)
        {
            case BuildScenes.Game:
                SceneManager.LoadSceneAsync(GAME_SCENE);
                return;
            case BuildScenes.Credits:
                SceneManager.LoadSceneAsync(CREDITS_SCENE);
                return;
        }
    }


}
