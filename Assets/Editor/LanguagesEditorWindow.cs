using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class LanguagesEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private CustomVisualElement bodyElement;
    private string currentClassList;

    public List<Button> HeaderButtons = new List<Button>();

    private VisualElement gameplayPannel;

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

        SetupGameplay(root);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }

    private void BodyElement_OnClassRemoved(string obj)
    {
        if(obj == currentClassList)
        {

        }
        switch (obj)
        {
            case "Gameplay":
                gameplayPannel.style.display = DisplayStyle.None;
                break;
        }
    }

    private void BodyElement_OnClassAdded(string obj)
    {
        if (currentClassList != string.Empty && currentClassList != obj)
        {
            bodyElement.RemoveFromClassList(currentClassList);
        }
        currentClassList = obj;
        switch (obj)
        {
            case "Gameplay":
                gameplayPannel.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void GetPannels(VisualElement root)
    {
        VisualTreeAsset gameplayPannelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/LanguagesEditorWindow/GameplayPannel.uxml");
        VisualElement gp = gameplayPannelAsset.Instantiate();
        root.Add(gp);

        gameplayPannel = root.Q<VisualElement>("GameplayPannel");
    }

    public void SetupGameplay(VisualElement root)
    {
        Button computerBtn = gameplayPannel.Q<Button>("Computer");
        Button objectsBtn = gameplayPannel.Q<Button>("UsableObjects");

        computerBtn.clicked += ComputerBtn_clicked;


        SetupComputer(root);
    }

    public void SetupComputer(VisualElement root)
    {
        ObjectField computerField = gameplayPannel.Q<ObjectField>("ComputerObjectField");
        GameObject obj = computerField.value as GameObject;
        if (obj == null) return;

        ComputerController targetComputer = obj.GetComponent<ComputerController>();
        if (targetComputer == null) return;

        var ss = targetComputer._uiComputerController.messagerieWindow.AllTextsLanguages[0];
        // Liste des langues (interface)
        VisualElement languagesListContainer = new VisualElement();
        languagesListContainer.style.flexDirection = FlexDirection.Column;
        gameplayPannel.Add(languagesListContainer);

        // Bouton pour ajouter un groupe de langues
        Button addButton = new Button();
        addButton.text = "Add language group";
        addButton.clicked += () =>
        {
            ss.mLanguages.Add(new MultiLanguage { Language = Languages.EN, Value = "New Text" });
            RefreshLanguageList(languagesListContainer, targetComputer); // Rafraîchir l'UI après l'ajout
        };
        gameplayPannel.Add(addButton);

        // Bouton pour sauvegarder les changements
        Button saveChanges = new Button();
        saveChanges.text = "Save Changes";
        saveChanges.clicked += () =>
        {
            Undo.RecordObject(targetComputer, "Save Language Changes"); // Permet d'annuler
            EditorUtility.SetDirty(targetComputer); // Marquer comme modifié

            Debug.Log("Languages saved!");
            // Ajouter du code pour sauvegarder les changements si nécessaire
        };
        gameplayPannel.Add(saveChanges);

        // Remplir la liste de langues
        RefreshLanguageList(languagesListContainer, targetComputer);
    }

    private void RefreshLanguageList(VisualElement container, ComputerController targetComputer)
    {
        container.Clear(); // Effacer les anciens éléments

        // Récupérer la liste directement sans copie pour modifier l'original
        var ss = targetComputer._uiComputerController.messagerieWindow.AllTextsLanguages[0].mLanguages;

        // Pour chaque élément dans la liste des langues, créer un Foldout
        for (int i = 0; i < ss.Count; i++)
        {
            int index = i; // Stocker l'index localement pour l'utiliser dans les callbacks
            MultiLanguage languageEntry = ss[index];

            Foldout languageFoldout = new Foldout { text = $"Language {index + 1}" };
            container.Add(languageFoldout);

            // Dropdown pour sélectionner la langue
            EnumField languageEnumField = new EnumField(languageEntry.Language);
            languageEnumField.label = "Language";
            languageEnumField.Init(Languages.EN);
            languageEnumField.RegisterValueChangedCallback(evt =>
            {
                ss[index] = new MultiLanguage { Language = (Languages)evt.newValue, Value = ss[index].Value };
                EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après chaque changement
            });
            languageFoldout.Add(languageEnumField);

            // Champ texte pour entrer la valeur de la langue
            TextField valueField = new TextField("Value");
            valueField.value = languageEntry.Value;
            valueField.RegisterValueChangedCallback(evt =>
            {
                ss[index] = new MultiLanguage { Language = ss[index].Language, Value = evt.newValue };
                EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après chaque changement
            });
            languageFoldout.Add(valueField);

            // Bouton pour supprimer un élément
            Button removeButton = new Button { text = "Remove" };
            removeButton.clicked += () =>
            {
                ss.RemoveAt(index);
                RefreshLanguageList(container, targetComputer); // Rafraîchir l'UI après la suppression
                EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après la suppression
            };
            languageFoldout.Add(removeButton);
        }
    }


    //Afficher les pages de l'ordinateur et pré sélectionner la première
    private void ComputerBtn_clicked()
    {
        //setup le computer
    }

    public void GenerateHeader(VisualElement root)
    {
        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/LanguagesEditorWindow/HeaderContainer.uxml");
        VisualElement ui = uiAsset.Instantiate();
        root.Add(ui);

        HeaderButtons = root.Query<Button>("HeaderButton_").ToList();

        foreach(Button button in HeaderButtons)
        {
            button.clicked += Button_clicked;
        }

        Button mainMenu = ui.Q<Button>("HeaderButton_MainMenu");
        VisualElement gameplay = ui.Q<VisualElement>("HeaderButton_Gameplay");
        VisualElement options = ui.Q<VisualElement>("HeaderButton_Options");
        Button gameplayBtn = gameplay.Q<Button>("HeaderButton_");
        Button optionsBtn = options.Q<Button>("HeaderButton_");

        gameplayBtn.clicked += () =>
        {
            OnGameplayWindowCliqued(root);
        };

        optionsBtn.clicked += () =>
        {
            OnOptionsButtonsCliqued(root);
        };
    }

    private void Button_clicked()
    {

    }

    private void OnOptionsButtonsCliqued(VisualElement root)
    {
        bodyElement.AddToClassList("Options");
        
    }

    public void OnGameplayWindowCliqued(VisualElement root)
    {
        bodyElement.AddToClassList("Gameplay");
    }
}
