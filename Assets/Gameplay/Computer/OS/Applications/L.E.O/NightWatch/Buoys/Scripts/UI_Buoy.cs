using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Buoy : MonoBehaviour
{
    public event Action<UI_Buoy> OnBuoyCliqued;

    [SerializeField] private UI_BuoyState _currentState;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _buoyIDText;

    private int _id = -1;
    public int ID
    {
        get => _id;
        set
        {
            _id = value;
            OnIdChanged();
        }
    }

    [SerializeField] private Color _uncheckedColor = Color.grey;
    [SerializeField] private Color _invalidColor = Color.red;
    [SerializeField] private Color _validColor = Color.green;

    public Button Button => _button;
    public UI_BuoyState CurrentState => _currentState;
    public bool HasBeenReportedToday { get; set; }

    private void Awake()
    {
        _button.onClick.AddListener(OnCustomButtonCliqued);
    }

    private void Start()
    {
        SwitchTo(UI_BuoyState.Unchecked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnCustomButtonCliqued);
    }

    private void OnCustomButtonCliqued()
    {
        int targetState = 0;

        if ((int)_currentState + 1 >= Enum.GetValues(typeof(UI_BuoyState)).Length || _currentState + 1 == UI_BuoyState.Failed)
            targetState = 0;
        else
            targetState = (int)_currentState + 1;

        SwitchTo((UI_BuoyState)targetState);
        OnBuoyCliqued?.Invoke(this);
    }

    private void OnIdChanged() 
    {
        _buoyIDText.text = $"Buoy: {ID.ToString("00")}";
    }

    public void SwitchTo(UI_BuoyState nextState)
    {
        switch(nextState)
        {
            case UI_BuoyState.Unchecked:
                EnterUncheckedState();
                break;

            case UI_BuoyState.Valid:
                EnterValidState();
                break;

            case UI_BuoyState.Invalid:
                EnterInvalidState();
                break;

            case UI_BuoyState.Failed:
                EnterFailedState();
                break;

            case UI_BuoyState.Reported:
                EnterReportedState();
                break;
        }

        _currentState = nextState;
    }

    private void EnterReportedState()
    {
        _button.interactable = false;
        _statusText.text = "Reported";
    }

    private void EnterUncheckedState()
    {
        _button.targetGraphic.color = _uncheckedColor;
        _statusText.text = "Unchecked";
    }

    private void EnterInvalidState()
    {
        _button.targetGraphic.color = _invalidColor;
        _statusText.text = "Invalid";
    }

    private void EnterFailedState()
    {
        _button.interactable = false;
        _statusText.text = "Failed";
    }

    private void EnterValidState()
    {
        _button.targetGraphic.color = _validColor;
        _statusText.text = "Valid";
    }
}
