using System;
using UnityEngine;

public class HandleGenerator : ItemBase
{
    [SerializeField] private Animator animator;

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

    public override void Use()
    {
        base.Use();
        IsEnabled = !IsEnabled;
    }

    private void Awake()
    {
        UpdateHandle();
    }

    private void UpdateHandle()
    {
        animator.SetBool("IsEnabled", IsEnabled);
    }
}
