using System;
using UnityEngine;

public class HandleGenerator : ItemBase
{
    [SerializeField] private Animator animator;

    private string forPlayer;
    public override string Name { get => forPlayer; set => forPlayer = value; }

    private bool isEnabled = false;
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (value != isEnabled)
            {
                isEnabled = value;
                OnChanged?.Invoke(isEnabled);
                UpdateHandle();
            }
        }
    }

    public event Action<bool> OnChanged; //param new Value
    [SerializeField] private CustomEvent_String eventName;

    public override bool Use()
    {
        base.Use();
        IsEnabled = !IsEnabled;
        return false;
    }

    private void Awake()
    {
        UpdateHandle();
    }

    private void UpdateHandle()
    {
        animator.SetBool("IsEnabled", IsEnabled);
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
