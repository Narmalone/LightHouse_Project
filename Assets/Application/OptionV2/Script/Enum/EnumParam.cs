using UnityEngine;

[CreateAssetMenu(menuName = "Enum Wrappers/Enum")]
public class EnumParam : EnumWrapper
{
    public enum Quality { Low, Medium, High, VeryHigh, Epic }

    [SerializeField] private Quality current;

    public Quality Current => current;

    public override string[] GetNames() => System.Enum.GetNames(typeof(Quality));
    public override int GetCount() => System.Enum.GetValues(typeof(Quality)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => current = (Quality)index;
    public override int GetIndex() => (int)current;
}