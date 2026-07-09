using UnityEngine;
using UnityEngine.Localization;


public class ObjectiveManager : NotPersistentSingleton<ObjectiveManager>
{
    [SerializeField] private UI_ObjectiveController _uiController;

    protected override void Awake()
    {
        base.Awake();
        if (_uiController == null) _uiController = GetComponentInChildren<UI_ObjectiveController>();
        _uiController.Hide();
    }

    public void CompleteObjective(System.Action onEnd)
    {
        _uiController.CompleteObjective(onEnd);
    }

    public void SetObjective(string objectiveDescription)
    {
        _uiController.WriteDescription(objectiveDescription);
    }

    public async void SetObjective(LocalizedString objectiveDescription)
    {
        _uiController.WriteDescription(await objectiveDescription.Resolve());
    }

    public async void SetObjective(LocalizedString objectiveDescription, params object[] arguments)
    {
        _uiController.WriteDescription(await objectiveDescription.Build(arguments));
    }
}
