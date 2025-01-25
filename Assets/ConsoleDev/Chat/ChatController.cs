using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#region ENUMS
public enum ChatTabs
{
    Dev,
    Game,
    Historic,
    CurrentSelectedTab,
    ALL
}

public enum LogLevel
{
    Normal,
    Warning,
    Error
}
#endregion

public class ChatController : Singleton<ChatController>
{
    #region PRIVATE SERIALIZED FIELDS
    [SerializeField] private SO_Command[] _commands;

    [Header("-- REFERENCES -- ")]
    [SerializeField] private TextMeshProUGUI messageTextPrefab = null;
    [SerializeField] private Button messageButtonPrefab = null;
    [SerializeField] private CanvasGroup chatRenderer = null;
    [SerializeField] private CanvasGroup playerInputsRenderer = null;
    [SerializeField] private Transform contentTransform;

    [Header("PLAYER INPUTS")]
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private Button submitButton = null;

    [Header("AUTO COMPLETION SYSTEM")]
    [SerializeField] private CanvasGroup autoCompletionParent = null;
    [SerializeField] private Transform suggestionsTransform;
    [SerializeField] private GameObject suggestionPrefab = null;

    [Header("EXTENSIONS REFERENCES AND WINDOWS")]
    [SerializeField] private FpsWindowController fpsWindow = null;
    [SerializeField] private PrefabLoader prefabLoader = null;
    [SerializeField] private WorldStatsWindow worldWindow = null;
    [SerializeField] private MeteoDebbugerWindow meteoWindow = null;

    [Header("TABS")]
    [SerializeField] private ComputerTabController[] allTabs;
    [SerializeField] private ComputerTabController devTab;
    [SerializeField] private ComputerTabController gameTab;
    [SerializeField] private ComputerTabController historicTab;
    #endregion

    #region PRIVATE FIELDS
    private ComputerTabController currentSelectedTab = null;

    private Dictionary<string, Action<string[]>> commandDictionary;
    private List<string> availableCommands;
    private Dictionary<string, List<string>> variableNames;
    private List<GameObject> instantiatedControllers = new List<GameObject>();
    private Dictionary<string, (MethodInfo, Type[], object)> _functions = new Dictionary<string, (MethodInfo, Type[], object)>();

    #endregion

    public ComputerTabController[] AllTabs { get { return allTabs; } set { allTabs = value; } }
    public FpsWindowController FpsWindow => fpsWindow;
    public PrefabLoader PrefabLoader => prefabLoader;
    public WorldStatsWindow WorldWindow => worldWindow;
    public MeteoDebbugerWindow MeteoWindow => meteoWindow;
    public SO_Command[] Commands => _commands;

    #region MONOBEHAVIOUR CALLBACKS
    protected override void Awake()
    {
        base.Awake();
        SetupInputFieldListeners();
        SetupTabListeners();
        SetupSubmitButtonListeners();
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        InitializeCommands();
        InitializeFunctions();
        SwitchTab(ChatTabs.Dev);
    }

    private void OnEnable()
    {
        SendChatMessage("Console apparue avec succès", ChatTabs.Dev, "Console");
    }

    private void Start()
    {
        variableNames = GetAllVariableCommands(); //quand une scène est chargée aussi
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSubmitCliqued();
        }

        autoCompletionParent.blocksRaycasts = autoCompletionParent.alpha <= 0 ? false : true;
        autoCompletionParent.interactable = autoCompletionParent.alpha <= 0 ? false : true;

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    #endregion

    #region LISTENERS && INITIALIZE 

    private void SetupInputFieldListeners()
    {
        inputField.onSelect.AddListener((s) =>
        {
            autoCompletionParent.alpha = 1f;
            if (string.IsNullOrEmpty(s))
                inputField.text = "/";
        });

        inputField.onValueChanged.AddListener(OnInputFieldChanged);
    }

    private void SetupTabListeners()
    {
        devTab.tabBtn.onClick.AddListener(() => SwitchTab(ChatTabs.Dev));

        gameTab.tabBtn.onClick.AddListener(() => SwitchTab(ChatTabs.Game));

        historicTab.tabBtn.onClick.AddListener(() => SwitchTab(ChatTabs.Historic));
    }

    private void SetupSubmitButtonListeners()
    {
        submitButton.onClick.AddListener(OnSubmitCliqued);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        variableNames = GetAllVariableCommands();
        InitializeFunctions();
    }

    protected virtual void InitializeCommands()
    {
        commandDictionary = new Dictionary<string, Action<string[]>>();
        foreach (var cmd in _commands)
        {
            commandDictionary.Add(cmd.command, (args) => cmd.action.Execute(args, this));
        }
        availableCommands = commandDictionary.Keys.ToList();
    }

