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

    private void SetSelectedIndex(int index)
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
    }

    public void ForceRebuildUI()
    {
        _choiceText.text = CurrentSelectedOption;
    }

    public void CheckConstraints(int index)
    {
        if(index == 0)
        {
            if(!RightButton.interactable)
                RightButton.interactable = true;
            LeftButton.interactable = false;
        }
        else if(index == Choices.Count - 1)
        {
            if (!LeftButton.interactable)
                LeftButton.interactable = true;
            RightButton.interactable = false;
        }
        else
        {
            if(!LeftButton.interactable)
                LeftButton.interactable = true;
            if(RightButton.interactable)
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
