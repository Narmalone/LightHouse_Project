using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LanguagesEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private CustomVisualElement bodyElement;

    [MenuItem("Window/UI Toolkit/LanguagesEditorWindow")]
    public static void ShowExample()
    {
        LanguagesEditorWindow wnd = GetWindow<LanguagesEditorWindow>();
        wnd.titleContent = new GUIContent("LanguagesEditorWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        GenerateHeader(root);

        bodyElement = new CustomVisualElement();
        bodyElement.OnClassAdded += BodyElement_OnClassAdded;
        bodyElement.OnClassRemoved += BodyElement_OnClassRemoved;

        GetPannels(root);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }

    private void BodyElement_OnClassRemoved(string obj)
    {
        Debug.Log("class removed" + obj);
        switch (obj)
        {
            case "Gameplay":
                Debug.Log("gameplay added");
                break;
        }
    }

    private void BodyElement_OnClassAdded(string obj)
    {
        Debug.Log("class added " + obj);
    }

    public void GetPannels(VisualElement root)
    {
        VisualElement gameplay = root.Q<VisualElement>("GameplayPannel");
        Debug.Log("get pannel");

    }

    public void GenerateHeader(VisualElement root)
    {
        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/LanguagesEditorWindow/HeaderContainer.uxml");
        VisualElement ui = uiAsset.Instantiate();
        root.Add(ui);

        Button mainMenu = ui.Q<Button>("HeaderButton_MainMenu");
        VisualElement gameplay = ui.Q<VisualElement>("HeaderButton_Gameplay");
        Button gameplayBtn = gameplay.Q<Button>("HeaderButton_");

        gameplayBtn.clicked += () =>
        {
            OnGameplayWindowCliqued(root);
        };
    }

    public void OnGameplayWindowCliqued(VisualElement root)
    {
        bodyElement.AddToClassList("Gameplay");
        Debug.Log(bodyElement.ClassListContains("Gameplay"));
    }
}
