using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ConsoleVariableAttribute : Attribute
{
    public string Name { get; set; }

    public ConsoleVariableAttribute([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
    {
        Name = memberName;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ConsoleCategoryAttribute : Attribute
{
    public string Category { get; set; }

    public ConsoleCategoryAttribute(string category = null)
    {
        Category = category;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ConsoleFunctionAttribute : Attribute
{
    public string Name { get; private set; }
    public Type[] ArgumentTypes { get; set; }

    public ConsoleFunctionAttribute([System.Runtime.CompilerServices.CallerMemberName] string name = "")
    {
        Name = name;
    }
}