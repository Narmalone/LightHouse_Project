using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneLoaderButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private string _sceneName;

    private void OnValidate()
    {
        if(button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void Awake()
    {
        if(button != null)
        {
            button.onClick.AddListener(() =>
            {
                LightHouseSceneManager.Instance.LoadAsync(_sceneName);
            });
        }   
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
