using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ItemBaseAnim : ItemBase
{
    [Header("ITEM BASE ANIM")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected string paramBoolName;
    [SerializeField] protected CustomEvent_String eventName;

    [HideInInspector] public bool isEnabled = false;
    public event Action<bool> OnEnabledChange; //param: new value
    public bool IsEnabled { get { return isEnabled; } 
        set
        {
            if(isEnabled != value)
            {
                isEnabled = value;
                OnEnabledChange?.Invoke(isEnabled);
                ChangeAnim();
            }
        } 
    }

    public override bool Use()
    {
        IsEnabled = !IsEnabled;
        OnUse?.Invoke();
        return false;
    }

    public virtual void ChangeAnim()
    {
        animator.SetBool(paramBoolName, isEnabled);
    }
}
