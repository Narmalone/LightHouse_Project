using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //GAMEPLAY PANNEL
    private VisualElement gameplayPannel;
    private VisualElement currentGameplayOptionDisplayed;

    //COMPUTER
    private const string MAILBOX_PANNEL_KEY = "MailBox";
    private const string METEO_PANNEL_KEY = "Meteo";
    private const string NIGHTVEIL_PANNEL_KEY = "NightVeil";
    private const string MAINTENANCE_PANNEL_KEY = "Maintenance";
    private const string SUPPLY_PANNEL_KEY = "Supply";

    private VisualElement lastComputerPageDisplayed;
    private VisualElement _mailBoxPannel;
    private VisualElement _meteoPannel;
    private VisualElement _nightVeilPannel;
    private VisualElement _maintenancePannel;
    private VisualElement _supplyPannel;

    private ComputerController _lastSelectedComputer;

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

    #region HEADER

    public void GenerateHeader(VisualElement root)
    {
        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/LanguagesEditorWindow/HeaderContainer.uxml");
        VisualElement ui = uiAsset.Instantiate();
        root.Add(ui);

        HeaderButtons = root.Query<Button>("HeaderButton_").ToList();

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

    private void OnOptionsButtonsCliqued(VisualElement root)
    {
        bodyElement.AddToClassList("Options");

    }

    public void OnGameplayWindowCliqued(VisualElement root)
    {
        bodyElement.AddToClassList("Gameplay");
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
            case "Options":
                
                break;
        }
    }
    #endregion

    public void GetPannels(VisualElement root)
    {
        VisualTreeAsset gameplayPannelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/LanguagesEditorWindow/GameplayPannel.uxml");
        VisualElement gp = gameplayPannelAsset.Instantiate();
        root.Add(gp);

        gameplayPannel = root.Q<VisualElement>("GameplayPannel");
    }

    public void SetupGameplay(VisualElement root)
    {
        VisualElement computerContainer = gameplayPannel.Q<VisualElement>("ComputerContainer"); 
        VisualElement usableContainer = gameplayPannel.Q<VisualElement>("UsableObjectsContainer"); 

        Button computerBtn = gameplayPannel.Q<Button>("Computer");
        Button objectsBtn = gameplayPannel.Q<Button>("UsableObjects");

        computerBtn.clicked += () =>
        {
            if(currentGameplayOptionDisplayed != null)
            {
                currentGameplayOptionDisplayed.style.display = DisplayStyle.None;
            }
            currentGameplayOptionDisplayed = computerContainer;

            computerContainer.style.display = DisplayStyle.Flex;
        };

        objectsBtn.clicked += () =>
        {
            if (currentGameplayOptionDisplayed != null)
            {
                currentGameplayOptionDisplayed.style.display = DisplayStyle.None;
            }
            currentGameplayOptionDisplayed = usableContainer;

            usableContainer.style.display = DisplayStyle.Flex;
        };

        SetupComputer(root);
        SetupUsableObjects(root, usableContainer);
    }

    private async void SetupUsableObjects(VisualElement root, VisualElement usableContainer)
    {
        // Charger les objets à partir d'une clé
        var result = await AddressableCustomUtility.LoadAndAssociateResultWithKey(new List<string>() { "items" }, () => { });

        foreach (var item in result)
        {
            // Créer un Foldout pour chaque item
            Foldout fold = CreateFoldoutForItem(item.Key, item.Value.Result);
            usableContainer.Add(fold);

            // Récupérer les LanguageContainer associés à l'objet
            var languageContainers = item.Value.Result.GetComponentsInChildren<LanguageContainer>();

            if (languageContainers.Length > 0)
            {
                // Rafraîchir les sections de langues pour l'objet en question
                RefreshLanguageSectionsForUsableObject(fold, item.Value.Result, languageContainers);
            }
        }
    }

    private Foldout CreateFoldoutForItem(string itemName, GameObject itemValue)
    {
        // Créer un Foldout pour un objet donné
        Foldout fold = new Foldout { text = itemName };

        // Ajouter un ObjectField pour afficher l'objet
        ObjectField itemField = new ObjectField();
        itemField.objectType = itemValue.GetType();
        itemField.SetValueWithoutNotify(itemValue);
        fold.Add(itemField);

        return fold;
    }

    private void RefreshLanguageSectionsForUsableObject(Foldout fold, GameObject itemValue, LanguageContainer[] languageContainers)
    {
        // Dictionnaire pour stocker l'état des Foldouts (ouvert/fermé) par section
        Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();

        // Sauvegarder l'état des Foldouts existants (ouverts/fermés)
        foreach (Foldout existingFoldout in fold.Children().OfType<Foldout>())
        {
            int index = fold.IndexOf(existingFoldout);
            if (index >= 0 && index < languageContainers.Length)
            {
                foldoutStates[index] = existingFoldout.value;
            }
        }

        fold.Clear(); // Effacer les anciens éléments avant de rafraîchir

        // Boucle pour chaque section de LanguageContainer
        for (int i = 0; i < languageContainers.Length; i++)
        {
            int sectionIndex = i;
            var languageGroup = languageContainers[sectionIndex];

            // Créer un Foldout pour chaque LanguageContainer
            Foldout sectionFoldout = new Foldout { text = languageGroup.gameObject.name };

            // Restaurer l'état du Foldout (ouvert/fermé)
            if (foldoutStates.ContainsKey(i))
            {
                sectionFoldout.value = foldoutStates[i];  // Restaurer l'état précédemment enregistré
            }
            else
            {
                sectionFoldout.value = false;  // Fermé par défaut si pas d'état précédent
            }

            fold.Add(sectionFoldout);

            // Conteneur pour les langues dans cette section
            VisualElement languagesListContainer = new VisualElement();
            languagesListContainer.style.flexDirection = FlexDirection.Column;
            sectionFoldout.Add(languagesListContainer);

            // Boucle pour ajouter les langues dans cette section
            for (int j = 0; j < languageGroup.Languages.Count; j++)
            {
                int languageIndex = j;
                MultiLanguage languageEntry = languageGroup.Languages[languageIndex];

                // Ajouter le dropdown pour la langue
                EnumField languageEnumField = new EnumField(languageEntry.Language);
                languageEnumField.label = "Language";
                languageEnumField.Init(Languages.EN);
                languageEnumField.RegisterValueChangedCallback(evt =>
                {
                    languageContainers[sectionIndex].Languages[languageIndex] = new MultiLanguage
                    {
                        Language = (Languages)evt.newValue,
                        Value = languageContainers[sectionIndex].Languages[languageIndex].Value
                    };
                    EditorUtility.SetDirty(itemValue);
                });
                languagesListContainer.Add(languageEnumField);

                // Champ texte pour la valeur de la langue
                TextField valueField = new TextField("Value");
                valueField.value = languageEntry.Value;
                valueField.RegisterValueChangedCallback(evt =>
                {
                    languageContainers[sectionIndex].Languages[languageIndex] = new MultiLanguage
                    {
                        Language = languageContainers[sectionIndex].Languages[languageIndex].Language,
                        Value = evt.newValue
                    };
                    EditorUtility.SetDirty(itemValue);
                });
                languagesListContainer.Add(valueField);

                // Bouton pour supprimer une langue
                Button removeButton = new Button { text = "Remove" };
                removeButton.clicked += () =>
                {
                    languageContainers[sectionIndex].Languages.RemoveAt(languageIndex);
                    RefreshLanguageSectionsForUsableObject(fold, itemValue, languageContainers);
                    EditorUtility.SetDirty(itemValue);
                };
                languagesListContainer.Add(removeButton);
            }

            // Bouton pour ajouter une nouvelle langue dans la section
            Button addLanguageButton = new Button { text = "Add language group" };
            addLanguageButton.clicked += () =>
            {
                languageContainers[sectionIndex].Languages.Add(new MultiLanguage { Language = Languages.EN, Value = "New Text" });
                RefreshLanguageSectionsForUsableObject(fold, itemValue, languageContainers);
                EditorUtility.SetDirty(itemValue);
                sectionFoldout.value = true; // Ouvre le Foldout après avoir ajouté une nouvelle langue
            };
            sectionFoldout.Add(addLanguageButton);
        }
    }


    public void SetupComputer(VisualElement root)
    {
        // Récupération des panels existants
        ObjectField computerField = gameplayPannel.Q<ObjectField>("ComputerObjectField");
        GameObject obj = computerField.value as GameObject;

        //récupérer les container sous le dropdown ou on va stocker tous les texts ect...
        _mailBoxPannel = gameplayPannel.Q<VisualElement>(MAILBOX_PANNEL_KEY);
        _meteoPannel = gameplayPannel.Q<VisualElement>(METEO_PANNEL_KEY);
        _nightVeilPannel = gameplayPannel.Q<VisualElement>(NIGHTVEIL_PANNEL_KEY);
        _maintenancePannel = gameplayPannel.Q<VisualElement>(MAINTENANCE_PANNEL_KEY);
        _supplyPannel = gameplayPannel.Q<VisualElement>(SUPPLY_PANNEL_KEY);

        if (obj == null) return;

        ComputerController targetComputer = obj.GetComponent<ComputerController>();
        if (targetComputer == null) return;
        _lastSelectedComputer = targetComputer;

        // DropdownField pour sélectionner la page à afficher dans le tool
        DropdownField computerPages = gameplayPannel.Q<DropdownField>("DropdownComputer");
        computerPages.RegisterValueChangedCallback(OnDropDownChangedSelected);

        // Ajouter le bouton pour sauvegarder les changements
        Button saveChanges = new Button();
        saveChanges.text = "Save Changes";
        saveChanges.clicked += () =>
        {
            Undo.RecordObject(targetComputer, "Save Changes");
            EditorUtility.SetDirty(targetComputer);
            Debug.Log("Changes saved!");
        };
        gameplayPannel.Add(saveChanges);

        // Initialisation des sections
        SetupPanels(targetComputer);
    }

    private void OnDropDownChangedSelected(ChangeEvent<string> evt)
    {
        if(evt.newValue != string.Empty && lastComputerPageDisplayed != null)
        {
            lastComputerPageDisplayed.style.display = DisplayStyle.None;
        }

        switch (evt.newValue)
        {
            case MAILBOX_PANNEL_KEY:
                lastComputerPageDisplayed = _mailBoxPannel;
                break;
            case METEO_PANNEL_KEY:
                lastComputerPageDisplayed = _meteoPannel;
                break;
            case NIGHTVEIL_PANNEL_KEY:
                lastComputerPageDisplayed = _nightVeilPannel;
                break;
            case MAINTENANCE_PANNEL_KEY:
                lastComputerPageDisplayed = _maintenancePannel;
                break;
            case SUPPLY_PANNEL_KEY:
                lastComputerPageDisplayed = _supplyPannel;
                break;
        }

        lastComputerPageDisplayed.style.display = DisplayStyle.Flex;
    }

    private void SetupPanels(ComputerController targetComputer)
    {

        // Création des conteneurs pour chaque section
        VisualElement messagerieContainer = new VisualElement();
        messagerieContainer.style.flexDirection = FlexDirection.Column;
        _mailBoxPannel.Add(messagerieContainer);

        VisualElement meteoContainer = new VisualElement();
        meteoContainer.style.flexDirection = FlexDirection.Column;
        _meteoPannel.Add(meteoContainer);

        // Initialisation des sections spécifiques
        SetupMessagerie(targetComputer, messagerieContainer);
        SetupMeteo(targetComputer, meteoContainer);
    }

    private void SetupMessagerie(ComputerController targetComputer, VisualElement container)
    {
        // Utiliser le texte spécifique à la messagerie
        CreateAndRefreshLanguageSections(container, targetComputer, targetComputer._uiComputerController.messagerieWindow.AllTextsLanguages);
        
    }

    // Setup pour la section Météo
    private void SetupMeteo(ComputerController targetComputer, VisualElement container)
    {
        // Créer deux Foldouts principaux pour la météo : un pour les textes fixes et un pour l'échelle de Beaufort
        Foldout weatherTextFoldout = new Foldout 
        { 
            text = "WEATHER TEXTS",
            style = { fontSize = 18 }
        };

        Foldout beaufortFoldout = new Foldout 
        { 
            text = "BEAUFORT",
            style = { fontSize = 18 }
        };

        // Ajouter les deux Foldouts au container
        container.Add(weatherTextFoldout);
        container.Add(beaufortFoldout);

        // S'assurer que lorsqu'un Foldout est ouvert, l'autre se ferme
        weatherTextFoldout.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue) // Si l'utilisateur ouvre ce Foldout
            {
                beaufortFoldout.value = false; // Fermer l'autre
            }
        });

        beaufortFoldout.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue) // Si l'utilisateur ouvre ce Foldout
            {
                weatherTextFoldout.value = false; // Fermer l'autre
            }
        });

        // Contenu du Foldout "Weather Text Information"
        VisualElement weatherTextContainer = new VisualElement();
        weatherTextFoldout.Add(weatherTextContainer);
        CreateAndRefreshLanguageSections(weatherTextContainer, targetComputer, targetComputer._uiComputerController.meteoWindow.AllFixedTexts);

        // Contenu du Foldout "Beaufort Scale"
        VisualElement beaufortContainer = new VisualElement();
        beaufortFoldout.Add(beaufortContainer);

        // Ajout d'un bouton pour ajouter un nouvel élément BeaufortScaleInfo
        Button addBeaufortButton = new Button { text = "Add Beaufort Scale Info" };
        beaufortContainer.Add(addBeaufortButton);

        addBeaufortButton.clicked += () =>
        {
            // Ajouter un nouvel élément vide à la liste
            var newScaleInfo = new BeaufortScaleInfo
            {
                ScaleLevel = "New Level",
                WavesHeight = "New Height",
                WaterDescription = "New Description",
                waterDescriptionLanguages = new List<MultiLanguage>()
            };

            // Ajouter à la liste
            targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo.Add(newScaleInfo);

            // Mettre à jour l'interface pour refléter l'ajout
            RefreshBeaufortContainer(beaufortContainer, targetComputer);
        };

        // Appel initial pour charger les éléments existants
        RefreshBeaufortContainer(beaufortContainer, targetComputer);
    }

    private void RefreshBeaufortContainer(VisualElement beaufortContainer, ComputerController targetComputer)
    {
        // Effacer le contenu existant avant de le rafraîchir
        beaufortContainer.Clear();

        // Ré-ajouter le bouton pour ajouter un nouvel élément
        Button addBeaufortButton = new Button { text = "Add Beaufort Scale Info" };
        beaufortContainer.Add(addBeaufortButton);

        addBeaufortButton.clicked += () =>
        {
            // Ajouter un nouvel élément à la liste avec des valeurs par défaut
            var newScaleInfo = new BeaufortScaleInfo
            {
                ScaleLevel = "New Level",
                WavesHeight = "New Height",
                WaterDescription = "New Description",
                waterDescriptionLanguages = new List<MultiLanguage>()
            };

            targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo.Add(newScaleInfo);
            RefreshBeaufortContainer(beaufortContainer, targetComputer);
        };

        // Ajouter tous les éléments existants de l'échelle de Beaufort
        for (int i = 0; i < targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo.Count; i++)
        {
            var currentIndex = i; // Clone de l'index
            var current = targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo[currentIndex];

            // Créer un Foldout pour chaque niveau de l'échelle Beaufort
            Foldout scaleFoldout = new Foldout { text = $"Level {current.ScaleLevel}" };
            beaufortContainer.Add(scaleFoldout);

            // Créer un conteneur pour les valeurs éditables (ScaleLevel et WavesHeight)
            VisualElement editableContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            scaleFoldout.Add(editableContainer);

            // Créer un label et un TextField pour ScaleLevel (modifiable)
            Label scaleLevelLabel = new Label { text = "Scale Level: " };
            scaleLevelLabel.style.fontSize = 12;
            editableContainer.Add(scaleLevelLabel);

            TextField scaleLevelField = new TextField();
            scaleLevelField.value = current.ScaleLevel;
            scaleLevelField.RegisterValueChangedCallback(evt =>
            {
                current.ScaleLevel = evt.newValue;
                targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo[currentIndex] = current;
            });
            editableContainer.Add(scaleLevelField);

            // Créer un label et un TextField pour WavesHeight (modifiable)
            Label waveHeightLabel = new Label { text = "Waves Height: " };
            waveHeightLabel.style.fontSize = 12;
            editableContainer.Add(waveHeightLabel);

            TextField waveHeightField = new TextField();
            waveHeightField.value = current.WavesHeight;
            waveHeightField.RegisterValueChangedCallback(evt =>
            {
                current.WavesHeight = evt.newValue;
                targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo[currentIndex] = current;
            });
            editableContainer.Add(waveHeightField);

            // Ajouter un sous-Foldout pour la gestion des langues
            Foldout languageFoldout = new Foldout { text = "Water Description Languages" };
            scaleFoldout.Add(languageFoldout);

            // Rafraîchir les sections de langues pour cet élément Beaufort
            RefreshBeaufortLanguageSections(languageFoldout, targetComputer, currentIndex);

            // Ajouter un bouton pour supprimer cet élément Beaufort
            Button removeButton = new Button { text = "Remove Beaufort Scale Info" };
            removeButton.clicked += () =>
            {
                targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo.RemoveAt(currentIndex);
                RefreshBeaufortContainer(beaufortContainer, targetComputer);
            };
            scaleFoldout.Add(removeButton);
        }
    }


    private void RefreshBeaufortLanguageSections(VisualElement container, ComputerController targetComputer, int beaufortIndex)
    {
        var beaufortInfo = targetComputer._uiComputerController.meteoWindow.WindController.BeaufortController._beaufortScaleInfo[beaufortIndex];

        // Effacer les anciens éléments avant de rafraîchir
        container.Clear();

        // Boucle pour chaque groupe de langues de la description d'eau
        for (int i = 0; i < beaufortInfo.waterDescriptionLanguages.Count; i++)
        {
            int languageIndex = i; // Clone de l'index
            var languageEntry = beaufortInfo.waterDescriptionLanguages[languageIndex];

            // Créer un Foldout pour chaque groupe de langues
            Foldout languageFoldout = new Foldout { text = $"Language: {languageEntry.Language}" };
            container.Add(languageFoldout);

            // Créer un EnumField pour changer la langue
            EnumField languageEnumField = new EnumField(languageEntry.Language);
            languageEnumField.label = "Language";
            languageEnumField.Init(languageEntry.Language);
            languageEnumField.RegisterValueChangedCallback(evt =>
            {
                beaufortInfo.waterDescriptionLanguages[languageIndex] = new MultiLanguage
                {
                    Language = (Languages)evt.newValue,
                    Value = languageEntry.Value
                };
            });
            languageFoldout.Add(languageEnumField);

            // Champ texte pour la valeur de la langue
            TextField valueField = new TextField("Description");
            valueField.value = languageEntry.Value;
            valueField.RegisterValueChangedCallback(evt =>
            {
                beaufortInfo.waterDescriptionLanguages[languageIndex] = new MultiLanguage
                {
                    Language = languageEntry.Language,
                    Value = evt.newValue
                };
            });
            languageFoldout.Add(valueField);

            // Bouton pour supprimer un groupe de langues
            Button removeButton = new Button { text = "Remove Language" };
            removeButton.clicked += () =>
            {
                beaufortInfo.waterDescriptionLanguages.RemoveAt(languageIndex);
                RefreshBeaufortLanguageSections(container, targetComputer, beaufortIndex);
            };
            languageFoldout.Add(removeButton);
        }

        // Bouton pour ajouter un nouveau groupe de langues
        Button addLanguageButton = new Button { text = "Add Language" };
        addLanguageButton.clicked += () =>
        {
            beaufortInfo.waterDescriptionLanguages.Add(new MultiLanguage { Language = Languages.EN, Value = "New Description" });
            RefreshBeaufortLanguageSections(container, targetComputer, beaufortIndex);
        };
        container.Add(addLanguageButton);
    }

    // Méthode pour rafraîchir les sections avec les langues ou autres textes
    private void CreateAndRefreshLanguageSections(VisualElement container, ComputerController targetComputer, LanguageText[] targetTexts)
    {
      
        // Dictionnaire pour stocker l'état des Foldouts (ouvert/fermé) par section
        Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();

        // Sauvegarder l'état des Foldouts existants (ouverts/fermés)
        foreach (Foldout existingFoldout in container.Children().OfType<Foldout>())
        {
            // On récupère l'index du foldout basé sur son texte (s'il est unique, ou sinon utiliser un autre moyen d'indexation)
            int index = container.IndexOf(existingFoldout);
            if (index >= 0 && index < targetTexts.Length)
            {
                foldoutStates[index] = existingFoldout.value;
            }
        }

        container.Clear(); // Effacer les anciens éléments avant de rafraîchir

        // Boucle pour chaque section de texte
        for (int i = 0; i < targetTexts.Length; i++)
        {
            int sectionIndex = i;
            var languageGroup = targetTexts[sectionIndex];

            // Créer un Foldout avec le nom du texte
            Foldout sectionFoldout = new Foldout { text = targetTexts[sectionIndex].Text.text };
            sectionFoldout.style.fontSize = 14;
            // Restaurer l'état du Foldout (ouvert/fermé)
            if (foldoutStates.ContainsKey(i))
            {
                sectionFoldout.value = foldoutStates[i];  // Restaurer l'état précédemment enregistré
            }
            else
            {
                sectionFoldout.value = false;  // Fermé par défaut si pas d'état précédent
            }

            container.Add(sectionFoldout);

            // Conteneur pour les langues dans cette section
            VisualElement languagesListContainer = new VisualElement();
            languagesListContainer.style.flexDirection = FlexDirection.Column;
            sectionFoldout.Add(languagesListContainer);

            // Boucle pour ajouter les langues dans cette section
            for (int j = 0; j < languageGroup.mLanguages.Count; j++)
            {
                int languageIndex = j;
                MultiLanguage languageEntry = languageGroup.mLanguages[languageIndex];

                // Ajouter le dropdown pour la langue
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
                });
                languagesListContainer.Add(languageEnumField);

                // Champ texte pour la valeur de la langue
                TextField valueField = new TextField("Value");
                valueField.value = languageEntry.Value;
                valueField.RegisterValueChangedCallback(evt =>
                {
                    targetTexts[sectionIndex].mLanguages[languageIndex] = new MultiLanguage
                    {
                        Language = targetTexts[sectionIndex].mLanguages[languageIndex].Language,
                        Value = evt.newValue
                    };
                });
                languagesListContainer.Add(valueField);

                // Bouton pour supprimer une langue
                Button removeButton = new Button { text = "Remove" };
                removeButton.clicked += () =>
                {
                    targetTexts[sectionIndex].mLanguages.RemoveAt(languageIndex);
                    CreateAndRefreshLanguageSections(container, targetComputer, targetTexts);
                };
                languagesListContainer.Add(removeButton);
            }

            // Bouton pour ajouter une nouvelle langue dans la section
            Button addLanguageButton = new Button { text = "Add language group" };
            addLanguageButton.clicked += () =>
            {
                targetTexts[sectionIndex].mLanguages.Add(new MultiLanguage { Language = Languages.EN, Value = "New Text" });
                CreateAndRefreshLanguageSections(container, targetComputer, targetTexts);
                sectionFoldout.value = true; // Ouvre le Foldout après avoir ajouté une nouvelle langue
            };
            sectionFoldout.Add(addLanguageButton);
        }
    }
}
