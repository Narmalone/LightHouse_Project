using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boot/Sequence")]
public class BootSequence : ScriptableObject
{
    public List<BootStep> steps;
}