    public void InitializeFunctions()
    {
        _functions = new Dictionary<string, (MethodInfo, Type[], object)>();
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var mb in allMonoBehaviours)
        {
            var methods = mb.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ConsoleFunctionAttribute>();
                if (attr != null)
                {
                    attr.ArgumentTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (!_functions.ContainsKey(attr.Name))
                    {
                        _functions.Add(attr.Name, (method, attr.ArgumentTypes, mb));
                    }
                }
            }
        }
    }

    private void OnInputFieldChanged(string arg0)
    {
        if (string.IsNullOrEmpty(arg0))
            autoCompletionParent.alpha = 0f;
        UpdateSuggestionFromText(arg0);
    }

    private void OnSubmitCliqued()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        string inputText = inputField.text.Trim();
        string[] splitInput = inputText.Split(' ');
        string command = splitInput[0];
        string[] args = splitInput.Length > 1 ? splitInput[1..] : new string[0];

        if (command.StartsWith("/f"))
        {
            if (splitInput.Length < 2)
            {
                SendChatMessage("Invalid function call. Please provide a function name.", ChatTabs.Dev, logLevel: LogLevel.Error);
                return;
            }

            string functionName = splitInput[1];
            string[] fArgs = inputText.Substring($"/f {functionName}".Length).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (_functions.TryGetValue(functionName, out var functionInfo))
            {
                MethodInfo method = functionInfo.Item1;
                Type[] argumentTypes = functionInfo.Item2;
                object instance = functionInfo.Item3;

                if (fArgs.Length != argumentTypes.Length)
                {
                    SendChatMessage($"Invalid number of arguments for function {functionName}", ChatTabs.Dev, logLevel: LogLevel.Error);
                    return;
                }

                object[] parameters = new object[fArgs.Length];
                for (int i = 0; i < fArgs.Length; i++)
                {
                    try
                    {
                        parameters[i] = Convert.ChangeType(fArgs[i], argumentTypes[i]);
                    }
                    catch (Exception ex)
                    {
                        SendChatMessage($"Invalid argument type for argument {i + 1} of function {functionName}. Expected type: {argumentTypes[i].Name}. Error: {ex.Message}", ChatTabs.Dev, logLevel: LogLevel.Error);
                        return;
                    }
                }

                method.Invoke(instance, parameters);
                SendChatMessage(inputText, ChatTabs.Historic, "Historic");
                SendChatMessage($"Function called {functionName} with number of args {fArgs.Length}", ChatTabs.Dev);
            }
            else
            {
                SendChatMessage($"Unknown function: {functionName}", ChatTabs.Dev, logLevel: LogLevel.Error);
            }
        }
        else
        {
            if (commandDictionary.ContainsKey(command))
            {
                commandDictionary[command].Invoke(args);
                SendChatMessage(inputText, ChatTabs.Historic, "Historic");
            }
            else
            {
                SendChatMessage($"Unknown command: {command}", ChatTabs.Dev, logLevel: LogLevel.Error);
            }
        }
        if (autoCompletionParent.alpha > 0f) autoCompletionParent.alpha = 0f;
    }
    #endregion

    #region GET FUNCS
    public List<string> GetAllSceneNames()
    {
        List<string> sceneNames = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            sceneNames.Add(SceneManager.GetSceneAt(i).name);
        }
        return sceneNames;
    }

    public Dictionary<string, List<string>> GetAllVariableCommands()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        Dictionary<string, List<string>> varWithAttributes = new Dictionary<string, List<string>>();

        foreach (var mb in allMonoBehaviours)
        {
            IEnumerable<FieldInfo> fields = mb.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.IsDefined(typeof(ConsoleVariableAttribute), false));

            foreach (FieldInfo field in fields)
            {
                ConsoleVariableAttribute consoleVariableAttribute = (ConsoleVariableAttribute)field.GetCustomAttribute(typeof(ConsoleVariableAttribute));
                ConsoleCategoryAttribute consoleVariableCategoryAttribute = (ConsoleCategoryAttribute)field.GetCustomAttribute(typeof(ConsoleCategoryAttribute));

                string variableName = !string.IsNullOrEmpty(consoleVariableAttribute.Name) ? consoleVariableAttribute.Name : field.Name;
                string category = consoleVariableCategoryAttribute?.Category ?? $"Gameplay";

                if (!varWithAttributes.ContainsKey(category))
                {
                    varWithAttributes.Add(category, new List<string>());
                }

                varWithAttributes[category].Add(variableName);
            }
        }
        return varWithAttributes;
    }
    
    public List<ComputerTabController> GetTargetTabs(ChatTabs where)
    {
        List<ComputerTabController> targetTabs = new List<ComputerTabController>();
        switch (where)
        {
            case ChatTabs.Game:
                targetTabs.Add(gameTab);
                break;
            case ChatTabs.Dev:
                targetTabs.Add(devTab);
                break;
            case ChatTabs.Historic:
                targetTabs.Add(historicTab);
                break;
            case ChatTabs.CurrentSelectedTab:
                targetTabs.Add(currentSelectedTab);
                break;
            case ChatTabs.ALL:
                targetTabs.Add(gameTab);
                targetTabs.Add(historicTab);
                break;
        }
        return targetTabs;
    }

    #endregion

    #region ACTIONS FUNCS
    public void TryGenerateObject(string name, float dst = 3f, Vector3 pos = default)
    {
        GameObject prefabToInstantiate = prefabLoader.GetObjectFromDictionnaryByName(name);
        // Calculate the position of the object in front of the camera
        Vector3 cameraForward = Camera.main.transform.forward;
        pos.y = Camera.main.transform.position.y;
        Vector3 objPosition = Camera.main.transform.position + cameraForward * dst;

        // Instantiate the object at the calculated position
        GameObject obj = Instantiate(prefabToInstantiate, objPosition, Quaternion.identity);
        instantiatedControllers.Add(obj);
    }
    #endregion

    #region SUGGESTION FUNCTIONS
    private void UpdateSuggestionFromText(string inputFieldText)
    {
        foreach (Transform child in suggestionsTransform)
        {
            Destroy(child.gameObject);
        }

        if (string.IsNullOrEmpty(inputFieldText)) return;

        // Filter available commands based on input
        List<string> suggestions = availableCommands.Where(cmd => cmd.StartsWith(inputFieldText, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (SO_Command cmd in _commands)
        {
            bool startWithCustomCommand = inputFieldText.StartsWith(cmd.command + " ", StringComparison.OrdinalIgnoreCase);
            if (!startWithCustomCommand) continue;
            string variableInput = inputFieldText.Substring(cmd.GetSubstringValue() + 1);
            IEnumerable<string> variableSuggestions;

            switch (cmd.suggestionSource)
            {
                case SO_Command.SuggestionSource.VariableNames:
                    variableSuggestions = variableNames
                        .Where(var => var.Key == cmd.keyName && var.Value.Any(v => v.StartsWith(variableInput, StringComparison.OrdinalIgnoreCase)))
                        .SelectMany(var => var.Value, (category, variable) => $"{cmd.command} {variable}");
                    break;
                case SO_Command.SuggestionSource.SceneNames:
                    variableSuggestions = GetAllSceneNames()
                        .Where(sceneName => sceneName.StartsWith(variableInput, StringComparison.OrdinalIgnoreCase))
                        .Select(sceneName => $"{cmd.command} {sceneName}");
                    break;
                case SO_Command.SuggestionSource.ObjectNames:
                    variableSuggestions = prefabLoader.DictionnaryObjects.Keys
                        .Where(objName => objName.StartsWith(variableInput, StringComparison.OrdinalIgnoreCase))
                        .Select(objName => $"{cmd.command} {objName}");
                    break;
                default:
                    throw new NotImplementedException($"Suggestion source '{cmd.suggestionSource}' is not implemented.");
            }

            foreach (string suggestion in variableSuggestions)
            {
                if (!suggestions.Contains(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }
        }

        if (inputFieldText.StartsWith("/f", StringComparison.OrdinalIgnoreCase))
        {
            string[] splitInput = inputFieldText.Split(' ');
            if (splitInput.Length >= 2)
            {
                string partialFunctionName = splitInput[1].ToLowerInvariant();
                var matchingFunctions = _functions
                   .Where(f => f.Key.ToLowerInvariant().Contains(partialFunctionName))
                   .OrderBy(f => f.Key.Length) // prioritize exact matches
                   .ThenBy(f => f.Key); // then sort alphabetically

                foreach (var func in matchingFunctions)
                {
                    string suggestion = $"/f {func.Key}";
                    if (!suggestions.Contains(suggestion))
                    {
                        suggestions.Add(suggestion);
                    }
                    if (splitInput.Length > 2)
                    {
                        string functionInput = string.Join(" ", splitInput, 2, splitInput.Length - 2);
                        var methodInfo = func.Value.Item1; // Get the MethodInfo object
                        var parameters = methodInfo.GetParameters();
                        foreach (var parameter in parameters)
                        {
                            if (parameter.Name.StartsWith(functionInput, StringComparison.OrdinalIgnoreCase))
                            {
                                suggestion = $"/f {func.Key} {parameter.ParameterType.Name}";
                                if (!suggestions.Contains(suggestion))
                                {
                                    suggestions.Add(suggestion);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Suggest all function names
                foreach (var func in _functions)
                {
                    suggestions.Add($"/f {func.Key}");
                }
            }
        }

        autoCompletionParent.alpha = suggestions.Count <= 0 ? 0f : 1f;
        foreach (var suggestion in suggestions.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
        {
            var suggestionInstance = Instantiate(suggestionPrefab, suggestionsTransform);
            suggestionInstance.GetComponentInChildren<TextMeshProUGUI>().text = suggestion;
            suggestionInstance.GetComponent<Button>().onClick.AddListener(() => OnSuggestionClicked(suggestion));
        }
    }

    private void OnSuggestionClicked(string suggestion)
    {
        inputField.text = suggestion;
        inputField.caretPosition = suggestion.Length;
        inputField.Select();
    }
    #endregion

    #region HIDE & SHOW FUNCTIONS

    public void ShowChat()
    {
        chatRenderer.alpha = 1.0f;
    }

    public void HideChat()
    {
        chatRenderer.alpha = 0.0f;
    }

    public void HidePlayersChatsInputs()
    {
        playerInputsRenderer.interactable = false;
    }

    public void ShowPlayersChatsInputs()
    {
        playerInputsRenderer.interactable = true;
    }

    #endregion

    #region SWITCH TABS
    public void SwitchTab(ChatTabs nextTab)
    {
        if (currentSelectedTab != null)
        {
            currentSelectedTab.HideTabMessages();
            currentSelectedTab.tabBtn.interactable = true;
        }
        else
        {
            currentSelectedTab = devTab;
        }

        switch (nextTab)
        {
            case ChatTabs.Game:
                currentSelectedTab = gameTab;
                break;
            case ChatTabs.Historic:
                currentSelectedTab = historicTab;
                break;
            case ChatTabs.Dev:
                currentSelectedTab = devTab;
                break;
        }

        currentSelectedTab.tabBtn.interactable = false;
        currentSelectedTab.ShowTabMessages();
    }

    #endregion

    #region CHAT MESSAGES FUNCTIONS
    public void SendChatMessage(string message, ChatTabs where, string fromWho = "Console", LogLevel logLevel = LogLevel.Normal)
    {
        List<ComputerTabController> targetTabs = GetTargetTabs(where);
        foreach (ComputerTabController targetTab in targetTabs)
        {
            if (targetTab != null)
            {
                bool mustEnableMessage = targetTab == currentSelectedTab;
                targetTab.OnAddMessage(messageTextPrefab,
                    $"{fromWho}: {message}",
                    mustEnableMessage ? contentTransform : targetTab.transform,
                    mustEnableMessage,
                    logLevel);
            }
        }
        inputField.text = string.Empty;
    }

    public void SendButtonChatMessage(string message, Action act, ChatTabs where, string fromWho = "Console", LogLevel logLevel = LogLevel.Normal)
    {
        List<ComputerTabController> targetTabs = GetTargetTabs(where);
        foreach (ComputerTabController targetTab in targetTabs)
        {
            if (targetTab != null)
            {
                bool mustEnableMessage = targetTab == currentSelectedTab;
                targetTab.OnAddMessage(messageButtonPrefab, 
                    act,
                    $"{fromWho} {message}",
                    mustEnableMessage ? contentTransform : targetTab.transform,
                    mustEnableMessage,
                    logLevel);
            }
        }
        inputField.text = string.Empty;
    }
    #endregion

    #region CLEAR FUNCS

    public void ClearAll()
    {
        foreach(ComputerTabController tab in allTabs)
        {
            tab.ClearTab();
        }
    }

    public void ClearChat(List<ChatTabs> which)
    {
        foreach(var chatTab in which)
        {
            switch (chatTab)
            {
                case ChatTabs.Game:
                    gameTab.ClearTab();
                    break;
                case ChatTabs.Dev:
                    devTab.ClearTab();
                    break;
                case ChatTabs.Historic:
                    historicTab.ClearTab(); 
                    break;
                case ChatTabs.CurrentSelectedTab:
                    currentSelectedTab.ClearTab();
                    break;
            }
        }
    }

    public void ClearChat(ChatTabs which)
    {
        switch (which)
        {
            case ChatTabs.Game:
                gameTab.ClearTab();
                break;
            case ChatTabs.Dev:
                devTab.ClearTab();
                break;
            case ChatTabs.Historic:
                historicTab.ClearTab();
                break;
            case ChatTabs.CurrentSelectedTab:
                currentSelectedTab.ClearTab();
                break;
        }
    }
    #endregion

}
