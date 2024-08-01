using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabBtnDisplay : MonoBehaviour
{
    [SerializeField] private Button btn;
    public Button Button => btn;

    public ComputerTabs TabToDisplay;

    public event Action<ComputerTabs> OnTabClicked;

    private void Awake()
    {
        btn.onClick.AddListener(() =>
        {
            OnTabClicked?.Invoke(this.TabToDisplay);
        });
    }
}
