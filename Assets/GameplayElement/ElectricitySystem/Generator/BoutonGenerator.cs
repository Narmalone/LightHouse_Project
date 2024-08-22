using System;
using UnityEngine;

public class BoutonGenerator : ItemBase
{
    private string forPlayer = "Switch to ";
    public override string Name { get => forPlayer; set => forPlayer = value; }
    private bool isEnabled = false;
    public bool IsEnabled { get => isEnabled; 
        set 
        {
            if(value != isEnabled)
            {
                isEnabled = value;
                OnChanged?.Invoke(isEnabled);
                UpdateButton();
            }
        } 
    }

    [SerializeField] private Color DisabledColor = Color.red;
    [SerializeField] private Color EnabledColor = Color.green;

    [SerializeField] private Renderer m_rend;

    public event Action<bool> OnChanged; //param new Value

    [SerializeField] private CustomEvent_String eventName;

    private void Awake()
    {
        UpdateButton();
    }

    public override bool Use()
    {
        base.Use();
        //Start Motion, sound...
        IsEnabled = !IsEnabled;
        return false;
    }

    private void UpdateButton()
    {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_BaseColor", isEnabled ? EnabledColor : DisabledColor);
        m_rend.SetPropertyBlock(propertyBlock);

        if (isEnabled)
        {
            forPlayer = "Switch to Off";
        }
        else
        {
            forPlayer = "Switch to On";
        }
        eventName?.Raise(forPlayer);
    }

}
