using System;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    string Name { get; }
    List<IOption> GetOptions();
}

public abstract class ItemBase : MonoBehaviour, IItem
{
    public virtual string Name { get; }
    public virtual List<IOption> GetOptions() { return new List<IOption>(); }
}

public interface IOption
{
    string Name { get; }
}

public abstract class OptionBase : IOption
{
    public virtual string Name { get; }
}

public class GrabOptionBase : OptionBase
{
    public override string Name { get; } = "Grab";
}

public class HoldOptionBase : OptionBase
{
    public override string Name { get; } = "Hold";
}

public class UseOptionBase : OptionBase
{
    public override string Name { get; } = "Use";
    public Action UseAction { get; set; }
}