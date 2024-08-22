using System;
using UnityEngine;

public class FuelLid : ItemBase
{
    private string forPlayer = "Close";
    public override string Name { get => forPlayer; set => forPlayer = value; }
    private bool isOpen = false;
    public bool IsOpen {  get { return isOpen; }
        set
        {
            if (value != isOpen)
            {
                isOpen = value;
                OnChanged?.Invoke(isOpen);
                UpdateOpen();
            }
        }
    }
    public event Action<bool> OnChanged; //param new Value
    [SerializeField] private Animator animator;

    [SerializeField] private CustomEvent_String eventName;

    private void Awake()
    {
        UpdateOpen();
    }
    public override bool Use()
    {
        base.Use();
        IsOpen = !IsOpen;
        return false;
    }

    private void UpdateOpen()
    {
        animator.SetBool("Open", isOpen);
        if (isOpen)
        {
            forPlayer = "Close";
        }
        else
        {
            forPlayer = "Open";
        }
        eventName?.Raise(forPlayer);
    }

}
