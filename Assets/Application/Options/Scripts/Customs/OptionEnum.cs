using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionEnum : MonoBehaviour
{
    public List<string> Choices = new List<string>();
    public int CurrentChoiceIndex;
    public bool EnableConstraints;
    [SerializeField] private TextMeshProUGUI _choiceText;
    public string CurrentSelectedOption => Choices[CurrentChoiceIndex];

    public event Action<int> OnValueChanged;
    public Button LeftButton;
    public Button RightButton;

    private void Awake()
    {
        LeftButton.onClick.AddListener(LeftButton_OnClick);
        RightButton.onClick.AddListener(RightButton_OnClick);
    }

    private void Start()
    {
        SetSelectedIndex(CurrentChoiceIndex);
    }

    private void RightButton_OnClick()
    {
        SetSelectedIndex(CurrentChoiceIndex + 1);
    }

    private void LeftButton_OnClick()
    {
        SetSelectedIndex(CurrentChoiceIndex - 1);
    }

    public void SetSelectedIndex(int index)
    {
        if (EnableConstraints)
            CheckConstraints(index);
        if(index < 0)
        {
            index = Choices.Count - 1;
        }
        else if(index >= Choices.Count)
        {
            index = 0;
        }
        CurrentChoiceIndex = index;
        _choiceText.text = CurrentSelectedOption;
        OnValueChanged?.Invoke(index);
    }

    public void SetValueWithoutNotify(int index)
    {
        if (EnableConstraints)
            CheckConstraints(index);
        if (index < 0)
        {
            index = Choices.Count - 1;
        }
        else if (index >= Choices.Count)
        {
            index = 0;
        }
        CurrentChoiceIndex = index;
        _choiceText.text = CurrentSelectedOption;
    }

    public void ForceRebuildUI()
    {
        _choiceText.text = CurrentSelectedOption;
    }

    public void CheckConstraints(int index)
    {
        int count = Choices.Count;

        // Aucun ou un seul choix : on bloque tout
        if (count <= 1)
        {
            LeftButton.interactable = false;
            RightButton.interactable = false;
            return;
        }

        // Plusieurs choix => comportement normal
        if (index <= 0)
        {
            LeftButton.interactable = false;
            RightButton.interactable = true;
        }
        else if (index >= count - 1)
        {
            LeftButton.interactable = true;
            RightButton.interactable = false;
        }
        else
        {
            LeftButton.interactable = true;
            RightButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        LeftButton.onClick.RemoveListener(LeftButton_OnClick);
        RightButton.onClick.RemoveListener(RightButton_OnClick);
    }

    public void AddOptions(List<string> optionsToAdd)
    {
        Choices.AddRange(optionsToAdd);
        ForceRebuildUI();
    }

}
