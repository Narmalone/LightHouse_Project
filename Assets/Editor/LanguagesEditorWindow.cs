using System;
using System.Collections.Generic;
using System.Linq;
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
    private VisualElement lastComputerPageDisplayed;
    private VisualElement messageriePannel;
    private VisualElement meteoPannel;

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
        messageriePannel = gameplayPannel.Q<VisualElement>("Messagerie");
        meteoPannel = gameplayPannel.Q<VisualElement>("Meteo");
        if (obj == null) return;

        ComputerController targetComputer = obj.GetComponent<ComputerController>();
        if (targetComputer == null) return;

        DropdownField computerPages = gameplayPannel.Q<DropdownField>("DropdownComputer");
        computerPages.RegisterValueChangedCallback(OnDropDownChangedSelected);

        // Conteneur pour afficher la liste des sections de langues
        VisualElement languagesSectionsContainer = new VisualElement();
        languagesSectionsContainer.style.flexDirection = FlexDirection.Column;
        messageriePannel.Add(languagesSectionsContainer);

        // Bouton pour sauvegarder les changements
        Button saveChanges = new Button();
        saveChanges.text = "Save Changes";
        saveChanges.clicked += () =>
        {
            Undo.RecordObject(targetComputer, "Save Language Changes");
            EditorUtility.SetDirty(targetComputer);
            Debug.Log("Languages saved and object marked as dirty!");
        };
        gameplayPannel.Add(saveChanges);

        // Remplir la liste des sections de langues, messagerie
        SetupMessagerie(targetComputer, languagesSectionsContainer);
        SetupMeteo(targetComputer, languagesSectionsContainer);
    }

    private void OnDropDownChangedSelected(ChangeEvent<string> evt)
    {
        if(evt.newValue != string.Empty && lastComputerPageDisplayed != null)
        {
            lastComputerPageDisplayed.style.display = DisplayStyle.None;
        }

        switch (evt.newValue)
        {
            case "Messagerie":
                lastComputerPageDisplayed = messageriePannel;
                break;
            case "Météo":
                lastComputerPageDisplayed = meteoPannel;
                break;
            case "Veille de nuit":
                break;
            case "Maintenance":
                break;
            case "Ravitaillement":
                break;
        }

        lastComputerPageDisplayed.style.display = DisplayStyle.Flex;
    }

    private void SetupMessagerie(ComputerController targetComputer, VisualElement container)
    {
        RefreshLanguageSections(container, targetComputer, targetComputer._uiComputerController.messagerieWindow.AllTextsLanguages);

    }
    
    private void SetupMeteo(ComputerController targetComputer, VisualElement container)
    {
        //RefreshLanguageSections(container, targetComputer, targetComputer._uiComputerController.meteoWindow.AllFixedTexts);

    }


    private void RefreshLanguageSections(VisualElement container, ComputerController targetComputer, LanguageText[] targetTexts)
    {
        container.Clear(); // Effacer les anciens éléments

        // Pour chaque élément dans la liste AllTextsLanguages, créer un Foldout pour chaque section
        for (int i = 0; i < targetTexts.Length; i++)
        {
            int sectionIndex = i; // Stocker l'index localement pour l'utiliser dans les callbacks
            var languageGroup = targetTexts[sectionIndex];

            // Créer un Foldout pour chaque section avec un nom lisible
            Foldout sectionFoldout = new Foldout { text = targetTexts[sectionIndex].Text.text };
            container.Add(sectionFoldout);

            // Ajouter un sous-Foldout pour chaque langue de cette section
            VisualElement languagesListContainer = new VisualElement();
            languagesListContainer.style.flexDirection = FlexDirection.Column;
            sectionFoldout.Add(languagesListContainer);

            // Ajouter les langues pour cette section
            for (int j = 0; j < languageGroup.mLanguages.Count; j++)
            {
                int languageIndex = j;
                MultiLanguage languageEntry = languageGroup.mLanguages[languageIndex];

                // Foldout pour chaque langue
                Foldout languageFoldout = new Foldout { text = $"Language {languageIndex + 1}" };
                languagesListContainer.Add(languageFoldout);

                // Dropdown pour sélectionner la langue
                EnumField languageEnumField = new EnumField(languageEntry.Language);
                languageEnumField.label = "Language";
                languageEnumField.Init(Languages.EN);
                languageEnumField.RegisterValueChangedCallback(evt =>
                {
                    targetTexts[sectionIndex].mLanguages[languageIndex] = new MultiLanguage
                    {
                        Language = (Languages)evt.newValue,
                        Value = targetTexts[sectionIndex].mLanguages[languageIndex].Value
                    };
                    EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après chaque changement
                });
                languageFoldout.Add(languageEnumField);

                // Champ texte pour entrer la valeur de la langue
                TextField valueField = new TextField("Value");
                valueField.value = languageEntry.Value;
                valueField.RegisterValueChangedCallback(evt =>
                {
                    targetTexts[sectionIndex].mLanguages[languageIndex] = new MultiLanguage
                    {
                        Language = targetTexts[sectionIndex].mLanguages[languageIndex].Language,
                        Value = evt.newValue
                    };
                    EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après chaque changement
                });
                languageFoldout.Add(valueField);

                // Bouton pour supprimer un élément
                Button removeButton = new Button { text = "Remove" };
                removeButton.clicked += () =>
                {
                    targetTexts[sectionIndex].mLanguages.RemoveAt(languageIndex);
                    RefreshLanguageSections(container, targetComputer, targetTexts); // Rafraîchir l'UI après la suppression
                    EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après la suppression
                };
                languageFoldout.Add(removeButton);
            }

            // Bouton pour ajouter une langue dans cette section
            Button addLanguageButton = new Button { text = "Add language group" };
            addLanguageButton.clicked += () =>
            {
                targetTexts[sectionIndex].mLanguages.Add(new MultiLanguage { Language = Languages.EN, Value = "New Text" });
                RefreshLanguageSections(container, targetComputer, targetTexts); // Rafraîchir l'UI après l'ajout
                EditorUtility.SetDirty(targetComputer);  // Marquer l'objet comme modifié après l'ajout
            };
            sectionFoldout.Add(addLanguageButton);
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
