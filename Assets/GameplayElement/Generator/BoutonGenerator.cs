using System;
using UnityEngine;

public class BoutonGenerator : ItemBase
{
    public override string Name => $"Switch ";
    private bool isEnabled = false;
    public bool IsEnabled { get => isEnabled; 
        set 
        {
            if(value != isEnabled)
            {
                isEnabled = value;
                OnChanged?.Invoke(isEnabled);
                UpdateColor();
            }
        } 
    }

    [SerializeField] private Color DisabledColor = Color.red;
    [SerializeField] private Color EnabledColor = Color.green;

    [SerializeField] private Renderer m_rend;

    public event Action<bool> OnChanged; //param new Value

    private void Awake()
    {
        UpdateColor();
    }

    public override void Use()
    {
        base.Use();
        //Start Motion, sound...
        IsEnabled = !IsEnabled;
    }

    private void UpdateColor()
    {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_BaseColor", isEnabled ? EnabledColor : DisabledColor);
        m_rend.SetPropertyBlock(propertyBlock);
    }

}
