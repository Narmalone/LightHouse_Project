using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JerricanEssence : ItemBase
{
    public override string Name => "Jerrican";
    [SerializeField, Tooltip("Percentage of giving")] private float essenceValue = 100f;
}
